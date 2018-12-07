using System.Diagnostics;
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

            ChatMessagesListView.LayoutManager.ItemsCacheLimit = 10;

            _viewModel = (BindingContext as ChatPageViewModel);
            _viewModel.ScrollMessagesEvent += Handle_ScrollMessagesEvent;
            _visualContainer = ChatMessagesListView.GetVisualContainer();

            _scrollView = ChatMessagesListView.GetScrollView();
            _scrollView.SetBinding(Xamarin.Forms.ScrollView.ContentSizeProperty, "ScrollViewSize");
            _scrollView.SetBinding(Xamarin.Forms.ScrollView.ScrollYProperty, "ScrollY");
            ChatMessagesListView.SetBinding(HeightProperty, "ScrollViewHeight");
        }


        void Handle_ScrollMessagesEvent(object sender, System.EventArgs e)
        {
            if(e is ScrollEventArgs args)
            {
                ChatMessagesListView.LayoutManager.ScrollToRowIndex(args.Index, Syncfusion.ListView.XForms.ScrollToPosition.End, !args.Animated);
            }
        }

    }
}
