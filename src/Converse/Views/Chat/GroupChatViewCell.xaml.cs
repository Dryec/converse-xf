using Xamarin.Forms;

namespace Converse.Views.Chat
{
    public partial class GroupChatViewCell : ViewCell
    {
        public static readonly BindableProperty FooProperty =
            BindableProperty.Create(nameof(Foo), typeof(string), typeof(GroupChatViewCell), null);

        public GroupChatViewCell()
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
