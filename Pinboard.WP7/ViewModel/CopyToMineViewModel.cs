using System;
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
using GalaSoft.MvvmLight.Messaging;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Helper;
using Mono.App.Pinboard.Service;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class CopyToMineViewModel : MonoViewModelBase
    {
        public CopyToMineViewModel(BookmarkItemViewModel vm)
        {
            Current = vm;
            Current.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "Url" || e.PropertyName == "Title")
                    {
                        SaveBookmarkCommand.RaiseCanExecuteChanged();
                    }
                };
        }

        private bool isTagMessageRegisterd;

        public BookmarkItemViewModel Current { get; set; }

        private void ProcessTagMessage(GenericMessage<Tag> m)
        {
            Current.Tags.Add(m.Content.Name);
        }

        private PopupControlViewModel popupDataContext;

        public PopupControlViewModel PopupDataContext
        {
            get { return popupDataContext; }
            set
            {
                popupDataContext = value;
                RaisePropertyChanged(() => PopupDataContext);
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

        #region Command

        private RelayCommand saveBookmarkCommand;

        public RelayCommand SaveBookmarkCommand
        {
            get
            {
                return saveBookmarkCommand ??
                    (saveBookmarkCommand = new RelayCommand(() =>
                    {
                        IsProgress = true;
                        ProgressMessage = Consts.SavingMessa;
                        PinboardService.GetInstance().SaveBookmark(Current).Subscribe(s =>
                        {
                            foreach (var t in PopupDataContext.TagPairs)
                            {
                                PinboardService.GetInstance().RenameTag(t.OldName, t.NewName).Subscribe();
                            }

                            IsProgress = false;
                            ProgressMessage = string.Empty;
                            Messenger.Send(new GenericMessage<string>(MessageKeys.GoBackKey));
                            Messenger.Default.Unregister<GenericMessage<Tag>>(this, ProcessTagMessage);
                        },
                        ex =>
                        {
                            Messenger.Send(new MyDialogMessage(MessageKeys.SaveBookmarkFailKey));
                        });
                    },
                    () =>
                    {
                        if (string.IsNullOrWhiteSpace(Current.Url) || string.IsNullOrWhiteSpace(Current.Title))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
            }
        }

        private ICommand navigateTagCommand;

        public ICommand NavigateTagCommand
        {
            get
            {
                return navigateTagCommand ??
                    (navigateTagCommand = new RelayCommand(() =>
                    {
                        Messenger.Send(new NavigationMessage(new Uri("/View/AddTagView.xaml", UriKind.Relative),
                                        new AddTagViewModel(), MessageKeys.TransitionKey));
                    }));
            }
        }

        private ICommand initializeCommand;

        public ICommand InitializeCommand
        {
            get
            {
                return initializeCommand ??
                    (initializeCommand = new RelayCommand(() =>
                    {
                        if (!isTagMessageRegisterd)
                        {
                            Messenger.Default.Register<GenericMessage<Tag>>(this, ProcessTagMessage);
                            isTagMessageRegisterd = true;
                            PopupDataContext = new PopupControlViewModel(Current);
                        }
                    }));
            }
        }

        private ICommand finalizeCommand;

        public ICommand FinalizeCommand
        {
            get
            {
                return finalizeCommand ??
                    (finalizeCommand = new RelayCommand(() =>
                    {
                        Messenger.Default.Unregister<GenericMessage<Tag>>(this, ProcessTagMessage);
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
                        PopupDataContext.PopupTag = tag;
                        PopupDataContext.IsShowPopup = true;
                    }));
            }
        }

        #endregion Command
    }
}