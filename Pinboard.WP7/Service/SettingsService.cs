using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Mono.App.Pinboard.Service
{
    public static class SettingsService
    {
        private static IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private static object obj = new object();

        public static void SaveSetting<T>(string key, T value)
        {
            lock (obj)
            {
                settings[key] = value;
                settings.Save();
            }
        }

        private static T LoadSetting<T>(string key)
        {
            lock (obj)
            {
                return (T)settings[key];
            }
        }

        public static T LoadSettingOrDefault<T>(string key, T defaultValue)
        {
            if (Contains(key))
            {
                return LoadSetting<T>(key);
            }
            else
            {
                return defaultValue;
            }
        }

        public static T LoadSettingOrDefault<T>(string key)
        {
            if (Contains(key))
            {
                return LoadSetting<T>(key);
            }
            else
            {
                return default(T);
            }
        }

        public static bool Contains(string key)
        {
            return settings.Contains(key);
        }

        public static void SaveBackgroundAsDefault()
        {
            SaveBackground(BackgroundType.Default);
        }

        public static void SaveBackgroundAsForest()
        {
            SaveBackground(BackgroundType.Forest);
        }

        public static void SaveBackground(BackgroundType type)
        {
            SaveSetting(Consts.BackgroundKey, type);
        }

        public static BackgroundType LoadBackgroundType()
        {
            return LoadSettingOrDefault<BackgroundType>(Consts.BackgroundKey);
        }

        public static bool IsEnabledClipboard()
        {
            return LoadSettingOrDefault<bool>(Consts.ClipboardKey, true);
        }

        public static IObservable<bool> LoadEnabledClipboard()
        {
            var enabledClipboard = IsEnabledClipboard();
            if (!enabledClipboard)
            {
                return new BehaviorSubject<bool>(enabledClipboard);
            }
            return PinboardService.GetInstance().IsLogin();
        }

        internal static void SaveAccountInfo(string account, string password, string secret)
        {
            settings[Consts.AccountKey] = account;
            settings[Consts.PasswordKey] = password;
            settings[Consts.SecretKey] = secret;
            settings.Save();
        }
    }

    public enum BackgroundType
    {
        Default,
        Forest,
    }

    public static class BackgroudTypeExtension
    {
        private static string defaultThemeName = "Default Theme";

        public static string ConvertToString(this BackgroundType type)
        {
            switch (type)
            {
                case BackgroundType.Default:
                    return defaultThemeName;
                case BackgroundType.Forest:
                    return "Original Theme (Forest)";
                default:
                    return defaultThemeName;
            }
        }

        public static BackgroundType ToBackgroudType(this string name)
        {
            return (BackgroundType)Enum.Parse(typeof(BackgroundType), name, true);
        }
    }
}