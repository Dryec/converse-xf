using System;
using System.Linq;
using System.Threading.Tasks;
using Bitcoin.BIP39;
using Client;
using Converse.Helpers;
using Converse.Tron;
using Crypto;

namespace Converse.Services
{
    public class TronConnection
    {
        public WalletClient Client { get; private set; }
        public WalletSolidityClient SolidityClient { get; private set; }
        public WalletExtensionClient ExtentionClient { get; private set; }

        public TronConnection()
        {

        }

        public void Connect()
        {
            var fullNode = AppConstants.DefaultFullNodeIP
                                       + ":"
                                       + AppConstants.DefaultFullNodePort.ToString();
            var solidityNode = AppConstants.DefaultSolidityNodeIP
                                           + ":"
                                           + AppConstants.DefaultSolidityNodePort.ToString();

            Client = new WalletClient(fullNode);
            SolidityClient = new WalletSolidityClient(solidityNode);
            ExtentionClient = new WalletExtensionClient(solidityNode);
        }

    }
}
