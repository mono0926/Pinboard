using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reactive;
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
using GalaSoft.MvvmLight.Messaging;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Helper;
using Mono.App.Pinboard.Message;
using Mono.App.Pinboard.Model;
using Mono.App.Pinboard.Service;
using Mono.Framework.Common.Extensions;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class BookmarksViewModel : MonoViewModelBase
    {
        public BookmarksViewModel(ItemBoxViewModel itemBox)
        {
            SelectedIndex = itemBox.ToNum();
            Messenger.Default.Register<GenericMessage<Tag>>(this, ReflectToTag);

            All = new ObservableItems<BookmarkItemViewModel>(service.allItems);
            Unread = new ObservableItems<BookmarkItemViewModel>(service.unreadItems);
            Private_ = new ObservableItems<BookmarkItemViewModel>(service.privateItems);
            Public_ = new ObservableItems<BookmarkItemViewModel>(service.publicItems);
            Untagged = new ObservableItems<BookmarkItemViewModel>(service.untaggedItems);
            Tagged = new ObservableItems<BookmarkItemViewModel>(service.taggedItems);

            GlobalMessenger.Register<BookmarkChangeMessage>(this, m =>
                {
                    if (m.Type == BookmarkChangeType.Add)
                    {
                        All.Refresh();
                        Unread.Refresh();
                        Private_.Refresh();
                        Public_.Refresh();
                        Untagged.Refresh();
                        Tagged.Refresh();
                    }
                    else if (m.Type == BookmarkChangeType.Delete || m.Type == BookmarkChangeType.Modify)
                    {
                        if (All.Items.Contains(m.Content))
                        {
                            All.Refresh();
                        }
                        if (Unread.Items.Contains(m.Content))
                        {
                            Unread.Refresh();
                        }
                        if (Private_.Items.Contains(m.Content))
                        {
                            Private_.Refresh();
                        }
                        if (Public_.Items.Contains(m.Content))
                        {
                            Public_.Refresh();
                        }
                        if (Untagged.Items.Contains(m.Content))
                        {
                            Untagged.Refresh();
                        }
                        if (Tagged.Items.Contains(m.Content))
                        {
                            Tagged.Refresh();
                        }
                    }
                });
        }

        private bool isInitialized;
        private PinboardService service = PinboardService.GetInstance();

        public ObservableItems<BookmarkItemViewModel> All { get; set; }

        public ObservableItems<BookmarkItemViewModel> Unread { get; set; }

        public ObservableItems<BookmarkItemViewModel> Private_ { get; set; }

        public ObservableItems<BookmarkItemViewModel> Public_ { get; set; }

        public ObservableItems<BookmarkItemViewModel> Untagged { get; set; }

        public ObservableItems<BookmarkItemViewModel> Tagged { get; set; }

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

        private string currentTag;

        public string CurrentTag
        {
            get { return currentTag ?? "All"; }
            set
            {
                currentTag = value;
                RaisePropertyChanged(() => CurrentTag);
                SetTagged();
                RaisePropertyChanged(() => Tagged);
            }
        }

        private int selectedIndex;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }

        private void ReflectToTag(GenericMessage<Tag> m)
        {
            CurrentTag = m.Content.Name;
        }

        private void SetTagged()
        {
            if (currentTag == null)
            {
                Tagged = new ObservableItems<BookmarkItemViewModel>(service.taggedItems);
            }
            else
            {
                Tagged = new ObservableItems<BookmarkItemViewModel>(service.allItems.Where(x => x.Tags.Contains(currentTag)).ToList());
            }
            Tagged.Initialize();
        }

        private void Transit(BookmarkItemViewModel item, BrowseType type)
        {
            Messenger.Send(new NavigationMessage(
                new Uri("/View/BookmarkContentView.xaml", UriKind.Relative),
                new BookmarkContentViewModel(ChooseVM(), item, type, ChooseTitle()),
                MessageKeys.TransitionKey));
        }

        private IList<BookmarkItemViewModel> ChooseVM()
        {
            switch (SelectedIndex)
            {
                case 0:
                    return All.SourceItems;
                case 1:
                    return Unread.SourceItems;
                case 2:
                    return Private_.SourceItems;
                case 3:
                    return Public_.SourceItems;
                case 4:
                    return Untagged.SourceItems;
                case 5:
                    return Tagged.SourceItems;
                default:
                    return null;
            }
        }

        private string ChooseTitle()
        {
            switch (SelectedIndex)
            {
                case 0:
                    return "All";
                case 1:
                    return "Unread";
                case 2:
                    return "Private";
                case 3:
                    return "Public";
                case 4:
                    return "Untagged";
                case 5:
                    return "Tagged";
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
                      All.Initialize();
                      Unread.Initialize();
                      Private_.Initialize();
                      Public_.Initialize();
                      Untagged.Initialize();
                      Tagged.Initialize();
                      IsProgressVisible = false;
                      isInitialized = true;
                  }));
            }
        }

        private void InitializeItems()
        {
            All.Initialize();
            Unread.Initialize();
            Private_.Initialize();
            Public_.Initialize();
            Untagged.Initialize();
            Tagged.Initialize();
        }

        private ICommand backkeyPressedCommand;

        public ICommand BackKeyPressedCommand
        {
            get
            {
                return backkeyPressedCommand ??
                    (backkeyPressedCommand = new RelayCommand(() =>
                    {
                        Messenger.Default.Unregister<GenericMessage<Tag>>(this, ReflectToTag);
                    }));
            }
        }

        private ICommand browserCommand;

        public ICommand BrowserCommand
        {
            get
            {
                return browserCommand ??
                    (browserCommand = new RelayCommand<BookmarkItemViewModel>((item) =>
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
                    (configureCommand = new RelayCommand<BookmarkItemViewModel>(item =>
                    {
                        Transit(item, BrowseType.Info);
                    }));
            }
        }

        private ICommand changeTagCommand;

        public ICommand ChangeTagCommand
        {
            get
            {
                return changeTagCommand ??
                    (changeTagCommand = new RelayCommand(() =>
                    {
                        Messenger.Send(new NavigationMessage(new Uri("/View/SelectTagView.xaml", UriKind.Relative),
                            new SelectTagViewModel(), MessageKeys.TransitionKey));
                    }));
            }
        }

        private ICommand deleteCommand;

        public ICommand DeleteCommand
        {
            get
            {
                return deleteCommand ??
                    (deleteCommand = new RelayCommand<BookmarkItemViewModel>((item) =>
                    {
                        ChooseVM().Remove(item);
                        PinboardService.GetInstance().DeleteBookmark(item)
                            .Subscribe(s => { },
                            ex =>
                            {
                                Messenger.Send(new MyDialogMessage(MessageKeys.ErrorKey));
                            });
                    }));
            }
        }

        private ICommand loadAllMore;

        public ICommand LoadAllMore
        {
            get
            {
                return loadAllMore ??
                    (loadAllMore = new RelayCommand(() =>
                    {
                        All.LoadMore();
                    }));
            }
        }

        private ICommand loadUnreadMore;

        public ICommand LoadUnreadMore
        {
            get
            {
                return loadUnreadMore ??
                    (loadUnreadMore = new RelayCommand(() =>
                    {
                        Unread.LoadMore();
                    }));
            }
        }

        private ICommand loadPublicMore;

        public ICommand LoadPublicMore
        {
            get
            {
                return loadPublicMore ??
                    (loadPublicMore = new RelayCommand(() =>
                    {
                        Public_.LoadMore();
                    }));
            }
        }

        private ICommand loadPrivateMore;

        public ICommand LoadPrivateMore
        {
            get
            {
                return loadPrivateMore ??
                    (loadPrivateMore = new RelayCommand(() =>
                    {
                        Private_.LoadMore();
                    }));
            }
        }

        private ICommand loadUntaggedMore;

        public ICommand LoadUntaggedMore
        {
            get
            {
                return loadUntaggedMore ??
                    (loadUntaggedMore = new RelayCommand(() =>
                    {
                        Untagged.LoadMore();
                    }));
            }
        }

        private ICommand loadTaggedMore;

        public ICommand LoadTaggedMore
        {
            get
            {
                return loadTaggedMore ??
                    (loadTaggedMore = new RelayCommand(() =>
                    {
                        Tagged.LoadMore();
                    }));
            }
        }

        #endregion Command
    }
}