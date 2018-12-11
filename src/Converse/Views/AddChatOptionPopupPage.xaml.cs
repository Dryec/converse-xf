using Rg.Plugins.Popup.Pages;

namespace Converse.Views
{
    public partial class AddChatOptionPopupPage : PopupPage
    {
        public AddChatOptionPopupPage()
        {
            InitializeComponent();
        }

        // Prevent hide popup
        protected override bool OnBackButtonPressed() => false;
    }
}
