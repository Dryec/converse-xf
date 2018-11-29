using System;
using Converse.Enums;
using Converse.Models;
using Converse.Views.Chat;
using Xamarin.Forms;

namespace Converse.DataTemplateSelectors
{
    public class ChatEntryTemplateSelector : DataTemplateSelector
    {
        readonly DataTemplate _normalChatDataTemplate;
        readonly DataTemplate _groupChatDataTemplate;

        public ChatEntryTemplateSelector()
        {
            _normalChatDataTemplate = new DataTemplate(typeof(NormalChatViewCell));
            _groupChatDataTemplate = new DataTemplate(typeof(GroupChatViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is ChatEntry chatEntry)
            {
                switch (chatEntry.Type)
                {
                    case ChatType.Normal:
                        return _normalChatDataTemplate;
                    case ChatType.Group:
                        return _groupChatDataTemplate;
                }
            }

            return null;
        }
    }
}
