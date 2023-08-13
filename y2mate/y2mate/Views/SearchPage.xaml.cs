using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using y2mate.API;
using y2mate.Config;

namespace y2mate.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage
    {
        public SearchPage()
        {
            InitializeComponent();
        }

        private async Task ShowErrorMessage(string Message, uint Length = 600)
        {
            ErrorMessageLabel.Text = Message;

            ErrorMessageLabel.FadeTo(1, Length);
            await DismissButton.FadeTo(1, Length);
        }

        private async Task HideErrorMessage(uint Length = 400)
        {
            DismissButton.FadeTo(0, Length);
            await ErrorMessageLabel.FadeTo(0, Length);

            ErrorMessageLabel.Text = string.Empty;
        }

        private async Task StartActivityIndicator(bool Start = true)
        {
            if (Start)
            {
                InnerActivityIndicator.IsRunning = true;

                ClearButton.FadeTo(0, 200);
                await SearchButton.FadeTo(0, 200);

                await InnerActivityIndicator.FadeTo(1, 500);

                if (ErrorMessageLabel.Opacity > 0)
                {
                    await HideErrorMessage(250);
                }

            }
            else
            {
                InnerActivityIndicator.IsRunning = false;

                await InnerActivityIndicator.FadeTo(0, 200);
                
                ClearButton.FadeTo(1, 500);
                await SearchButton.FadeTo(1, 500);
            }
        }

        private async void DismissButton_Clicked(object sender = null, EventArgs e = null)
        {
            await HideErrorMessage(500);
        }

        private void VideoUrlEntry_TextChanged(object sender, TextChangedEventArgs e)
        {

            SearchButton.IsEnabled = !string.IsNullOrEmpty(VideoUrlEntry.Text);
            ClearButton.IsEnabled = !string.IsNullOrEmpty(VideoUrlEntry.Text);
        }

        private void ClearButton_Clicked(object sender = null, EventArgs e = null)
        {
            VideoUrlEntry.Text = string.Empty;
        }


        private async void SearchButton_Clicked(object sender = null, EventArgs e = null)
        {
            // Search for video

            VideoUrlEntry.IsEnabled = false;
            StartActivityIndicator(true);

            string VideoUrl = VideoUrlEntry.Text;
            JObject response = await WebApi.SearchForVideo(VideoUrl);

            bool Success = bool.Parse(response["Success"]?.ToString() ?? "false");
            string ErrorMessage = response["ErrorMessage"]?.ToString() ?? string.Empty;

            if (Success && (ErrorMessage == string.Empty))
            {
                string responseHtml = response["ResponseHtml"]?.ToString() ?? string.Empty;


                JObject LoadedResponse;
                try
                {
                    LoadedResponse = JObject.Parse(responseHtml);
                }
                catch (Exception)
                {
                    await ShowErrorMessage(Messages.EmptyResponseError);
                    RestoreWidgets(0);
                    return;
                }

                InnerActivityIndicator.Opacity = 0;
                //await Navigation.PushAsync(new DownloadPage(LoadedResponse));
                await Navigation.PushAsync(new DownloadPage());

                RestoreWidgets(1000);
            }
            else
            {
                // Error

                await RestoreWidgets(0);
                await ShowErrorMessage(ErrorMessage);
            }
        }

        private async Task RestoreWidgets(int DelayMs = 1000)
        {
            // Wait for user to finish exiting this Page
            await Task.Delay(DelayMs);

            VideoUrlEntry.IsEnabled = true;
            ClearButton_Clicked();

            await StartActivityIndicator(false);
        }
    }
}