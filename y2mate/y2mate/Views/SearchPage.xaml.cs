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
using y2mate.Models;

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
                    await RestoreWidgets(0);
                    return;
                }


                // Check for JSON keys
                foreach (string key in GlobalConfig.RequiredSearchResponseKeys)
                {
                    if (!LoadedResponse.ContainsKey(key))
                    {
                        // Error message
                        await ShowErrorMessage(Messages.KeyFromResponseNotFound.Replace("{key}", key));
                        await RestoreWidgets(0);

                        return;
                    }
                }


                string responseStatus = LoadedResponse["status"]?.ToString() ?? string.Empty;
                string responseMessage = LoadedResponse["mess"]?.ToString() ?? string.Empty;

                if (responseMessage.Contains("Please enter valid video URL.") || responseMessage.Contains("Sorry! An error has occurred."))
                {
                    // Video doesn't exists error
                    await ShowErrorMessage(Messages.VideoNotExists);
                    await RestoreWidgets(0);

                    return;
                }
                else if (responseStatus.ToLower() != "ok")
                {
                    // Unknown error
                    await ShowErrorMessage($"Nastala neznámá chyba. Y2mate vrátil:\n\n{response}");
                    await RestoreWidgets(0);

                    return;
                }

                FoundVideoModel VideoItem = new FoundVideoModel();
                VideoItem.VideoUrl = VideoUrl ?? string.Empty;
                VideoItem.VideoId = LoadedResponse["vid"]?.ToString() ?? string.Empty;
                VideoItem.VideoTitle = LoadedResponse["title"]?.ToString() ?? string.Empty;
                VideoItem.VideoDurationTimeSec = int.Parse(LoadedResponse["t"]?.ToString() ?? "0");
                VideoItem.YtChannel = LoadedResponse["a"]?.ToString() ?? string.Empty;

                try
                {
                    JObject QualitiesJson = (JObject)LoadedResponse["links"];

                    JObject mp3Json = (JObject)QualitiesJson["mp3"];

                    foreach (var mp3 in mp3Json)
                    {
                        JObject mp3QualityTemp = (JObject)mp3.Value;

                        if (mp3QualityTemp["f"].ToString() == "mp3")
                        {
                            VideoItem.AvailableMP3.Add(mp3QualityTemp["q"].ToString().Trim(), mp3QualityTemp["k"].ToString().Trim());
                        }
                    }


                    JObject mp4Json = (JObject)QualitiesJson["mp4"];

                    foreach (var mp4 in mp4Json)
                    {
                        JObject mp4QualityTemp = (JObject)mp4.Value;

                        if (mp4QualityTemp["f"].ToString() == "mp4")
                        {
                            VideoItem.AvailableMP4.Add(mp4QualityTemp["q"].ToString().Trim(), mp4QualityTemp["k"].ToString().Trim());
                        }
                    }
                }
                catch(Exception ex)
                {
                    // Unknown error
                    await ShowErrorMessage($"Nastala chyba: {ex.Message}");
                    await RestoreWidgets(0);

                    return;
                }


                await Navigation.PushAsync(new DownloadPage(VideoItem));

                InnerActivityIndicator.Opacity = 0;
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