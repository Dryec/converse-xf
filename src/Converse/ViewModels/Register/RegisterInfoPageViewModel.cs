using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using System.Windows.Input;

namespace Converse.ViewModels.Register
{
    public class RegisterInfoPageViewModel : ViewModelBase
    {
        
        public RegisterInfoPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService)
         : base(navigationService, pageDialogService, deviceService)
        {
            Title = "Register Info";
        }
    }
}
