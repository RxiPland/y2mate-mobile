using MvvmHelpers;
using MvvmHelpers.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using y2mate.API;
using y2mate.Config;
using y2mate.Models;
using y2mate.Views;

namespace y2mate.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        public ObservableRangeCollection<FoundVideoModel> foundVideosHistory { get; set; }
        public AsyncCommand RefreshCommand { get; }
        public AsyncCommand DeleteHistoryCommand { get; }


        FoundVideoModel _videoSelected;
        public FoundVideoModel VideoSelected
        {
            get => _videoSelected;
            set
            {
                if (value != null)
                {
                    //Application.Current.MainPage.Navigation.PushAsync(new SearchPage(value));
                    Shell.Current.GoToAsync($"//SearchPage?VideoUrl={Uri.EscapeDataString(value.VideoUrl)}");

                    value = null;
                }

                _videoSelected = value;
            }
        }

        public HistoryViewModel()
        {
            foundVideosHistory = new ObservableRangeCollection<FoundVideoModel>();
            DeleteHistoryCommand = new AsyncCommand(DeleteHistory);
        }

        public async Task LoadHistory()
        {
            ObservableRangeCollection<FoundVideoModel> TempHistory = await WebApi.LoadHistoryFromFile();
            foundVideosHistory.Clear();
            foundVideosHistory.AddRange(TempHistory);
        }
        
        public async Task DeleteHistory()
        {
            await WebApi.DeleteHistory();
            foundVideosHistory.Clear();
        }
    }
}
