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
using Microsoft.Phone.Tasks;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class SetupItemViewModel : MonoViewModelBase
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public SetupItemType Type { get; set; }
    }

    public enum SetupItemType
    {
        Account,
        Setting,
        Support,
    }
}