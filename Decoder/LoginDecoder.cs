using System.Text;

namespace Noskito.Network.Decoder;

public class LoginDecoder : IDecoder
{
    public IEnumerable<string> Decode(byte[] value)
    {
        var packet = new StringBuilder();

        for (var i = 0; i < value.Length; i++)
        {
            packet.Append(Convert.ToChar(value[i] - 15));
        }

        packet.Remove(packet.Length - 1, 1);
        return new[] { packet.ToString() };
    }
}