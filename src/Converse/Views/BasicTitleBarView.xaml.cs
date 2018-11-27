using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Converse.Views
{
    public partial class BasicTitleBarView : ContentView
    {
        public static readonly BindableProperty CanGoBackProperty =
            BindableProperty.Create(nameof(CanGoBack), typeof(bool), typeof(BasicTitleBarView), null);

        public BasicTitleBarView()
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
