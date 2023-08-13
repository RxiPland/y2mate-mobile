using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace y2mate.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            InitializeComponent();
        }

        async private void y2mateWebButton_Clicked(object sender = null, EventArgs e = null)
        {
            await Browser.OpenAsync("https://y2mate.com", BrowserLaunchMode.SystemPreferred);
        }

        async private void SourceCodeButton_Clicked(object sender = null, EventArgs e = null)
        {
            await Browser.OpenAsync("https://github.com/RxiPland/y2mate-mobile", BrowserLaunchMode.SystemPreferred);
        }
    }
}