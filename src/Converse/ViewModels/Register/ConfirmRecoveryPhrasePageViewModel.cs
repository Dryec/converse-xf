using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;

namespace Converse.ViewModels.Register
{
    public class ConfirmRecoveryPhrasePageViewModel : ViewModelBase
    {
        public ConfirmRecoveryPhrasePageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService) : base(navigationService, pageDialogService, deviceService)
        {
            Title = "Recovery Phrase";
        }
    }
}
