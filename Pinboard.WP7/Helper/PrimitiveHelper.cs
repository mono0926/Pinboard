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

namespace Mono.App.Pinboard.Helper
{
    public static class PrimitiveHelper
    {
        public static ObservableCollection<string> ToObserbleCollection(this string tags)
        {
            var result = new ObservableCollection<string>();
            if (tags != null)
            {
                foreach (var i in tags.Split(' '))
                {
                    result.Add(i);
                }
            }
            return result;
        }
    }
}