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
using GalaSoft.MvvmLight.Messaging;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.ViewModel;

namespace Mono.App.Pinboard.Message
{
    public class BookmarkChangeMessage : GenericMessage<BookmarkItemViewModel>
    {
        public BookmarkChangeMessage(BookmarkItemViewModel vm, BookmarkChangeType type)
            : base(vm)
        {
            this.Type = type;
        }

        public BookmarkChangeType Type { get; set; }
    }

    public enum BookmarkChangeType
    {
        Add,
        Delete,
        Modify,
    }
}