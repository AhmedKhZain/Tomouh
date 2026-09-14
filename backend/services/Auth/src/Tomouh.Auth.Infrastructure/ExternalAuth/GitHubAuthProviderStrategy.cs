using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Application.Interfaces;
using Tomouh.Auth.Infrastructure.Options;

namespace Tomouh.Auth.Infrastructure.ExternalAuth;

public class GitHubAuthProviderStrategy : IExternalAuthProviderStrategy
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ProviderSettings _gitHubSettings;

    public GitHubAuthProviderStrategy(
        IHttpClientFactory httpClientFactory,
        IOptions<ExternalAuthSettings> externalAuthOptions)
    {
        _httpClientFactory = httpClientFactory;
        _gitHubSettings = externalAuthOptions.Value.GitHub;
    }

    public AuthProvider Provider => AuthProvider.GitHub;

    public async Task<ExternalAuthPayload> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("GitHubAuth");

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("TomouhApp", "1.0"));

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var gitHubUser = await response.Content.ReadFromJsonAsync<GitHubUserResponse>(cancellationToken: cancellationToken);

        if (gitHubUser is null)
            throw new InvalidOperationException("Failed to deserialize GitHub user response.");

        var email = gitHubUser.Email;
        if (string.IsNullOrEmpty(email))
        {
            email = await FetchPrimaryEmailAsync(client, token, cancellationToken);
        }

        var (firstName, lastName) = SplitName(gitHubUser.Name ?? gitHubUser.Login);

        return new ExternalAuthPayload(
            SubjectId: gitHubUser.Id.ToString(),
            Email: email,
            FirstName: firstName,
            LastName: lastName,
            PictureUrl: gitHubUser.AvatarUrl,
            Name: gitHubUser.Name ?? gitHubUser.Login
        );
    }

    private async Task<string> FetchPrimaryEmailAsync(HttpClient client, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("TomouhApp", "1.0"));

        var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return string.Empty;

        var emails = await response.Content.ReadFromJsonAsync<List<GitHubEmailResponse>>(cancellationToken: cancellationToken);
        return emails?.FirstOrDefault(e => e.Primary && e.Verified)?.Email ?? emails?.FirstOrDefault()?.Email ?? string.Empty;
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return (string.Empty, string.Empty);
        var parts = fullName.Trim().Split(' ', 2);
        return (parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }

    private record GitHubUserResponse(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("login")] string Login,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("avatar_url")] string? AvatarUrl);

    private record GitHubEmailResponse(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("primary")] bool Primary,
        [property: JsonPropertyName("verified")] bool Verified);
}