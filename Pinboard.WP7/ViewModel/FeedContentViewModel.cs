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
using Microsoft.Phone.Tasks;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Helper;
using Mono.App.Pinboard.Model;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class FeedContentViewModel : ContentBaseViewModel<FeedItemViewModel>
    {
        public FeedContentViewModel(IList<FeedItemViewModel> items, FeedItemViewModel current, BrowseType type, string title)
            : base(items, current, type, title) { }

        private ICommand copyToMineCommand;

        public ICommand CopyToMineCommand
        {
            get
            {
                return copyToMineCommand ??
                    (copyToMineCommand = new RelayCommand(() =>
                    {
                        Messenger.Send(new NavigationMessage(new Uri("/View/CopyToMineView.xaml", UriKind.Relative),
                            new CopyToMineViewModel(Current.ToBookmark()), MessageKeys.TransitionKey));
                    }));
            }
        }

        private ICommand browseUserItems;

        public ICommand BrowseUserItems
        {
            get
            {
                return browseUserItems ??
                    (browseUserItems = new RelayCommand<string>(account =>
                    {
                        Messenger.Send(new NavigationMessage(new Uri("/View/UserView.xaml", UriKind.Relative),
                            new UserViewModel(account), MessageKeys.TransitionKey));
                    }));
            }
        }
    }
}