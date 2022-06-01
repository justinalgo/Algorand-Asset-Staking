using Algorand.V2.Algod.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class AlvaHoldingsFactory : HoldingsAirdropFactory
    {
        public AlvaHoldingsFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils) : base(indexerUtils, algodUtils)
        {
            this.DropAssetId = 553615859;
            this.Decimals = 2;
            this.CreatorAddresses = new string[] { "ALVA7QT5JWKXMWGNYL3JYFTCFFCYJVUIFZAD4S7AKFW5M7OI6Q7X3EAGFY" };
            this.RevokedAddresses = new string[] { "ALVA7QT5JWKXMWGNYL3JYFTCFFCYJVUIFZAD4S7AKFW5M7OI6Q7X3EAGFY",
                                                   "BKOSO3RMXW6XIB7TQWOPIEUSW2Q4PY2DKCMQO433WHLKFV22ZXEPQGQDKU" };
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            Dictionary<int, ulong> powerToValue = new Dictionary<int, ulong>();
            powerToValue[11] = 200;
            powerToValue[12] = 300;
            powerToValue[13] = 800;
            powerToValue[14] = 2000;
            powerToValue[15] = 4500;
            powerToValue[16] = 10000;
            powerToValue[17] = 22500;
            powerToValue[18] = 47500;


            ulong[] endingDigits;
            DateTime t = DateTime.Now;
            if (t.DayOfWeek == DayOfWeek.Sunday)
            {
                endingDigits = new ulong[] { 0, 1 };
            } 
            else if (t.DayOfWeek == DayOfWeek.Tuesday)
            {
                endingDigits = new ulong[] { 2, 3, 4 };
            }
            else if (t.DayOfWeek == DayOfWeek.Thursday)
            {
                endingDigits = new ulong[] { 5, 6, 7 };
            }
            else if (t.DayOfWeek == DayOfWeek.Saturday)
            {
                endingDigits = new ulong[] { 8, 9 };
            }
            else
            {
                endingDigits = new ulong[] { };
            }


            Dictionary<ulong, ulong> assetValues = new Dictionary<ulong, ulong>();

            var txns = await IndexerUtils.GetTransactions(this.CreatorAddresses[0], txType: Algorand.V2.Indexer.Model.TxType.Acfg);
            
            foreach (var txn in txns)
            {
                if (txn?.CreatedAssetIndex != null)
                {
                    ulong id = (ulong)txn.CreatedAssetIndex;

                    if (endingDigits.Contains(id % 10))
                    {
                        dynamic obj = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(txn.Note));
                        try
                        {
                            if (obj.standard == "arc69")
                            {
                                int power = (int)obj.properties.Power;
                                if (power > 10)
                                {
                                    assetValues[id] = powerToValue[power];
                                }
                            }
                            else
                            {
                                Console.WriteLine("Failed on " + id);
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Failed on " + id);
                        }
                    }

                }
            }

            return assetValues;
        }

        public override void AddAssetsInAccount(AirdropUnitCollectionManager collectionManager, Account account, IDictionary<ulong, ulong> assetValues)
        {
            IEnumerable<AssetHolding> assetHoldings = account.Assets;

            if (assetHoldings != null)
            {
                foreach (AssetHolding asset in assetHoldings)
                {
                    ulong sourceAssetId = asset.AssetId;
                    ulong numberOfSourceAsset = asset.Amount;

                    if (sourceAssetId == 557939659 && numberOfSourceAsset > 0)
                    {
                        collectionManager.AddModifier(new AirdropUnitCollectionModifier(
                            account.Address,
                            this.DropAssetId,
                            sourceAssetId,
                            .025,
                            numberOfSourceAsset: numberOfSourceAsset));
                    }
                    else if (assetValues.ContainsKey(sourceAssetId) && numberOfSourceAsset > 0)
                    {
                        ulong assetValue = assetValues[sourceAssetId];
                        collectionManager.AddAirdropUnit(new AirdropUnit(
                            account.Address,
                            this.DropAssetId,
                            sourceAssetId,
                            assetValue,
                            numberOfSourceAsset: numberOfSourceAsset,
                            isMultiplied: true));
                    }
                }
            }
        }
    }
}
