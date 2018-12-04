using System;
using Client;

namespace Converse.Services
{
    public interface ITronConnection
    {
        WalletClient WalletClient { get;  }
        WalletSolidityClient WalletSolidityClient { get;  }
        WalletExtensionClient WalletExtentionClient { get;  }
    }
}
