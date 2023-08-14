using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using y2mate.Config;
using y2mate.ViewModels;

namespace y2mate.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoryPage : ContentPage
	{
		public HistoryPage ()
		{
			InitializeComponent ();
            BindingContext = new HistoryViewModel();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is HistoryViewModel viewModel)
            {
                await viewModel.LoadHistory();
            }
        }


        private void HistoryItemsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }

        private void BackToStartButton_Clicked(object sender, EventArgs e)
        {
            var listItems = HistoryItemsListView.ItemsSource.OfType<object>();

            if (listItems.Count() <= 0)
            {
                return;
            }

            HistoryItemsListView.ScrollTo(listItems.First(), ScrollToPosition.Start, true);
        }
    }
}