using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Command;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Helper;
using Mono.App.Pinboard.Model;
using Mono.App.Pinboard.Service;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class UserViewModel : SearchViewModelBase
    {
        public UserViewModel()
            : base("FeedContentView", PinboardService.GetInstance().GetAllAccounts, PinboardService.GetInstance().GetUserItems)
        {
        }

        public UserViewModel(string account)
            : base("FeedContentView", PinboardService.GetInstance().GetAllAccounts, PinboardService.GetInstance().GetUserItems, account)
        {
        }
    }
}