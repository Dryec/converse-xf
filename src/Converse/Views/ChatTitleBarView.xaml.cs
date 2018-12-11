using Xamarin.Forms;

namespace Converse.Views
{
    public partial class ChatTitleBarView : ContentView
    {
        public static readonly BindableProperty CanGoBackProperty =
            BindableProperty.Create(nameof(CanGoBack), typeof(bool), typeof(ChatTitleBarView), null);

        public ChatTitleBarView()
        {
            InitializeComponent();
        }

        public bool CanGoBack
        {
            get => (bool)GetValue(CanGoBackProperty);
            set => SetValue(CanGoBackProperty, value);
        }
    }
}
