using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Phone.Tasks;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Helper;
using Mono.App.Pinboard.Model;
using Mono.App.Pinboard.Service;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class MainViewModel : MonoViewModelBase
    {
        public MainViewModel()
        {
            GlobalMessenger.Register<GenericMessage<BackgroundType>>(this, m =>
            {
                BackgroundImageType = m.Content;
            });
        }

        #region property

        private PinboardService service = PinboardService.GetInstance();

        private BackgroundType backgroundImageType;

        public BackgroundType BackgroundImageType
        {
            get { return backgroundImageType; }
            set
            {
                backgroundImageType = value;
                RaisePropertyChanged(() => BackgroundImageType);
            }
        }

        public ItemBoxViewModel AllBookmarkItem { get; set; }

        public ItemBoxViewModel UnreadItem { get; set; }

        public ItemBoxViewModel PrivateItem { get; set; }

        public ItemBoxViewModel PublicItem { get; set; }

        public ItemBoxViewModel UntaggedItem { get; set; }

        public ItemBoxViewModel TaggedItem { get; set; }

        public ItemBoxViewModel NetworkItem { get; set; }

        public ItemBoxViewModel PopularItem { get; set; }

        public ItemBoxViewModel RecentItem { get; set; }

        public ItemBoxViewModel PowerSearhItem { get; set; }

        public ItemBoxViewModel TagSearchItem { get; set; }

        public ItemBoxViewModel AnotherUserItem { get; set; }

        public bool IsReloadVisible
        {
            get
            {
                if (!SettingsService.Contains(Consts.LastSynkKey))
                {
                    return true;
                }
                var diff = DateTime.Now.AddMinutes(-Consts.PinboardApiRestrictionInterval)
                    .CompareTo(SettingsService.LoadSettingOrDefault<DateTime>(Consts.LastSynkKey));
                if (diff > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

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

        #endregion property

        private bool isInitialized;

        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }
            IsProgress = true;
            ProgressMessage = Consts.SyncingMessage;
            BackgroundImageType = SettingsService.LoadBackgroundType();
            Observable.Start(() =>
            {
                service.LoadPostFromDB(() =>
                    {
                        CountAll();
                        isInitialized = true;
                        ReloadItems();
                    });
            })
            .Subscribe();

            ConfirmClipboard();

            Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(Consts.ReloadCheckInterval))
                 .ObserveOn(SynchronizationContext.Current)
                 .Subscribe(_ => RaisePropertyChanged(() => IsReloadVisible));
        }

        private void ConfirmClipboard()
        {
            if (!Clipboard.ContainsText())
            {
                return;
            }
            SettingsService.LoadEnabledClipboard()
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(clipboadExisted =>
            {
                if (!clipboadExisted)
                {
                    return;
                }
                var message = new MyDialogMessage(MessageKeys.ClipboardKey, result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Messenger.Send(new NavigationMessage(
                            new Uri(string.Format("/View/{0}View.xaml", "CopyToMine"), UriKind.Relative),
                            new CopyToMineViewModel(new FeedItemViewModel
                            {
                                Time = DateTime.Now,
                                Title = string.Empty,
                                Tags = new ObservableCollection<string>()
                            }.ToBookmark()),
                            MessageKeys.TransitionKey));
                    }
                });
                Messenger.Send(message);
            });
        }

        private void ReloadItems()
        {
            PinboardService.GetInstance().GetPopular().Subscribe(_ =>
            {
                PopularItem.Count = service.popularItems.Count;
                CollapseProgressIfDone();
            });
            PinboardService.GetInstance().GetRecentAll().Subscribe(_ =>
            {
                RecentItem.Count = service.recentItems.Count;
                CollapseProgressIfDone();
            });

            PinboardService.GetInstance().IsLogin().Subscribe(loggedIn =>
                {
                    if (loggedIn)
                    {
                        PinboardService.GetInstance().GetAllBookmarks()
                        .Subscribe(posts =>
                        {
                            CountBookmarks();
                            CollapseProgressIfDone();
                        }, (ex) =>
                        {
                            Debug.WriteLine(ex.Message);
                            Debug.WriteLine(ex.StackTrace);
                        });
                        PinboardService.GetInstance().GetNetwork().Subscribe(_ =>
                        {
                            NetworkItem.Count = service.networkItems.Count;
                            CollapseProgressIfDone();
                        });

                        PinboardService.GetInstance().GetTags().Subscribe(_ =>
                        {
                            CollapseProgressIfDone();
                        });
                    }
                    else
                    {
                        CollapseProgressIfDone();
                        Messenger.Send(new MyDialogMessage(MessageKeys.LoginFailKey, result =>
                            {
                                Messenger.Send(new NavigationMessage(
                                    new Uri(string.Format(Consts.ViewFormat, "Account"), UriKind.Relative),
                                    new AccountViewModel(), MessageKeys.TransitionKey));
                            }));
                    }
                });
        }

        private void CountAll()
        {
            CountBookmarks();
            RecentItem.Count = service.recentItems.Count;
            PopularItem.Count = service.popularItems.Count;
            NetworkItem.Count = service.networkItems.Count;
        }

        private void CountBookmarks()
        {
            AllBookmarkItem.Count = service.allItems.Count;
            PrivateItem.Count = service.privateItems.Count;
            UntaggedItem.Count = service.untaggedItems.Count;
            UnreadItem.Count = service.unreadItems.Count;
            PublicItem.Count = service.publicItems.Count;
            TaggedItem.Count = service.taggedItems.Count;
        }

        private void CollapseProgressIfDone()
        {
            if (PinboardService.GetInstance().IsLoaded)
            {
                ExecuteOnUIThread(() =>
                {
                    IsProgress = false;
                    ProgressMessage = string.Empty;
                });
            }

            PinboardService.GetInstance().IsLogin()
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(loggedIn =>
                {
                    if (!loggedIn && PinboardService.GetInstance().IsLoadedOnlyNoLogin)
                    {
                        IsProgress = false;
                        ProgressMessage = string.Empty;
                    }
                });
        }

        #region Command

        private ICommand _setupCommand;

        public ICommand SetupCommand
        {
            get
            {
                return _setupCommand ?? (_setupCommand = new RelayCommand<SetupItemType>(type =>
                {
                    switch (type)
                    {
                        case SetupItemType.Account:
                            Messenger.Send(new NavigationMessage(
                                  new Uri("/View/AccountView.xaml", UriKind.Relative),
                                  new AccountViewModel(), MessageKeys.TransitionKey));
                            break;

                        case SetupItemType.Setting:
                            Messenger.Send(new NavigationMessage(
                                  new Uri("/View/SettingsView.xaml", UriKind.Relative),
                                  new SettingsViewModel(), MessageKeys.TransitionKey));
                            break;

                        case SetupItemType.Support:
                            new EmailComposeTask
                                {
                                    To = Consts.AuthorMailAddress,
                                    Subject = Consts.SupportMailTitle,
                                    Body = Consts.SupportMailBody,
                                }
                                .Show();
                            break;
                        default:
                            break;
                    }
                }));
            }
        }

        private ICommand _itemCommand;

        public ICommand ItemCommand
        {
            get
            {
                return _itemCommand ??
                    (_itemCommand = new RelayCommand<ItemBoxViewModel>((item) =>
                    {
                        ViewModelBase vm = null;
                        string view = string.Empty;

                        switch (item.Type)
                        {
                            case YourItemType.Search:
                                if (PinboardService.GetInstance().IsTrial)
                                {
                                    vm = new TrialViewModel();
                                    view = "Trial";
                                }
                                else
                                {
                                    vm = new PowerSearchViewModel();
                                    view = "PowerSearch";
                                }
                                break;

                            case YourItemType.TagSearch:
                                vm = new TagItemsSearchViewModel();
                                view = "User";
                                break;

                            case YourItemType.User:
                                vm = new UserViewModel();
                                view = "User";
                                break;

                            case YourItemType.Recent:
                            case YourItemType.Popular:
                            case YourItemType.Network:
                                vm = new FeedsViewModel(item);
                                view = "Feeds";
                                break;

                            default:
                                vm = new BookmarksViewModel(item);
                                view = "Bookmarks";
                                break;
                        }
                        Messenger.Send(new NavigationMessage(
                            new Uri(string.Format(Consts.ViewFormat, view), UriKind.Relative),
                            vm, MessageKeys.TransitionKey));
                    }));
            }
        }

        private ICommand _initializeCommand;

        public ICommand InitializeCommand
        {
            get
            {
                return _initializeCommand ??
                    (_initializeCommand = new RelayCommand(() =>
                    {
                        Initialize();
                        RaisePropertyChanged(() => IsReloadVisible);
                    }));
            }
        }

        private ICommand reloadCommand;

        public ICommand ReloadCommand
        {
            get
            {
                return reloadCommand ??
                    (reloadCommand = new RelayCommand(() =>
                    {
                        IsProgress = true;
                        ProgressMessage = Consts.SyncingMessage;
                        Observable.Start(ReloadItems).Subscribe();
                    }));
            }
        }

        #endregion Command
    }
}