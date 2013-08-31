using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.App.Pinboard;
using Mono.App.Pinboard.Service;

namespace Pinboard.WP7.UnitTest.Service
{
    [TestClass]
    public class SettingServiceTest : SilverlightTest
    {
        private static IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        [TestInitialize]
        public void InitializeTest()
        {
            settings.Clear();
        }

        [TestMethod]
        [Asynchronous]
        public void WhenNotLoginNotSettingExpectDisabled()
        {
            PinboardService.GetInstance().Login("ssssxxxxxxx", "hogehoge")
                .Subscribe(login =>
                {
                    login.Is(false);
                    SettingsService.LoadEnabledClipboard()
                        .Subscribe(x =>
                        {
                            x.Is(false);
                            EnqueueTestComplete();
                        });
                });
        }

        [TestMethod]
        [Asynchronous]
        public void WhenLoginNotSettingExpectEnabled()
        {
            PinboardService.GetInstance().Login("monoXi", "hogehoge")
                .Subscribe(login =>
                {
                    login.Is(true);
                    SettingsService.LoadEnabledClipboard()
                        .Subscribe(x =>
                        {
                            x.Is(true);
                            EnqueueTestComplete();
                        });
                });
        }

        [TestMethod]
        [Asynchronous]
        public void WhenLoginSettingTrueExpectEnabled()
        {
            PinboardService.GetInstance().Login("monoXi", "hogehoge")
                .Subscribe(login =>
                {
                    login.Is(true);
                    SettingsService.SaveSetting(Consts.ClipboardKey, true);
                    SettingsService.LoadEnabledClipboard()
                        .Subscribe(x =>
                        {
                            x.Is(true);
                            EnqueueTestComplete();
                        });
                });
        }

        [TestMethod]
        [Asynchronous]
        public void WhenLoginNotSettingFalseExpectDisabled()
        {
            PinboardService.GetInstance().Login("monoXi", "hogehoge")
                .Subscribe(login =>
                {
                    login.Is(true);
                    SettingsService.SaveSetting(Consts.ClipboardKey, false);
                    SettingsService.LoadEnabledClipboard()
                        .Subscribe(x =>
                        {
                            x.Is(false);
                            EnqueueTestComplete();
                        });
                });
        }
    }
}