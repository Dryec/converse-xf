using Rg.Plugins.Popup.Pages;

namespace Converse.Views
{
    public partial class UpdateUserPopupPage : PopupPage
    {
        public UpdateUserPopupPage()
        {
            InitializeComponent();
        }

        // Prevent hide popup
        protected override bool OnBackButtonPressed() => true;
    }
}
