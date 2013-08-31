using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Helper;
using Mono.App.Pinboard.Model;
using Mono.App.Pinboard.Service;
using Mono.Framework.Common.Extensions;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class FeedsViewModel : MonoViewModelBase
    {
        public FeedsViewModel(ItemBoxViewModel itemBox)
        {
            SelectedIndex = itemBox.ToNumForFeed();

            Network = new ObservableItems<FeedItemViewModel>(service.networkItems);
            Popular = new ObservableItems<FeedItemViewModel>(service.popularItems);
            Recent = new ObservableItems<FeedItemViewModel>(service.recentItems);
        }

        private bool isInitialized;
        private PinboardService service = PinboardService.GetInstance();

        public ObservableItems<FeedItemViewModel> Network { get; set; }

        public ObservableItems<FeedItemViewModel> Popular { get; set; }

        public ObservableItems<FeedItemViewModel> Recent { get; set; }

        private bool isProgressVisible;

        public bool IsProgressVisible
        {
            get { return isProgressVisible; }
            set
            {
                isProgressVisible = value;
                RaisePropertyChanged(() => IsProgressVisible);
            }
        }

        private int selectedIndex;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                RaisePropertyChanged(() => SelectedIndex);
            }
        }

        private void Transit(FeedItemViewModel item, BrowseType type)
        {
            Messenger.Send(new NavigationMessage(
                new Uri("/View/FeedContentView.xaml", UriKind.Relative),
                new FeedContentViewModel(ChooseVMType(), item, type, ChooseTitle()),
                MessageKeys.TransitionKey));
        }

        private IList<FeedItemViewModel> ChooseVMType()
        {
            switch (SelectedIndex)
            {
                case 0:
                    return Network.SourceItems;
                case 1:
                    return Popular.SourceItems;
                case 2:
                    return Recent.SourceItems;
                default:
                    return null;
            }
        }

        private string ChooseTitle()
        {
            switch (SelectedIndex)
            {
                case 0:
                    return "Network";
                case 1:
                    return "Popular";
                case 2:
                    return "Recent";
                default:
                    return null;
            }
        }

        #region Command

        private ICommand initializeCommand;

        public ICommand InitializeCommand
        {
            get
            {
                return initializeCommand ??
                  (initializeCommand = new RelayCommand(() =>
                  {
                      if (isInitialized)
                      {
                          return;
                      }
                      IsProgressVisible = true;

                      Network.Initialize();
                      Popular.Initialize();
                      Recent.Initialize();
                      IsProgressVisible = false;
                      isInitialized = true;
                  }));
            }
        }

        private ICommand browserCommand;

        public ICommand BrowserCommand
        {
            get
            {
                return browserCommand ??
                    (browserCommand = new RelayCommand<FeedItemViewModel>((item) =>
                    {
                        Transit(item, BrowseType.Browser);
                    }));
            }
        }

        private ICommand configureCommand;

        public ICommand ConfigureCommand
        {
            get
            {
                return configureCommand ??
                    (configureCommand = new RelayCommand<FeedItemViewModel>((item) =>
                    {
                        Transit(item, BrowseType.Info);
                    }));
            }
        }

        private ICommand deleteCommand;

        public ICommand DeleteCommand
        {
            get
            {
                return deleteCommand ??
                    (deleteCommand = new RelayCommand<FeedItemViewModel>((item) =>
                    {
                        ChooseVMType().Remove(item);
                    }));
            }
        }

        private ICommand _loadNetworkMore;

        public ICommand LoadNetworkMore
        {
            get
            {
                return _loadNetworkMore ?? (_loadNetworkMore = new RelayCommand(() =>
                    {
                        Network.LoadMore();
                    }));
            }
        }

        private ICommand _loadPopularMore;

        public ICommand LoadPopularMore
        {
            get
            {
                return _loadPopularMore ?? (_loadPopularMore = new RelayCommand(() =>
                {
                    Popular.LoadMore();
                }));
            }
        }

        private ICommand _loaRecentMore;

        public ICommand LoaRecentMore
        {
            get
            {
                return _loaRecentMore ?? (_loaRecentMore = new RelayCommand(() =>
                {
                    Recent.LoadMore();
                }));
            }
        }

        #endregion Command
    }
}