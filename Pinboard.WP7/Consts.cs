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
    internal static class Consts
    {
        internal const int LimitItem = 20;
        internal const int LimitSocial = 10;
        internal const int InitialSize = 20;
        internal const int PinboardApiRestrictionInterval = 5;
        internal const int ReloadCheckInterval = 1;
        internal const string ViewFormat = "/View/{0}View.xaml";
        internal const string AuthorMailAddress = "mono0926@gmail.com";
        internal const string SupportMailTitle = "Pinboard for WP7 Support";
        internal const string SupportMailBody = "Hi, mono.";
        internal const string LastSynkKey = "last_sync";
        internal const string BackgroundKey = "background_key";
        internal const string ClipboardKey = "clipboard_key";
        internal const string SecretKey = "secret";
        internal const string AccountKey = "account";
        internal const string PasswordKey = "password";
        internal const string LogginMessage = "Loggin in now...";
        internal const string SavingMessa = "Saving your bookmark now...";
        internal const string DeletingMessage = "Deleting your bookmark now...";
        internal const string SyncingMessage = "Syncing your data...";
        internal const string LoadingTag = "Loading tag...";
        internal const string LoadingNow = "Now loading...";
    }
}