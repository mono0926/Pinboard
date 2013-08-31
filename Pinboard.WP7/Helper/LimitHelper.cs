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
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Service;
using Mono.App.Pinboard.ViewModel;
using Mono.Framework.Common.Extensions;

namespace Mono.App.Pinboard.Helper
{
    public static class LimitHelper
    {
        private static IEnumerable<T> Limit<T>(IEnumerable<T> items, int num)
        {
            if (PinboardService.GetInstance().IsTrial)
            {
                return items.Take(num);
            }
            return items;
        }

        private static IList<T> Limit<T>(IList<T> items, int num)
        {
            if (PinboardService.GetInstance().IsTrial)
            {
                return items.Take(num).ToList();
            }
            return items;
        }

        public static IEnumerable<T> Limit<T>(this  IEnumerable<T> items) where T : ItemBase
        {
            return Limit(items, Consts.LimitSocial);
        }

        public static IList<T> Limit<T>(this  IList<T> items) where T : ItemViewModelBase
        {
            return Limit(items, Consts.LimitSocial);
        }

        public static IList<BookmarksViewModel> Limit(this  IList<BookmarksViewModel> items)
        {
            return Limit(items, Consts.LimitItem);
        }

        public static IEnumerable<Post> Limit(this IEnumerable<Post> items)
        {
            return Limit(items, Consts.LimitItem);
        }

        public static ObservableCollection<FeedItemViewModel> ToObservableCollectionLimit<T>(this IEnumerable<T> source) where T : FeedItem
        {
            var r = new ObservableCollection<FeedItemViewModel>();
            source.Limit().ForEach(s => r.Add(s.ToItemViewModel()));
            return r;
        }
    }
}