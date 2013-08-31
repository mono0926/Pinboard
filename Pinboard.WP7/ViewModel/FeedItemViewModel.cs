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

namespace Mono.App.Pinboard.ViewModel
{
    public class FeedItemViewModel : ItemViewModelBase
    {
        public string Identifier { get; set; }

        public string Author { get; set; }
    }
}