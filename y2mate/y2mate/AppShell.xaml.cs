using System;
using System.Collections.Generic;
using Xamarin.Forms;
using y2mate.Views;

namespace y2mate
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void OnMenuItemClicked(object sender = null, EventArgs e = null)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
