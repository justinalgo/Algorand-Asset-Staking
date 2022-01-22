namespace Airdrop
{
    public class AirdropAmount
    {
        public string Wallet { get; set; }
        public ulong Amount { get; set; }
        public ulong AssetId { get; set; }

        public AirdropAmount(string wallet, ulong assetId, ulong amount)
        {
            this.Wallet = wallet;
            this.AssetId = assetId;
            this.Amount = amount;
        }
    }
}
