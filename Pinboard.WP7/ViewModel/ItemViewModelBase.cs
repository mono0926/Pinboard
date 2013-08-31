using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Mono.Api.ReactivePinboard.Model;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public abstract class ItemViewModelBase : MonoViewModelBase
    {
        private string url;

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                RaisePropertyChanged(() => Url);
            }
        }

        public DateTime Time { get; set; }

        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                RaisePropertyChanged(() => Description);
            }
        }

        private ObservableCollection<string> tags;

        public ObservableCollection<string> Tags
        {
            get { return tags; }
            set
            {
                tags = value;
                RaisePropertyChanged(() => Tags);
            }
        }
    }
}