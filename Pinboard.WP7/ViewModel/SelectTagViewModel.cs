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
using GalaSoft.MvvmLight.Messaging;
using Mono.Api.ReactivePinboard.Model;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class SelectTagViewModel : TagsViewModel
    {
        private ICommand allCommand;

        public ICommand AllCommand
        {
            get
            {
                return allCommand ??
                  (allCommand = new RelayCommand(() =>
                  {
                      Messenger.Default.Send(new GenericMessage<Tag>(new Tag()));
                      Messenger.Send(new GenericMessage<string>(MessageKeys.GoBackKey));
                  }));
            }
        }
    }
}