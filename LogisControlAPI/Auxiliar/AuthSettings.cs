namespace LogisControlAPI.Auxiliar;
using System.Security.Cryptography;
public static class AuthSettings
{
    //This key must be preserved outside the code
    public static string PrivateKey { get; set; } = GeraKey(256);

    public static string GeraKey(int nBytes)
    {
        Aes aesAlgorithm = Aes.Create();
        aesAlgorithm.KeySize = 256;
        aesAlgorithm.GenerateKey();
        string keyBase64 = Convert.ToBase64String(aesAlgorithm.Key);
        return keyBase64;
    }
}
