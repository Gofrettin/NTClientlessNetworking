using Noskito.Network.Decoder;
using Noskito.Network.Encoder;

namespace Noskito.Network;

public class LoginClient : Client
{
    public LoginClient() : base(new LoginEncoder(), new WorldDecoder(), 25)
    {
        
    }
}