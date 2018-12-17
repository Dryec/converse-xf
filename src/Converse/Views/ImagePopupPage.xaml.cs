using Rg.Plugins.Popup.Pages;

namespace Converse.Views
{
    public partial class ImagePopupPage : PopupPage
    {
        public ImagePopupPage()
        {
            InitializeComponent();
        }

        // Prevent hide popup
        protected override bool OnBackButtonPressed() => false;
    }
}
