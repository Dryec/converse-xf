using System;
using Converse.Enums;
using Converse.Models;
using Converse.Views.Chat;
using Xamarin.Forms;

namespace Converse.DataTemplateSelectors
{
    public class ChatMessageTemplateSelector : DataTemplateSelector
    {
        readonly DataTemplate _rightMessageDataTemplate;
        readonly DataTemplate _leftMessageDataTemplate;

        public ChatMessageTemplateSelector() : this(false, false)
        {

        }

        public ChatMessageTemplateSelector(bool showName, bool showImage)
        {
            _rightMessageDataTemplate = new DataTemplate(typeof(RightChatMessageViewCell));
            _leftMessageDataTemplate = new DataTemplate(typeof(LeftChatMessageViewCell));
            _leftMessageDataTemplate.SetValue(LeftChatMessageViewCell.ShowNameProperty, showName);
            _leftMessageDataTemplate.SetValue(LeftChatMessageViewCell.ShowImageProperty, showImage);
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is ChatMessage msg)
            {
                return msg.IsSender ? _rightMessageDataTemplate : _leftMessageDataTemplate;
            }

            return null;
        }
    }
}
