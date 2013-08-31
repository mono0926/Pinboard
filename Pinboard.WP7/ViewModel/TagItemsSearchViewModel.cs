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
using Mono.App.Pinboard.Service;

namespace Mono.App.Pinboard.ViewModel
{
    public class TagItemsSearchViewModel : SearchViewModelBase
    {
        public TagItemsSearchViewModel()
            : base("FeedContentView", PinboardService.GetInstance().GetAllTags, PinboardService.GetInstance().GetTagItems) { }
    }
}