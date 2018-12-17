using Rg.Plugins.Popup.Pages;

namespace Converse.Views
{
    public partial class UserPopupPage : PopupPage
    {
        public UserPopupPage()
        {
            InitializeComponent();
        }

        // Prevent hide popup
        protected override bool OnBackButtonPressed() => false;
    }
}
