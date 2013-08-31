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
    public abstract class SearchViewModelBase : MonoViewModelBase
    {
        public SearchViewModelBase(string viewName, Func<IList<Tag>> getColllections, Func<string, IObservable<IEnumerable<FeedItem>>> getItems)
        {
            this.viewName = viewName;
            this.getColllections = getColllections;
            this.getItems = getItems;
        }

        public SearchViewModelBase(string viewName, Func<IList<Tag>> getColllections, Func<string, IObservable<IEnumerable<FeedItem>>> getItems, string account)
            : this(viewName, getColllections, getItems)
        {
            SelectedIndex = 1;
            ReflectToUser(account);
        }

        private string current;

        public string Current
        {
            get { return current; }
            set
            {
                current = value;
                RaisePropertyChanged(() => Current);
            }
        }

        private string viewName;
        private Func<IList<Tag>> getColllections;
        private Func<string, IObservable<IEnumerable<FeedItem>>> getItems;

        private bool isInitialized;

        public ObservableCollection<FeedItemViewModel> UserItems { get; set; }

        private IList<Group<Tag>> tagGroups;

        public IList<Group<Tag>> TagGroups
        {
            get { return tagGroups; }
            set
            {
                tagGroups = value;
                RaisePropertyChanged(() => TagGroups);
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

        private void ReflectToUser(string c)
        {
            Current = c;

            if (UserItems != null)
            {
                UserItems.Clear();
            }

            SelectedIndex = 1;

            if (string.IsNullOrWhiteSpace(Current))
            {
                return;
            }

            IsProgressVisible = true;
            getItems(Current).Subscribe(x =>
            {
                UserItems = x.ToObservableCollectionLimit();
                RaisePropertyChanged(() => UserItems);
                IsProgressVisible = false;
            });
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
                      isInitialized = true;

                      IsProgressVisible = true;

                      var accounts = getColllections();

                      var data = from t in accounts
                                 group t by Utils.ToChar(t.Name) into tag
                                 orderby tag.Key
                                 select new Group<Tag>(tag.Key, tag);
                      TagGroups = data.ToList();
                      IsProgressVisible = false;
                  }));
            }
        }

        private ICommand searchAccountCommand;

        public ICommand SearchAccountCommand
        {
            get
            {
                return searchAccountCommand ??
                  (searchAccountCommand = new RelayCommand<string>(account =>
                  {
                      ReflectToUser(account);
                  }));
            }
        }

        private ICommand selectTagCommand;

        public ICommand SelectTagCommand
        {
            get
            {
                return selectTagCommand ??
                  (selectTagCommand = new RelayCommand<Tag>(user =>
                  {
                      ReflectToUser(user.Name);
                  }));
            }
        }

        private ICommand browserCommand;

        public ICommand BrowserCommand
        {
            get
            {
                return browserCommand ??
                    (browserCommand = new RelayCommand<FeedItemViewModel>(item =>
                    {
                        Messenger.Send(new NavigationMessage(
                            new Uri(string.Format("/View/{0}.xaml", viewName), UriKind.Relative),
                            new FeedContentViewModel(UserItems, item, BrowseType.Browser, "Search"),
                            MessageKeys.TransitionKey));
                    }));
            }
        }

        #endregion Command
    }
}