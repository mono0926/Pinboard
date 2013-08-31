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
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Tasks;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Message;
using Mono.App.Pinboard.Model;
using Mono.App.Pinboard.Service;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class BookmarkContentViewModel : ContentBaseViewModel<BookmarkItemViewModel>
    {
        private bool isTagMessageRegisterd;

        public BookmarkContentViewModel(IList<BookmarkItemViewModel> items, BookmarkItemViewModel current, BrowseType type, string title)
            : base(items, current, type, title)
        {
            this.PopupDataContext = new PopupControlViewModel(Current);
        }

        public PopupControlViewModel PopupDataContext { get; set; }

        private void ProcessTagMessage(GenericMessage<Tag> m)
        {
            Current.Tags.Add(m.Content.Name);
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
                        if (isTagMessageRegisterd)
                        {
                            return;
                        }
                        Messenger.Default.Register<GenericMessage<Tag>>(this, ProcessTagMessage);
                        isTagMessageRegisterd = true;
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

        private RelayCommand saveCommand;

        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ??
                    (saveCommand = new RelayCommand(() =>
                    {
                        IsProgress = true;
                        ProgressMessage = Consts.SavingMessa;

                        PinboardService.GetInstance().SaveBookmark(Current).Subscribe(s =>
                        {
                            foreach (var t in PopupDataContext.TagPairs)
                            {
                                GlobalMessenger.Send(new BookmarkChangeMessage(Current, BookmarkChangeType.Modify));
                                PinboardService.GetInstance().RenameTag(t.OldName, t.NewName).Subscribe();
                            }

                            IsProgress = false;
                            ProgressMessage = string.Empty;
                        }, ex =>
                        {
                            Messenger.Send(new MyDialogMessage(MessageKeys.SaveBookmarkFailKey, result =>
                                {
                                    IsProgress = false;
                                    ProgressMessage = string.Empty;
                                }));
                        });
                    }));
            }
        }

        private RelayCommand deleteCommand;

        public RelayCommand DeleteCommand
        {
            get
            {
                return deleteCommand ??
                    (deleteCommand = new RelayCommand(() =>
                    {
                        Messenger.Send(new MyDialogMessage(MessageKeys.DeleteBookmarkConfirmKey, result =>
                            {
                                if (result != MessageBoxResult.OK)
                                {
                                    return;
                                }
                                IsProgress = true;
                                ProgressMessage = Consts.DeletingMessage;
                                Items.Remove(Current);
                                PinboardService.GetInstance().DeleteBookmark(Current.Url).Subscribe(_ =>
                                {
                                    IsProgress = false;
                                    ProgressMessage = string.Empty;
                                    GlobalMessenger.Send(new BookmarkChangeMessage(Current, BookmarkChangeType.Delete));
                                    Messenger.Send(new GenericMessage<string>(MessageKeys.GoBackKey));
                                }, ex =>
                                {
                                    Messenger.Send(new MyDialogMessage(MessageKeys.ErrorKey));
                                    IsProgress = false;
                                    ProgressMessage = string.Empty;
                                });
                            }));
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

        #endregion Command
    }
}