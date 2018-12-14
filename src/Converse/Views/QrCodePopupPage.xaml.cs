using Rg.Plugins.Popup.Pages;

namespace Converse.Views
{
    public partial class QrCodePopupPage : PopupPage
    {
        public QrCodePopupPage()
        {
            InitializeComponent();
        }

        // Prevent hide popup
        protected override bool OnBackButtonPressed() => false;
    }
}
