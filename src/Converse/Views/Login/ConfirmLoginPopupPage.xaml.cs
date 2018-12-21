using Rg.Plugins.Popup.Pages;

namespace Converse.Views.Login
{
    public partial class ConfirmLoginPopupPage : PopupPage
    {
        public ConfirmLoginPopupPage()
        {
            InitializeComponent();
        }

        // Prevent hide popup
        protected override bool OnBackButtonPressed() => false;
    }
}
