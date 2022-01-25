namespace Noskito.Network.Decoder;

public class WorldDecoder : IDecoder
{
    private static readonly char[] Keys = { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n', };

    public IEnumerable<string> Decode(byte[] bytes)
    {
        var index = 0;
        var output = new List<string>();
        var currentPacket = string.Empty;
        var size = bytes.Length;
        
        while (index < size)
        {
            var currentByte = bytes[index++];
            if (currentByte == 0xFF)
            {
                output.Add(currentPacket.Trim());
                currentPacket = string.Empty;
                continue;
            }

            var length = currentByte & 0x7F;
            if ((currentByte & 0x80) != 0)
            {
                while (length != 0)
                {
                    if (index <= size)
                    {
                        currentByte = bytes[index++];

                        var firstIndex = ((currentByte & 0xF0U) >> 4) - 1;
                        if (firstIndex < Keys.Length)
                        {
                            var c = Keys[firstIndex];
                            if (c != 0x6E)
                            {
                                currentPacket += c;
                            }
                        }

                        if (length <= 1)
                        {
                            break;
                        }

                        var secondIndex = (currentByte & 0xFU) - 1;
                        if (secondIndex < Keys.Length)
                        {
                            var c = Keys[secondIndex];
                            if (c != 0x6E)
                            {
                                currentPacket += c;
                            }
                        }

                        length -= 2;
                    }
                    else
                    {
                        length--;
                    }
                }
            }
            else
            {
                while (length != 0)
                {
                    if (index <= size)
                    {
                        currentPacket += (char)(bytes[index] ^ 0xFF);
                        index++;
                    }

                    length--;
                }
            }
        }
        return output;
    }
    
}