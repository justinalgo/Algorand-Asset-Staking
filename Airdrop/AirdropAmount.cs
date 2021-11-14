namespace Airdrop
{
    public class AirdropAmount
    {
        public string Wallet { get; set; }
        public long Amount { get; set; }
        public long AssetId { get; set; }

        public AirdropAmount(string wallet, long assetId, long amount)
        {
            this.Wallet = wallet;
            this.AssetId = assetId;
            this.Amount = amount;
        }
    }
}
