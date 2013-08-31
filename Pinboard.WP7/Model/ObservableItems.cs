using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Helper;
using Mono.App.Pinboard.ViewModel;
using Mono.Framework.Common.Extensions;

namespace Mono.App.Pinboard.Model
{
    public class ObservableItems<T> : INotifyPropertyChanged where T : ItemViewModelBase
    {
        public ObservableItems(IList<T> sourceItems)
        {
            this.SourceItems = sourceItems;
            this.Items = new ObservableCollection<T>();
        }

        private int currentPage = 0;

        public ObservableCollection<T> Items { get; set; }

        public IList<T> SourceItems { get; set; }

        public void LoadMore()
        {
            currentPage += 1;
            foreach (var i in SourceItems.Limit().Skip(currentPage * Consts.InitialSize).Take(Consts.InitialSize))
            {
                Items.Add(i);
            }
        }

        public void Initialize()
        {
            SourceItems.Limit().Take(Consts.InitialSize).ForEach(x => Items.Add(x));
        }

        public void Refresh()
        {
            Items.Clear();
            Initialize();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string name)
        {
            var p = PropertyChanged;
            if (p != null)
            {
                p(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}