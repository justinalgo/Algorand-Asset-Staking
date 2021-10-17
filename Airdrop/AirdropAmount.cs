namespace Airdrop
{
    public class AirdropAmount
    {
        public string Wallet { get; set; }
        public long Amount { get; set; }

        public AirdropAmount(string wallet, long amount)
        {
            this.Wallet = wallet;
            this.Amount = amount;
        }
    }
}
