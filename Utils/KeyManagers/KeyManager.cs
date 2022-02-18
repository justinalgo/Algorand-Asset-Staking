using Microsoft.Extensions.Configuration;

namespace Utils.KeyManagers
{
    public class KeyManager : IKeyManager
    {
        public Key CavernaWallet { get; }
        public Key ShrimpWallet { get; }
        public Key RugWallet { get; }

        public KeyManager(IConfiguration config)
        {
            this.CavernaWallet = new Key(config["airdropMnemonic"]);
            this.ShrimpWallet = new Key(config["lingLingMnemonic"]);
            this.RugWallet = new Key(config["RugMnemonic"]);
        }
    }
}
