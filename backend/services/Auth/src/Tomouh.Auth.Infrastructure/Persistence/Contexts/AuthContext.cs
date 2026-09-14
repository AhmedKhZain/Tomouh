using MongoDB.Driver;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Shared.Infrastructure.Features.Persistence.Contexts.Mongo;

namespace Tomouh.Auth.Infrastructure.Persistence.Contexts;

public class AuthContext(IMongoDatabase database) : MongoBaseContext(database)
{

    public string UsersCollectionName => GetCollectionName("Users");
    public string UserTokensCollectionName => GetCollectionName("UserTokens");

    public IMongoCollection<User> Users => Database.GetCollection<User>(UsersCollectionName);

    public IMongoCollection<UserToken> UserTokens => Database.GetCollection<UserToken>(UserTokensCollectionName);




}
