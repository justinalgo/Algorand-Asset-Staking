namespace Utils.KeyManagers
{
    public interface IKeyManager
    {
        Key CavernaWallet { get; }
        Key RugWallet { get; }
        Key ShrimpWallet { get; }
    }
}