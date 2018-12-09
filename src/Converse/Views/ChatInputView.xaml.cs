using System.Windows.Input;
using Xamarin.Forms;

namespace Converse.Views
{
    public partial class ChatInputView : ContentView
    {
        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(nameof(Message), typeof(string), typeof(ChatInputView), "", BindingMode.TwoWay);

        public static readonly BindableProperty SendCommandProperty =
            BindableProperty.Create(nameof(SendCommand), typeof(ICommand), typeof(ChatInputView), null);

        public ChatInputView()
        {
            InitializeComponent();
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public ICommand SendCommand
        {
            get => (ICommand)GetValue(SendCommandProperty);
            set => SetValue(SendCommandProperty, value);
        }
    }
}
