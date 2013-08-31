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

namespace Mono.App.Pinboard
{
    public class MessageKeys
    {
        public static string ClipboardKey { get { return "ClipboardKey"; } }

        public static string TransitionKey { get { return "TransitionKey"; } }

        public static string LoginFailKey { get { return "LoginFailKey"; } }

        public static string LoginSuccessKey { get { return "LoginSuccess"; } }

        public static string GoBackKey { get { return "GoBack"; } }

        public static string ErrorKey { get { return "Error"; } }

        public static string DeleteBookmarkConfirmKey { get { return "DeleteBookmarkConfirm"; } }

        public static string SaveBookmarkFailKey { get { return "SaveBookmarkFail"; } }
    }
}