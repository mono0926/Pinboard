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
using Mono.App.Pinboard.Model;
using Mono.App.Pinboard.ViewModel;

namespace Mono.App.Pinboard.Helper
{
    internal static class ItemBoxHelper
    {
        public static int ToNum(this ItemBoxViewModel item)
        {
            switch (item.Type)
            {
                case YourItemType.All:
                    return 0;
                case YourItemType.Unread:
                    return 1;
                case YourItemType.Private:
                    return 2;
                case YourItemType.Public:
                    return 3;
                case YourItemType.Untagged:
                    return 4;
                case YourItemType.Tags:
                    return 5;
                default:
                    return -1;
            }
        }

        public static int ToNumForFeed(this ItemBoxViewModel item)
        {
            switch (item.Type)
            {
                case YourItemType.Network:
                    return 0;
                case YourItemType.Popular:
                    return 1;
                case YourItemType.Recent:
                    return 2;
                default:
                    return -1;
            }
        }
    }
}