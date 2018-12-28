using Converse.Models;
using Rg.Plugins.Popup.Pages;

namespace Converse.Views
{
    public partial class SelectUserPopupPage : PopupPage
    {
        public SelectUserPopupPage()
        {
            InitializeComponent();
        }

        // Prevent hide popup
        protected override bool OnBackButtonPressed() => false;

        void Handle_ItemTapped(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            e.Handled = true;
            if (e.ItemData is UserInfo user)
            {
                AddressEntry.Text = user.TronAddress;
            }
        }
    }
}
