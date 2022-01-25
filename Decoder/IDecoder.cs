namespace Noskito.Network.Decoder;

public interface IDecoder
{
    IEnumerable<string> Decode(byte[] value);
}