using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Algorand.V2.Algod;
using Algorand.V2;
using Algorand.V2.Algod.Model;
using System.Threading.Tasks;
using Algorand;
using Encoder = Algorand.Encoder;
using System.IO;

namespace Utils.Algod
{
    public class AlgodUtils : IAlgodUtils
    {
        private readonly ILogger<AlgodUtils> log;
        private readonly DefaultApi algod;

        public AlgodUtils(ILogger<AlgodUtils> log, IConfiguration config)
        {
            this.log = log;
            var httpClient = HttpClientConfigurator.ConfigureHttpClient(config["Endpoints:Algod"], config["AlgodToken"]);
            this.algod = new DefaultApi(httpClient) { BaseUrl = config["Endpoints:Algod"] };
        }

        public async Task<PostTransactionsResponse> SubmitTransaction(SignedTransaction signedTransaction)
        {
            byte[] encodedTxBytes = Encoder.EncodeToMsgPack(signedTransaction);
            using MemoryStream ms = new MemoryStream(encodedTxBytes);
            return await algod.TransactionsAsync(ms);
        }

        public async Task<NodeStatusResponse> GetStatusAfterRound(ulong round)
        {
            return await algod.WaitForBlockAfterAsync(round);
        }

        public async Task<NodeStatusResponse> GetStatus()
        {
            return await algod.StatusAsync();
        }

        public async Task<ulong> GetLastRound()
        {
            NodeStatusResponse status = await this.GetStatus();
            return status.LastRound;
        }
    }
}
