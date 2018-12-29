using Rg.Plugins.Popup.Pages;

namespace Converse.Views
{
    public partial class GroupPopupPage : PopupPage
    {
        public GroupPopupPage()
        {
            InitializeComponent();
        }

        // Prevent hide popup
        protected override bool OnBackButtonPressed() => false;
    }
}
