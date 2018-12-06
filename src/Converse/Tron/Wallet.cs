using System;
using Client;
using Common;
using Crypto;
using Google.Protobuf;
using Protocol;

namespace Converse.Tron
{
    public class Wallet
    {
        public ECKey ECKey { get; }

        string _privateKey;
        public string PrivateKey
        {
            get
            {
                if (_privateKey != null)
                {
                    return _privateKey;
                }
                else if (ECKey != null)
                {
                    _privateKey = ECKey.GetPrivateKey().ToHexString();
                    return _privateKey;
                }

                return string.Empty;
            }
            set => _privateKey = value;
        }

        public string MnemonicSentence { get; set; }

        string _address;
        public string Address
        {
            get
            {
                if (_address != null)
                {
                    return _address;
                }
                else if (ECKey?.Pub != null)
                {
                    _address = new WalletAddress(ECKey).ToString();
                    return _address;
                }
                else
                {
                    return string.Empty;
                }
            }

            set => _address = value;
        }

        public Wallet()
        {
            ECKey = new ECKey();
        }

        public Wallet(ECKey eCKey, string mnenomic = null)
        {
            ECKey = eCKey;
            MnemonicSentence = mnenomic;
        }

        public void SignTransaction(Transaction transaction, bool setTimestamp = true)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }
            if (ECKey == null)
            {
                throw new NullReferenceException("No valid ECKey");
            }

            if (setTimestamp)
            {
                transaction.SetTimestampToNow();
            }

            var hash = Sha256.Hash(transaction.RawData.ToByteArray());
            var signature = ECKey.Sign(hash);

            var contracts = transaction.RawData.Contract;
            foreach (var contract in contracts)
            {
                transaction.Signature.Add(ByteString.CopyFrom(signature));
            }
        }
    }
}
