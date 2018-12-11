using System.Windows.Input;
using Xamarin.Forms;

namespace Converse.Views
{
    public partial class ChatsOverviewTitleBarView : ContentView
    {
        public static readonly BindableProperty CanGoBackProperty =
            BindableProperty.Create(nameof(CanGoBack), typeof(bool), typeof(ChatsOverviewTitleBarView), null);

        public static readonly BindableProperty AddCommandProperty =
            BindableProperty.Create(nameof(AddCommand), typeof(ICommand), typeof(ChatsOverviewTitleBarView), null);

        public ChatsOverviewTitleBarView()
        {
            InitializeComponent();
        }


        public bool CanGoBack
        {
            get => (bool)GetValue(CanGoBackProperty);
            set => SetValue(CanGoBackProperty, value);
        }

        public ICommand AddCommand
        {
            get => (ICommand)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }
    }
}
