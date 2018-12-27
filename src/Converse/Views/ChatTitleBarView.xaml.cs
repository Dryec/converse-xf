using System.Windows.Input;
using Converse.Models;
using Xamarin.Forms;

namespace Converse.Views
{
    public partial class ChatTitleBarView : ContentView
    {
        public static readonly BindableProperty CanGoBackProperty =
            BindableProperty.Create(nameof(CanGoBack), typeof(bool), typeof(ChatTitleBarView), null);

        public static readonly BindableProperty ImageProperty =
            BindableProperty.Create(nameof(Image), typeof(string), typeof(ChatTitleBarView), null);

        public static readonly BindableProperty ImageTappedCommandProperty =
            BindableProperty.Create(nameof(ImageTappedCommand), typeof(ICommand), typeof(ChatTitleBarView), null);

        public static readonly BindableProperty ImageTappedCommandParameterProperty =
            BindableProperty.Create(nameof(ImageTappedCommandParameter), typeof(object), typeof(ChatTitleBarView), null);

        public ChatTitleBarView()
        {
            InitializeComponent();
        }

        public bool CanGoBack
        {
            get => (bool)GetValue(CanGoBackProperty);
            set => SetValue(CanGoBackProperty, value);
        }

        public ICommand ImageTappedCommand
        {
            get => (ICommand)GetValue(ImageTappedCommandProperty);
            set => SetValue(ImageTappedCommandProperty, value);
        }

        public object ImageTappedCommandParameter
        {
            get => GetValue(ImageTappedCommandParameterProperty);
            set => SetValue(ImageTappedCommandParameterProperty, value);
        }

        public string Image
        {
            get => (string)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
    }
}
