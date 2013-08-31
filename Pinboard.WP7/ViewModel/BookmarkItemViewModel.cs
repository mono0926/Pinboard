using System;
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
using Mono.Framework.Mvvm.Message;

namespace Mono.App.Pinboard.ViewModel
{
    public class BookmarkItemViewModel : ItemViewModelBase
    {
        private bool isPrivate;

        public bool IsPrivate
        {
            get { return isPrivate; }
            set
            {
                isPrivate = value;
                RaisePropertyChanged(() => IsPrivate);
            }
        }

        private bool isUnread;

        public bool IsUnread
        {
            get { return isUnread; }
            set
            {
                isUnread = value;
                RaisePropertyChanged(() => IsUnread);
            }
        }

        public string Hash { get; set; }

        public string Meta { get; set; }
    }
}