using System;
using System.Collections.Generic;
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
using Mono.App.Pinboard.Model;
using Mono.App.Pinboard.Service;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public abstract class TagsViewModel : MonoViewModelBase
    {
        public TagsViewModel()
        {
            PinboardService.LockAndUsingDB(context =>
            {
                var data = from t in context.Tags.ToList()
                           group t by Utils.ToChar(t.Name) into tag
                           orderby tag.Key
                           select new Group<Tag>(tag.Key, tag);
                TagGroups = data.ToList();
                RaisePropertyChanged(() => TagGroups);
            }, false);
        }

        public IList<Group<Tag>> TagGroups { get; set; }

        #region Command

        private ICommand selectTagCommand;

        public ICommand SelectTagCommand
        {
            get
            {
                return selectTagCommand ??
                  (selectTagCommand = new RelayCommand<Tag>(tag =>
                    {
                        Messenger.Default.Send(new GenericMessage<Tag>(tag));
                        Messenger.Send(new GenericMessage<string>(MessageKeys.GoBackKey));
                    }));
            }
        }

        #endregion Command
    }

    public class Group<T> : List<T>
    {
        public Group(string key, IEnumerable<T> list)
        {
            GroupName = key;
            this.AddRange(list);
        }

        public string GroupName { get; set; }
    }
}