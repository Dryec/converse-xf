using System.Diagnostics;
using System.Threading.Tasks;
using Converse.ViewModels;
using Syncfusion.ListView.XForms;
using Syncfusion.ListView.XForms.Control.Helpers;
using Xamarin.Forms;

namespace Converse.Views
{
    public partial class ChatPage
    {
        VisualContainer _visualContainer { get; }
        ChatPageViewModel _viewModel { get; }
        ExtendedScrollView _scrollView { get; }

        public ChatPage()
        {
            InitializeComponent();

            ChatMessagesListView.LayoutManager.ItemsCacheLimit = 0;

            _viewModel = (BindingContext as ChatPageViewModel);
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.ScrollMessagesEvent += Handle_ScrollMessagesEvent;
            _visualContainer = ChatMessagesListView.GetVisualContainer();

            _scrollView = ChatMessagesListView.GetScrollView();
            _scrollView.SetBinding(Xamarin.Forms.ScrollView.ContentSizeProperty, "ScrollViewSize");
            _scrollView.SetBinding(Xamarin.Forms.ScrollView.ScrollYProperty, "ScrollY");
            ChatMessagesListView.SetBinding(HeightProperty, "ScrollViewHeight");

            ChatMessagesListView.Opacity = 0;
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ChatPageViewModel.Messages):
                    break;
                default:
                    break;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Workaround for the unexpected scrolling
            ChatMessagesListView.HeightRequest = ChatScrollView.Height - InputView.Height;
        }

        async void Handle_ScrollMessagesEvent(object sender, System.EventArgs e)
        {
            if (e is ScrollEventArgs args)
            {
                ChatMessagesListView.LayoutManager.ScrollToRowIndex(args.Index, args.ScrollPosition, !args.Animated);
                if(ChatMessagesListView.Opacity <= 0)
                {
                    await Task.Delay(25);
                    await ChatMessagesListView.FadeTo(1, 100, Easing.CubicInOut);
                }
            }
        }
    }
}
