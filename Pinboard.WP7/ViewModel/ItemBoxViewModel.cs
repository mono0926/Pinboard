using System;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Mono.App.Pinboard.Service;
using Mono.App.Pinboard.ViewModel;
using Mono.Framework.Mvvm.Message;
using Mono.Framework.Mvvm.ViewModel;

namespace Mono.App.Pinboard.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class ItemBoxViewModel : MonoViewModelBase
    {
        public string Title { get; set; }

        public string ImageSource { get; set; }

        public YourItemType Type { get; set; }

        private int count;

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                ExecuteOnUIThread(() => RaisePropertyChanged(() => Count));
            }
        }
    }

    public enum YourItemType
    {
        All,
        Unread,
        Private,
        Public,
        Untagged,
        Tags,
        Network,
        Popular,
        Recent,
        Search,
        User,
        TagSearch,
    }
}