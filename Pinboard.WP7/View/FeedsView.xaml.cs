﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Mono.Framework.Mvvm.View;

namespace Mono.App.Pinboard.View
{
    public partial class FeedsView : MonoPage
    {
        public FeedsView()
        {
            InitializeComponent();
        }

        private void MonoPage_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}