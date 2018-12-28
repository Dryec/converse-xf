using Xamarin.Forms;

namespace Converse.Views.Chat
{
    public partial class LeftChatMessageViewCell
    {
        public static readonly BindableProperty ShowNameProperty =
            BindableProperty.Create(nameof(ShowName), typeof(bool), typeof(LeftChatMessageViewCell), true);

        public LeftChatMessageViewCell()
        {
            InitializeComponent();
        }

        public bool ShowName
        {
            get => (bool)GetValue(ShowNameProperty);
            set => SetValue(ShowNameProperty, value);
        }

    }
}
