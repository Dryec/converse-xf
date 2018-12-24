using Rg.Plugins.Popup.Pages;

namespace Converse.Views
{
    public partial class CreateGroupPopupPage : PopupPage
    {
        public CreateGroupPopupPage()
        {
            InitializeComponent();
        }

        // Prevent hide popup
        protected override bool OnBackButtonPressed() => true;
    }
}
