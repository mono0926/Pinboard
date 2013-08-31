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
using Mono.App.Pinboard.Service;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class AddTagViewModel : TagsViewModel
    {
        /// <summary>
        /// 押したタグを通知し、前の画面に戻る
        /// </summary>
        private ICommand addCommand;

        public ICommand AddCommand
        {
            get
            {
                return addCommand ??
                  (addCommand = new RelayCommand<string>(tagStr =>
                  {
                      PinboardService.GetInstance().AddTagsSource(tagStr);
                      Messenger.Default.Send(new GenericMessage<Tag>(new Tag { Name = tagStr }));
                      Messenger.Send(new GenericMessage<string>(MessageKeys.GoBackKey));
                  }));
            }
        }
    }
}