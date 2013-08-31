using System;
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
using GalaSoft.MvvmLight.Messaging;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Helper;
using Mono.App.Pinboard.Model;
using Mono.App.Pinboard.Service;
using Mono.Framework.Common.Extensions;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class PowerSearchViewModel : MonoViewModelBase
    {
        public PowerSearchViewModel()
        {
            Items = new ObservableCollection<BookmarkItemViewModel>();
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            Messenger.Default.Register<GenericMessage<Tag>>(this, ProcessTagMessage);
        }

        public ObservableCollection<BookmarkItemViewModel> Items { get; set; }

        public string QueryText { get; set; }

        private bool isTitleOnly;

        public bool IsTitleOnly
        {
            get { return isTitleOnly; }
            set
            {
                isTitleOnly = value;
                RaisePropertyChanged(() => IsTitleOnly);
            }
        }

        private bool isAnd;

        public bool IsAnd
        {
            get { return isAnd; }
            set
            {
                isAnd = value;
            }
        }

        private ObservableCollection<string> tags = new ObservableCollection<string>();

        public ObservableCollection<string> Tags
        {
            get { return tags; }
            set
            {
                tags = value;
                RaisePropertyChanged(() => Tags);
            }
        }

        public bool DateEnabled { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsUnread { get; set; }

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
            get { return currentTag; }
            set
            {
                currentTag = value;
                RaisePropertyChanged(() => CurrentTag);
            }
        }

        private bool isShowPopup;

        public bool IsShowPopup
        {
            get { return isShowPopup; }
            set
            {
                isShowPopup = value;
                RaisePropertyChanged(() => IsShowPopup);
            }
        }

        private void SearchResult()
        {
            IsProgressVisible = true;
            Items.Clear();

            Observable.Start(() =>
            {
                BookmarkItemViewModel[] posts = null;
                PinboardService.LockAndUsingDB(context =>
                {
                    posts = PinboardService.GetInstance().allItems
                        .Where(x => string.IsNullOrWhiteSpace(QueryText) || (x.Title.Contains(QueryText) || (!IsTitleOnly && x.Description.Contains(QueryText))))//query
                        .Where(x => !DateEnabled || (x.Time.CompareTo(StartDate) > 0 && x.Time.CompareTo(EndDate) < 0))
                        .Where(x => !x.IsPrivate)
                        .Where(x => x.IsUnread).ToArray();

                    if (tags.Count() > 0)
                    {
                        if (IsAnd)
                        {
                            posts = posts.Where(x => Tags.All(t => x.Tags.Contains(t))).ToArray();
                        }
                        else
                        {
                            posts = posts.Where(x => Tags.Any(t => x.Tags.Contains(t))).ToArray();
                        }
                    }
                });
                return posts;
            })
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(posts =>
            {
                posts.ForEach(p => Items.Add(p));
                IsProgressVisible = false;
            });
        }

        private void ProcessTagMessage(GenericMessage<Tag> m)
        {
            this.Tags.Add(m.Content.Name);
        }

        #region Command

        private ICommand addTagCommand;

        public ICommand AddTagCommand
        {
            get
            {
                return addTagCommand ??
                    (addTagCommand = new RelayCommand(() =>
                    {
                        Messenger.Send(new NavigationMessage(new Uri("/View/TagSearchView.xaml", UriKind.Relative),
                                        new TagSearchViewModel(), MessageKeys.TransitionKey));
                    }));
            }
        }

        private ICommand searchCommand;

        public ICommand SearchCommand
        {
            get
            {
                return searchCommand ??
                    (searchCommand = new RelayCommand(SearchResult));
            }
        }

        private ICommand removeCommand;

        public ICommand RemoveCommand
        {
            get
            {
                return removeCommand ??
                    (removeCommand = new RelayCommand(() =>
                    {
                        Tags.Remove(CurrentTag);
                        IsShowPopup = false;
                    }));
            }
        }

        private ICommand cancelCommand;

        public ICommand CancelCommand
        {
            get
            {
                return cancelCommand ??
                    (cancelCommand = new RelayCommand<string>(tag =>
                    {
                        Tags.Remove(tag);
                        IsShowPopup = false;
                    }));
            }
        }

        private ICommand showPopupCommand;

        public ICommand ShowPopupCommand
        {
            get
            {
                return showPopupCommand ??
                    (showPopupCommand = new RelayCommand<string>(tag =>
                    {
                        CurrentTag = tag;
                        IsShowPopup = true;
                    }));
            }
        }

        private ICommand backkeyPressedCommand;

        public ICommand BackKeyPressedCommand
        {
            get
            {
                return backkeyPressedCommand ??
                    (backkeyPressedCommand = new RelayCommand(() =>
                    {
                        Messenger.Default.Unregister<GenericMessage<Tag>>(this, ProcessTagMessage);
                    }));
            }
        }

        private ICommand browserCommand;

        public ICommand BrowserCommand
        {
            get
            {
                return browserCommand ??
                    (browserCommand = new RelayCommand<BookmarkItemViewModel>(item =>
                    {
                        Messenger.Send(new NavigationMessage(
                            new Uri("/View/BookmarkContentView.xaml", UriKind.Relative),
                            new BookmarkContentViewModel(Items, item, BrowseType.Browser, "power search"),
                            MessageKeys.TransitionKey));
                    }));
            }
        }

        #endregion Command
    }
}