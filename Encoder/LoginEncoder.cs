namespace Noskito.Network.Encoder;

public class LoginEncoder : IEncoder
{
    public byte[] Encode(string value)
    {
        var bytes = new byte[value.Length + 1];
        for (var i = 0; i < value.Length; i++)
        {
            bytes[i] = (byte)((value[i] ^ 195) + 15);
        }

        bytes[^1] = 0xD8;
        return bytes;
    }
}