using System;
using System.Linq;
using System.Threading.Tasks;
using Bitcoin.BIP39;
using Converse.Helpers;
using Crypto;

namespace Converse.Tron
{
    public class WalletManager
    {
        public Wallet Wallet { get; private set; }

        public async Task<bool> LoadWalletAsync()
        {
            var mnemonic = await Xamarin.Essentials.SecureStorage.GetAsync(AppConstants.Keys.User.Mnemonic);

            if (mnemonic == null)
            {
                return false;
            }

            var ecKey = new ECKey(BIP39.GetSeedBytes(mnemonic).Take(32).ToArray());
            Wallet = new Wallet(ecKey, mnemonic);
            return true;
        }

        public async Task<Wallet> CreateNewWalletAsync(bool save = false)
        {
            var bip39 = new BIP39();
            var mnemonic = bip39.MnemonicSentence;
            var eCKey = new ECKey(bip39.SeedBytes.Take(32).ToArray());

            if (save)
            {
                await Xamarin.Essentials.SecureStorage.SetAsync(AppConstants.Keys.User.Mnemonic, mnemonic);
            }
            Wallet = new Wallet(eCKey, mnemonic);
            return Wallet;
        }

        public async Task<bool> SaveAsync()
        {
            if (Wallet == null || string.IsNullOrWhiteSpace(Wallet.MnemonicSentence))
            {
                return false;
            }

            await Xamarin.Essentials.SecureStorage.SetAsync(AppConstants.Keys.User.Mnemonic, Wallet.MnemonicSentence);
            return true;
        }
    }
}
