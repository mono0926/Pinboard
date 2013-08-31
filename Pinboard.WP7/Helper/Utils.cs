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

namespace Mono.App.Pinboard.Helper
{
    internal static class Utils
    {
        internal static string ToChar(string s)
        {
            char l = s.ToUpper()[0];
            if (l >= 'A' && l <= 'Z')
            {
                return l.ToString();
            }
            return "#";
        }
    }
}