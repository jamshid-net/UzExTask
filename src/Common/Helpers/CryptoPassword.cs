using System.Security.Cryptography;
using System.Text;

namespace ProjectTemplate.Shared.Helpers;

public static class CryptoPassword
{
    public const int SaltSize = 32;
    public const int HashSize = 64;
    public const int Iterations = 1000;

    public class HashSalt
    {
        public required string Hash { get; set; }
        public required string Salt { get; set; }
    }

    public static HashSalt CreateHashSalted(string password)
    {

        var saltBytes = new byte[SaltSize];
        RandomNumberGenerator.Create().GetNonZeroBytes(saltBytes);

        var salt = ByteArrayToString(saltBytes);
        byte[] byteValue = Encoding.UTF8.GetBytes(salt);

        using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, byteValue, Iterations, HashAlgorithmName.SHA256);
        var hashPassword = ByteArrayToString(rfc2898DeriveBytes.GetBytes(HashSize));
        var hashSalt = new HashSalt { Hash = hashPassword, Salt = salt };
        return hashSalt;
    }

    public static string GetHashSalted(string password, string salt)
    {
        byte[] saltBytes = Encoding.Default.GetBytes(salt);
        using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var hashPassword = ByteArrayToString(rfc2898DeriveBytes.GetBytes(HashSize));
        return hashPassword;
    }

    private static string ByteArrayToString(IReadOnlyCollection<byte> ba)
    {
        StringBuilder hex = new(ba.Count * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }

    private static byte[] StringToByteArray(string hex)
    {
        int numberChars = hex.Length;
        byte[] bytes = new byte[numberChars / 2];
        for (int i = 0; i < numberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

}
