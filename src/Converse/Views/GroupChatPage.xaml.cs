using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Converse.DataTemplateSelectors;
using Converse.Models;
using Converse.ViewModels;
using Syncfusion.ListView.XForms;
using Syncfusion.ListView.XForms.Control.Helpers;
using Xamarin.Forms;

namespace Converse.Views
{
    public partial class GroupChatPage
    {
        VisualContainer _visualContainer { get; }
        GroupChatPageViewModel _viewModel { get; }
        ExtendedScrollView _scrollView { get; }

        // Workaround for crash when closing page while listview tries to scroll
        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ChatMessageTemplateSelector TemplateSelector {get;set;}

        public GroupChatPage()
        {
            InitializeComponent(); 

            ChatMessagesListView.LayoutManager.ItemsCacheLimit = 2;

            _viewModel = (BindingContext as GroupChatPageViewModel);
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.ScrollMessagesEvent += Handle_ScrollMessagesEvent;

            _visualContainer = ChatMessagesListView.GetVisualContainer();
            _visualContainer.ScrollRows.Changed += ScrollRows_Changed;

            _scrollView = ChatMessagesListView.GetScrollView();
            _scrollView.SetBinding(Xamarin.Forms.ScrollView.ContentSizeProperty, "ScrollViewSize");
            _scrollView.SetBinding(Xamarin.Forms.ScrollView.ScrollYProperty, "ScrollY");
            ChatMessagesListView.SetBinding(HeightProperty, "ScrollViewHeight");

            ChatMessagesListView.Opacity = 0;

            Messages = new ObservableCollection<ChatMessage>();
            TemplateSelector = new ChatMessageTemplateSelector(true);
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ChatPageViewModel.Messages):
                    Messages = _viewModel.Messages;
                    break;
                default:
                    break;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Workaround for the unexpected scrolling
            ChatMessagesListView.HeightRequest = ChatScrollView.Height - InputView.Height;
            await Task.Delay(500);
            if (_viewModel.IsActive)
                ChatMessagesListView.HeightRequest = ChatScrollView.Height - InputView.Height;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        async void Handle_ScrollMessagesEvent(object sender, System.EventArgs e)
        {
            if (e is ScrollEventArgs args)
            {
                if (_viewModel.IsActive)
                {
                    ChatMessagesListView.LayoutManager.ScrollToRowIndex(args.Index, args.ScrollPosition, !args.Animated);
                    if (ChatMessagesListView.Opacity <= 0)
                    {
                        await Task.Delay(75);
                        if (_viewModel.IsActive)
                            await ChatMessagesListView.FadeTo(1, 275, Easing.CubicInOut);
                    }
                }
            }
        }

        void ScrollRows_Changed(object sender, Syncfusion.GridCommon.ScrollAxis.ScrollChangedEventArgs e)
        {
            _viewModel.LastVisibleIndex = _visualContainer.ScrollRows.LastBodyVisibleLineIndex;
        }
    }
}
