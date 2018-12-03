using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using System.Collections.ObjectModel;
using Converse.Models;
using Converse.TokenMessages;
using Newtonsoft.Json;

namespace Converse.ViewModels
{
    public class ChatPageViewModel : ViewModelBase
    {
        public UserInfo MySelf { get; set; }
        public UserInfo ChatPartner { get; set; }
        public ObservableCollection<ChatMessage> Messages { get; set; }

        public ChatPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService) : base(navigationService, pageDialogService, deviceService)
        {
            Title = "Pascal";

            MySelf = new UserInfo { Name = "Marius" };
            ChatPartner = new UserInfo { Name = "Pascal", ImageUri = new Uri("https://www.tron-society.com/img/team/pascal.jpg") };

            Messages = new ObservableCollection<ChatMessage>();

            for (var i = 0; i < 10; i++)
            {
                Messages.Add(new ChatMessage
                {
                    Message = "Hey!",
                    Sender = MySelf,
                    IsSender = true, Timestamp = DateTime.Now.AddDays(-2).AddSeconds(-854)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Hey! What's up?",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddDays(-1).AddSeconds(-263)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Just wanted to ask you something about TRON, hope you have some time?",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-35)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "My question is about Tron SRs, what are they and how do I vote for them?",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-34)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Sure I've time for you! Tron SRs represent the network, there are a total of 27 elected SRs which are handling the block production. To vote for them just open your wallet, freeze some TRX and search for you SR to distribute your votes.",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddMinutes(-28)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Awesome thanks! I'll have a look",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-26)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Your welcome!",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddMinutes(-25)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "If you want to know more - feel free to ask",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddMinutes(-12)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Do you have a website?",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-5)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Sure",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddMinutes(-4)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "https://tron-society.com",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddMinutes(-4)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Thanks I'll check it out",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-2)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "and I think I'll also vote for you!\ud83d\ude0c",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-1)
                });
            }
        }


        public override async void OnAppearing()
        {
            base.OnAppearing(); 
            var b = await _pageDialogService.DisplayAlertAsync("JSON", JsonConvert.SerializeObject(new ChangeNameTokenMessage { Name = "Pascal" }), "Ok", "c");
        }
    }
}
