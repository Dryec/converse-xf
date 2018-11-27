using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using System.Windows.Input;

namespace Converse.ViewModels
{
    public class WelcomePageViewModel : ViewModelBase
    {
        public WelcomePageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService) : base(navigationService, pageDialogService, deviceService)
        {
            Title = "Welcome";
        }
    }
}
