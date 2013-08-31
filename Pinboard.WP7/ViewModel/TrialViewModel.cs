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
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    public class TrialViewModel : MonoViewModelBase
    {
        private MarketplaceDetailTask marketplaceTask = new MarketplaceDetailTask();

        private ICommand purchaseCommand;

        public ICommand PurchaseCommand
        {
            get
            {
                return purchaseCommand ??
                    (purchaseCommand = new RelayCommand(() =>
                    {
                        marketplaceTask.Show();
                    }));
            }
        }
    }
}