using System;
using System.Data.Linq;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.ViewModel;

namespace Mono.App.Pinboard.Model
{
    public class AppContext : DataContext
    {
        public AppContext()
            : base("DataSource=isostore:/myapp.sdf;")
        {
            Debug.WriteLine(DateTime.Now + " appcontext");
        }

        public Table<ItemBase> Items;
        public Table<Tag> Tags;
    }
}