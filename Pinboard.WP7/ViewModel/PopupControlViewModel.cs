using System;
using System.Collections.Generic;
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
using Mono.App.Pinboard.Model;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class PopupControlViewModel : MonoViewModelBase
    {
        public IList<TagPair> TagPairs { get; set; }

        public PopupControlViewModel(BookmarkItemViewModel current)
        {
            this.current = current;
            this.TagPairs = new List<TagPair>();
        }

        private BookmarkItemViewModel current;

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

        private string popupTag;

        public string PopupTag
        {
            get { return popupTag; }
            set
            {
                popupTag = value;
                RaisePropertyChanged(() => PopupTag);
            }
        }

        #region Command

        private ICommand renameCommand;

        public ICommand RenameCommand
        {
            get
            {
                return renameCommand ??
                    (renameCommand = new RelayCommand<string>((s) =>
                    {
                        current.Tags.Remove(PopupTag);
                        current.Tags.Add(s);
                        TagPairs.Add(new TagPair { OldName = PopupTag, NewName = s });
                        IsShowPopup = false;
                    }));
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
                        current.Tags.Remove(PopupTag);
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
                    (cancelCommand = new RelayCommand(() =>
                    {
                        IsShowPopup = false;
                    }));
            }
        }

        #endregion Command
    }
}