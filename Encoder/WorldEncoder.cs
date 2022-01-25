namespace Noskito.Network.Encoder;

public class WorldEncoder : IEncoder
{
    private readonly int encryptionKey;
    private bool session;
    
    public WorldEncoder(int encryptionKey)
    {
        this.encryptionKey = encryptionKey;
        this.session = true;
    }

    public byte[] Encode(string packet)
    {
        var encoded = new List<byte>();

        var mask = new string(packet.Select(c =>
        {
            var b = (sbyte)c;
            if (c == '#' || c == '/' || c == '%')
            {
                return '0';
            }

            if ((b -= 0x20) == 0 || (b += unchecked((sbyte)0xF1)) < 0 || (b -= 0xB) < 0 ||
                (b - unchecked((sbyte)0xC5)) == 0)
            {
                return '1';
            }

            return '0';
        }).ToArray());

        var packetLength = packet.Length;

        var sequenceCounter = 0;
        var currentPosition = 0;

        while (currentPosition <= packetLength)
        {
            var lastPosition = currentPosition;
            while (currentPosition < packetLength && mask[currentPosition] == '0')
            {
                currentPosition++;
            }

            int sequences;
            int length;

            if (currentPosition != 0)
            {
                length = currentPosition - lastPosition;
                sequences = length / 0x7E;
                for (var i = 0; i < length; i++, lastPosition++)
                {
                    if (i == (sequenceCounter * 0x7E))
                    {
                        if (sequences == 0)
                        {
                            encoded.Add((byte)(length - i));
                        }
                        else
                        {
                            encoded.Add(0x7E);
                            sequences--;
                            sequenceCounter++;
                        }
                    }

                    encoded.Add((byte)((byte)packet[lastPosition] ^ 0xFF));
                }
            }

            if (currentPosition >= packetLength)
            {
                break;
            }

            lastPosition = currentPosition;
            while (currentPosition < packetLength && mask[currentPosition] == '1')
            {
                currentPosition++;
            }

            if (currentPosition == 0)
            {
                continue;
            }

            length = currentPosition - lastPosition;
            sequences = length / 0x7E;
            for (var i = 0; i < length; i++, lastPosition++)
            {
                if (i == (sequenceCounter * 0x7E))
                {
                    if (sequences == 0)
                    {
                        encoded.Add((byte)(length - i | 0x80));
                    }
                    else
                    {
                        encoded.Add(0x7E | 0x80);
                        sequences--;
                        sequenceCounter++;
                    }
                }

                var currentByte = (byte)packet[lastPosition];
                switch (currentByte)
                {
                    case 0x20:
                        currentByte = 1;
                        break;
                    case 0x2D:
                        currentByte = 2;
                        break;
                    case 0xFF:
                        currentByte = 0xE;
                        break;
                    default:
                        currentByte -= 0x2C;
                        break;
                }

                if (currentByte == 0x00)
                {
                    continue;
                }

                if ((i % 2) == 0)
                {
                    encoded.Add((byte)(currentByte << 4));
                }
                else
                {
                    encoded[^1] = (byte)(encoded.Last() | currentByte);
                }
            }
        }

        encoded.Add(0xFF);

        var sessionNumber = (sbyte)(encryptionKey >> 6 & 0xFF & 0x80000003);

        if (sessionNumber < 0)
        {
            sessionNumber = (sbyte)((sessionNumber - 1 | 0xFFFFFFFC) + 1);
        }

        var sessionKey = (byte)(encryptionKey & 0xFF);

        if (session)
        {
            sessionNumber = -1;
            session = false;
        }

        switch (sessionNumber)
        {
            case 0:
                for (var i = 0; i < encoded.Count; i++)
                {
                    encoded[i] = (byte)(encoded[i] + sessionKey + 0x40);
                }

                break;
            case 1:
                for (var i = 0; i < encoded.Count; i++)
                {
                    encoded[i] = (byte)(encoded[i] - (sessionKey + 0x40));
                }

                break;
            case 2:
                for (var i = 0; i < encoded.Count; i++)
                {
                    encoded[i] = (byte)((encoded[i] ^ 0xC3) + sessionKey + 0x40);
                }

                break;
            case 3:
                for (var i = 0; i < encoded.Count; i++)
                {
                    encoded[i] = (byte)((encoded[i] ^ 0xC3) - (sessionKey + 0x40));
                }

                break;
            default:
                for (var i = 0; i < encoded.Count; i++)
                {
                    encoded[i] = (byte)(encoded[i] + 0x0F);
                }

                break;
        }

        return encoded.ToArray();
    }
}