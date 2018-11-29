using Xamarin.Forms;

namespace Converse.Views.Chat
{
    public partial class NormalChatViewCell : ViewCell
    {
        public static readonly BindableProperty FooProperty =
            BindableProperty.Create(nameof(Foo), typeof(string), typeof(NormalChatViewCell), null);

        public NormalChatViewCell()
        {
            InitializeComponent();
        }

        public string Foo
        {
            get => (string)GetValue(FooProperty);
            set => SetValue(FooProperty, value);
        }
    }
}
