using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitcoin.BIP39;
using Client;
using Common;
using Converse.Helpers;
using Converse.Services;
using Crypto;
using Google.Protobuf;
using Protocol;

namespace Converse.Tron
{
    public class Wallet : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ECKey ECKey { get; }
        public string Mnemonic { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ProfileImageUrl { get; set; }

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

        public byte[] PublicKey => ECKey?.Pub?.GetEncoded();

        public Wallet()
        {
            ECKey = new ECKey();
        }

        public Wallet(ECKey eCKey)
        {
            ECKey = eCKey;
        }

        public Wallet(string mnemonic)
        {
            ECKey = new ECKey(BIP39.GetSeedBytes(mnemonic).Take(32).ToArray());
            Mnemonic = mnemonic;
        }

        public async Task<long> GetConverseTokenAmountAsync(TronConnection connection)
        {
            try
            {
                var account = await connection.Client.GetAccountAsync(new Account { Address = ByteString.CopyFrom(WalletAddress.Decode58Check(Address)) });
                return account.Asset.ContainsKey(AppConstants.TokenName) ? account.Asset[AppConstants.TokenName] : 0;
            }
            catch (Exception)
            {
                return 0;
            }
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

        public byte[] Encrypt(string message, byte[] publicKey)
        {
            return ECKey.Encrypt(Encoding.UTF8.GetBytes(message), publicKey);
        }

        public string Decrypt(byte[] encryptedMessage, byte[] publicKey)
        {
            var decryptedBytes = ECKey.Decrypt(encryptedMessage, publicKey);

            if(decryptedBytes != null)
            {
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            return string.Empty;
        }
    }
}
