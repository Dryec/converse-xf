using System;
using Client;
using Converse.Helpers;

namespace Converse.Services
{
    public class TronConnection : ITronConnection
    {

        public WalletClient WalletClient { get; }
        public WalletSolidityClient WalletSolidityClient { get; }
        public WalletExtensionClient WalletExtentionClient { get; }

        public TronConnection()
        {
            var fullNode = AppConstants.DefaultFullNodeIP
                                       + ":"
                                       + AppConstants.DefaultFullNodePort.ToString();
            var solidityNode = AppConstants.DefaultSolidityNodeIP
                                           + ":"
                                           + AppConstants.DefaultSolidityNodePort.ToString();
            
            WalletClient = new WalletClient(fullNode);
            WalletSolidityClient = new WalletSolidityClient(solidityNode);
            WalletExtentionClient = new WalletExtensionClient(solidityNode);
        }
    }
}
