using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Mono.App.Pinboard.Model;
using Mono.App.Pinboard.Service;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class SettingsViewModel : MonoViewModelBase
    {
        private IList<BackgroundItem> backgrounds;

        public SettingsViewModel()
        {
            backgrounds = typeof(BackgroundType).GetFields().Where(x => x.IsLiteral).Select(x => x.Name.ToBackgroudType()).Select(x => new BackgroundItem { Type = x }).ToList();

            var selected = backgrounds.First(x => x.Type == SettingsService.LoadBackgroundType());
            SelectedIndex = backgrounds.IndexOf(selected);
            IsEnabledClipboard = SettingsService.IsEnabledClipboard();
        }

        public IList<BackgroundItem> Backgrounds { get { return backgrounds; } }

        private int selectedIndex;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                RaisePropertyChanged(() => SelectedIndex);
            }
        }

        private bool isEnabledClipboard;

        public bool IsEnabledClipboard
        {
            get { return isEnabledClipboard; }
            set
            {
                isEnabledClipboard = value;
                RaisePropertyChanged(() => IsEnabledClipboard);
            }
        }

        private ICommand saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                return saveCommand ??
                    (saveCommand = new RelayCommand(() =>
                    {
                        var type = Backgrounds[SelectedIndex].Type;
                        SettingsService.SaveBackground(type);
                        SettingsService.SaveSetting(Consts.ClipboardKey, IsEnabledClipboard);

                        GlobalMessenger.Send(new GenericMessage<BackgroundType>(type));
                        Messenger.Send(new GenericMessage<string>(MessageKeys.GoBackKey));
                    }));
            }
        }
    }
}