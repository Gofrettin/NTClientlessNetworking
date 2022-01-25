using System.Net;
using System.Net.Sockets;
using Noskito.Network.Decoder;
using Noskito.Network.Encoder;

namespace Noskito.Network;

public abstract class Client
{
    private readonly Socket socket;
    private readonly Thread thread;
    private readonly IEncoder encoder;
    private readonly IDecoder decoder;
    private readonly byte lastByte;
    private readonly List<byte> cachedBuffer = new();

    public bool IsConnected => socket.Connected;

    public event Action<string> PacketReceived; 

    protected Client(IEncoder encoder, IDecoder decoder, byte lastByte)
    {
        this.encoder = encoder;
        this.decoder = decoder;
        this.lastByte = lastByte;
        this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        this.thread = new Thread(Loop);
    }

    private void Loop()
    {
        while (socket.Connected)
        {
            var buffer = new ArraySegment<byte>(new byte[socket.ReceiveBufferSize]);
            var size = socket.Receive(buffer);

            if (!socket.Connected)
            {
                break;
            }

            buffer = buffer[..size];

            if (buffer.Count == 0)
            {
                Disconnect();
                break;
            }

            if (buffer[^1] != lastByte)
            {
                cachedBuffer.AddRange(buffer);
                continue;
            }
            
            var bytes = cachedBuffer.Count > 0 ? cachedBuffer.Concat(buffer).ToArray() : buffer.ToArray();
            if (cachedBuffer.Count > 0)
            {
                cachedBuffer.Clear();
            }

            var decoded = decoder.Decode(bytes);
            foreach (var value in decoded)
            {
                PacketReceived?.Invoke(value);
            }
        }
    }

    public virtual void SendPacket(string packet)
    {
        var encoded = encoder.Encode(packet);
        if (encoded.Length == 0)
        {
            return;
        }

        socket.Send(encoded);
    }

    public virtual bool Connect(IPEndPoint ip)
    {
        socket.Connect(ip);
        if (!socket.Connected)
        {
            return false;
        }
        thread.Start();
        return true;
    }

    public void Disconnect()
    {
        socket.Close();
    }
}