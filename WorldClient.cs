using System.Net;
using System.Timers;
using Noskito.Network.Decoder;
using Noskito.Network.Encoder;
using Timer = System.Timers.Timer;

namespace Noskito.Network;

public class WorldClient : Client
{
    private int packetId;
    private int keepAliveId;

    private readonly Timer timer;
    
    public WorldClient(int encryptionKey) : base(new WorldEncoder(encryptionKey), new WorldDecoder(), 0xFF)
    {
        this.timer = new Timer(60000);
        this.timer.Elapsed += OnTick;
    }
    
    private void OnTick(object sender, ElapsedEventArgs e)
    {
        if (!IsConnected)
        {
            timer.Stop();
        }

        SendPacket($"pulse {++keepAliveId * 60} 1");
    }

    public override void SendPacket(string packet)
    {
        base.SendPacket($"{packetId++} {packet}");
    }

    public override bool Connect(IPEndPoint ip)
    {
        var connected = base.Connect(ip);
        if (!connected)
        {
            return false;
        }
        
        timer.Start();
        return true;
    }
}