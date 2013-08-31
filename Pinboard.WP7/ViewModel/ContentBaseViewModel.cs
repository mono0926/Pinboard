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
using Mono.App.Pinboard.Helper;
using Mono.App.Pinboard.Message;
using Mono.App.Pinboard.Model;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class ContentBaseViewModel<T> : MonoViewModelBase where T : ItemViewModelBase
    {
        public ContentBaseViewModel(IList<T> items, T current, BrowseType type, string title)
        {
            this.Items = items;
            this.Current = current;
            this.currentIndex = items.IndexOf(current);
            CurrentTitle = title;
            switch (type)
            {
                case BrowseType.Browser:
                    this.SelectedIndex = 0;
                    break;

                case BrowseType.Info:
                    this.SelectedIndex = 1;
                    break;
                default:
                    break;
            }
        }

        private int currentIndex;

        #region property

        public IList<T> Items { get; set; }

        public string CurrentTitle { get; private set; }

        private bool isFullScreen;

        private int selectedIndex;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                RaisePropertyChanged(() => FullEnabled);
            }
        }

        private string navigatingUrl;

        public string NavigatingUrl
        {
            get { return navigatingUrl; }
            set
            {
                navigatingUrl = value;
                RaisePropertyChanged(() => NavigatingUrl);

                CopyEnabled = (Current.Url != value);
            }
        }

        public string NavigatedTitle { get; set; }

        private string progressMessage;

        public string ProgressMessage
        {
            get { return progressMessage; }
            set
            {
                progressMessage = value;
                RaisePropertyChanged(() => ProgressMessage);
            }
        }

        private bool isProgress;

        public bool IsProgress
        {
            get { return isProgress; }
            set
            {
                isProgress = value;
                RaisePropertyChanged(() => IsProgress);
            }
        }

        private Thickness browserMargin;

        public Thickness BrowserMargin
        {
            get { return browserMargin; }
            set
            {
                browserMargin = value;
                RaisePropertyChanged(() => BrowserMargin);
            }
        }

        private T current;

        public T Current
        {
            get { return current; }
            set
            {
                current = value;
                RaisePropertyChanged(() => Current);
            }
        }

        public bool PreEnabled
        {
            get
            {
                if (currentIndex == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool NextEnabled
        {
            get
            {
                if (currentIndex >= Items.Count() - 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool FullEnabled
        {
            get
            {
                if (SelectedIndex == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool copyEnabled = false;

        public bool CopyEnabled
        {
            get { return copyEnabled; }
            set
            {
                copyEnabled = value;
                RaisePropertyChanged(() => CopyEnabled);
            }
        }

        #endregion property

        #region Command

        private RelayCommand preCommand;

        public RelayCommand PreCommand
        {
            get
            {
                return preCommand ??
                    (preCommand = new RelayCommand(() =>
                    {
                        currentIndex--;
                        Current = Items.ElementAt(currentIndex);
                        RaisePropertyChanged(() => PreEnabled);
                        RaisePropertyChanged(() => NextEnabled);
                    }));
            }
        }

        private RelayCommand nextCommand;

        public RelayCommand NextCommand
        {
            get
            {
                return nextCommand ??
                    (nextCommand = new RelayCommand(() =>
                    {
                        currentIndex++;
                        Current = Items.ElementAt(currentIndex);
                        RaisePropertyChanged(() => PreEnabled);
                        RaisePropertyChanged(() => NextEnabled);
                    }));
            }
        }

        private RelayCommand fullScreenCommand;

        public RelayCommand FullScreenCommand
        {
            get
            {
                return fullScreenCommand ??
                    (fullScreenCommand = new RelayCommand(() =>
                    {
                        if (isFullScreen)
                        {
                            BrowserMargin = new Thickness(0);
                        }
                        else
                        {
                            BrowserMargin = new Thickness(0, -137, 0, 0);
                        }
                        isFullScreen = !isFullScreen;
                    }));
            }
        }

        private ICommand navigatingCommand;

        public ICommand NavigatingCommand
        {
            get
            {
                return navigatingCommand ??
                    (navigatingCommand = new RelayCommand(() =>
                    {
                        IsProgress = true;
                        ProgressMessage = Consts.LoadingNow;
                    }));
            }
        }

        private ICommand loadCompletedCommand;

        public ICommand LoadCompletedCommand
        {
            get
            {
                return loadCompletedCommand ??
                    (loadCompletedCommand = new RelayCommand(() =>
                    {
                        IsProgress = false;
                        ProgressMessage = string.Empty;
                    }));
            }
        }

        private ICommand shareCommand;

        public ICommand ShareCommand
        {
            get
            {
                return shareCommand ??
                    (shareCommand = new RelayCommand(() =>
                    {
                        var u = NavigatingUrl;
                        var t = NavigatedTitle;

                        if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(t))
                        {
                            u = Current.Url;
                            t = Current.Title;
                        }

                        new ShareLinkTask
                        {
                            LinkUri = new Uri(u),
                            Title = t,
                            Message = t,
                        }
                        .Show();
                    }));
            }
        }

        private ICommand ieCommand;

        public ICommand IECommand
        {
            get
            {
                return ieCommand ??
                    (ieCommand = new RelayCommand(() =>
                    {
                        var u = NavigatingUrl;

                        if (string.IsNullOrWhiteSpace(u))
                        {
                            u = Current.Url;
                        }

                        new WebBrowserTask
                        {
                            Uri = new Uri(u),
                        }
                        .Show();
                    }));
            }
        }

        private ICommand copyCommand;

        public ICommand CopyCommand
        {
            get
            {
                return copyCommand ??
                    (copyCommand = new RelayCommand(() =>
                    {
                        var vm = new FeedItemViewModel
                            {
                                Url = NavigatingUrl,
                                Time = DateTime.Now,
                                Title = NavigatedTitle,
                                Tags = new ObservableCollection<string>()
                            };
                        Messenger.Send(new NavigationMessage(new Uri("/View/CopyToMineView.xaml", UriKind.Relative),
                            new CopyToMineViewModel(vm.ToBookmark()),
                            MessageKeys.TransitionKey));
                        GlobalMessenger.Send(new BookmarkChangeMessage(null, BookmarkChangeType.Add));
                    }));
            }
        }

        #endregion Command
    }
}