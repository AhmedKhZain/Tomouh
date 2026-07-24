namespace Common.Models;

public record UserIP(byte[] AddressBytes)
{
    public override string ToString()
    {
        if (AddressBytes == null || AddressBytes.Length == 0)
            return "0.0.0.0";

        if (AddressBytes.Length == 4)
        {
            return $"{AddressBytes[0]}.{AddressBytes[1]}.{AddressBytes[2]}.{AddressBytes[3]}";
        }

        var segments = new string[8];
        for (int i = 0; i < 8; i++)
        {
            segments[i] = ((AddressBytes[i * 2] << 8) + AddressBytes[i * 2 + 1]).ToString("x");
        }
        return string.Join(":", segments);
    }

    public static UserIP FromString(string value)
    {
        if (System.Net.IPAddress.TryParse(value, out var ipAddress))
        {
            return new UserIP(ipAddress.GetAddressBytes());
        }
        return new UserIP(new byte[] { 127, 0, 0, 1 });
    }
}