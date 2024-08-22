using Org.BouncyCastle.Crypto.Digests;
using System.Text;

namespace CollectionManagement.Others;

public class SHA3_256
{
    public static async Task<string> ComputeSha3_256Hash(string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text);

        var sha3 = new Sha3Digest(256);

        sha3.BlockUpdate(data, 0, data.Length);
        byte[] result = new byte[sha3.GetDigestSize()];

        sha3.DoFinal(result, 0);

        StringBuilder sb = new(result.Length * 2);

        foreach (byte b in result)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}