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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.App.Pinboard.ViewModel;

namespace Pinboard.WP7.UnitTest.ViewModel
{
    [TestClass]
    public class MainViewModelTest
    {
        [TestMethod]
        [Ignore]
        public void LoginTest()
        {
            var vm = new MainViewModel();
            vm.InitializeCommand.Execute(null);
        }
    }
}