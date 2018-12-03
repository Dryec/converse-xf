using Xamarin.Forms;

namespace Converse.Views
{
    public partial class ChatInputView : ContentView
    {
        public static readonly BindableProperty FooProperty =
            BindableProperty.Create(nameof(Foo), typeof(string), typeof(ChatInputView), null);

        public ChatInputView()
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
