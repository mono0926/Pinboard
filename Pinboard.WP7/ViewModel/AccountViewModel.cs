using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Tasks;
using Mono.App.Pinboard.Service;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class AccountViewModel : TrialViewModel
    {
        public AccountViewModel()
        {
            Account = SettingsService.LoadSettingOrDefault<string>(Consts.AccountKey, string.Empty);
            Password = SettingsService.LoadSettingOrDefault<string>(Consts.PasswordKey, string.Empty);
        }

        public string Account { get; set; }

        public string Password { get; set; }

        public bool IsTrialVisible
        {
            get { return PinboardService.GetInstance().IsTrial; }
        }

        private string progressMessage;

        public string ProgressMessage
        {
            get { return progressMessage; }
            set
            {
                progressMessage = value;
                RaisePropertyChanged(() => ProgressMessage);
            }
        }

        private bool isProgress;

        public bool IsProgress
        {
            get { return isProgress; }
            set
            {
                isProgress = value;
                RaisePropertyChanged(() => IsProgress);
            }
        }

        #region Command

        private ICommand singInCommand;

        public ICommand SignInCommand
        {
            get
            {
                return singInCommand ??
                    (singInCommand = new RelayCommand(() =>
                    {
                        ProgressMessage = Consts.LogginMessage;
                        IsProgress = true;

                        PinboardService.GetInstance().Login(Account, Password)
                            .Subscribe(result =>
                            {
                                if (result)
                                {
                                    Messenger.Send(new MyDialogMessage(MessageKeys.LoginSuccessKey));
                                    Messenger.Send(new GenericMessage<string>(MessageKeys.GoBackKey));
                                }
                                else
                                {
                                    var message = new MyDialogMessage(MessageKeys.LoginFailKey);
                                    Messenger.Send(message);
                                }
                                ProgressMessage = string.Empty;
                                IsProgress = false;
                            },
                            ex =>
                            {
                                Messenger.Send(new MyDialogMessage(MessageKeys.ErrorKey));
                                ProgressMessage = string.Empty;
                                IsProgress = false;
                            });
                    }));
            }
        }

        private ICommand closeCommand;

        public ICommand CloseCommand
        {
            get
            {
                return closeCommand ??
                    (closeCommand = new RelayCommand(() =>
                    {
                        Messenger.Send(new GenericMessage<string>(MessageKeys.GoBackKey));
                    }));
            }
        }

        #endregion Command
    }
}