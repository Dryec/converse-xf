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

            Wallet = new Wallet(mnemonic)
            {
                Mnemonic = mnemonic,
                Name = await Xamarin.Essentials.SecureStorage.GetAsync(AppConstants.Keys.User.Name),
                Email = await Xamarin.Essentials.SecureStorage.GetAsync(AppConstants.Keys.User.Email),
                ProfileImageUrl = await Xamarin.Essentials.SecureStorage.GetAsync(AppConstants.Keys.User.ProfileImageUrl)
            };
            return true;
        }

        public Wallet CreateNewWalletAsync()
        {
            var bip39 = new BIP39();
            var mnemonic = bip39.MnemonicSentence;
            var eCKey = new ECKey(bip39.SeedBytes.Take(32).ToArray());

            Wallet = new Wallet(mnemonic);
            return Wallet;
        }

        public async Task<bool> SaveAsync()
        {
            if (Wallet == null || string.IsNullOrWhiteSpace(Wallet.Mnemonic))
            {
                return false;
            }

            await Xamarin.Essentials.SecureStorage.SetAsync(AppConstants.Keys.User.Mnemonic, Wallet.Mnemonic);
            await Xamarin.Essentials.SecureStorage.SetAsync(AppConstants.Keys.User.Name, Wallet.Name ?? string.Empty);
            await Xamarin.Essentials.SecureStorage.SetAsync(AppConstants.Keys.User.Email, Wallet.Email ?? string.Empty);
            await Xamarin.Essentials.SecureStorage.SetAsync(AppConstants.Keys.User.ProfileImageUrl, Wallet.ProfileImageUrl ?? string.Empty);
            return true;
        }
    }
}
