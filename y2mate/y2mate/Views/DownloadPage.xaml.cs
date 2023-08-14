using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using y2mate.API;
using y2mate.Config;
using y2mate.Models;

namespace y2mate.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadPage : ContentPage
    {
        public FoundVideoModel VideoItem { get; }

        public DownloadPage(FoundVideoModel VideoItem)
        {
            InitializeComponent();
            this.VideoItem = VideoItem;

            SetValues();
        }

        private void SetValues()
        {
            videoTitle.Text = VideoItem.VideoTitle;

            // Seconds to time format
            TimeSpan videoDurationTime = TimeSpan.FromSeconds(VideoItem.VideoDurationTimeSec);
            videoLength.Text = videoDurationTime.ToString();
            ytChannel.Text = VideoItem.YtChannel;

            PickerFormat.Items.Add("mp3 (pouze zvuk)");
            PickerFormat.Items.Add("mp4 (video)");
        }

        private async Task SetLoadingToButton(bool value = true)
        {
            // Set button spinning (loading)

            if (value)
            {
                downloadButton.IsEnabled = false;
                PickerFormat.IsEnabled = false;
                PickerQuality.IsEnabled = false;

                await InnerActivityIndicator.FadeTo(1, 400);
            }
            else
            {
                await InnerActivityIndicator.FadeTo(0, 400);

                downloadButton.IsEnabled = true;
                PickerFormat.IsEnabled = true;
                PickerQuality.IsEnabled = true;

            }
        }


        private void PickerFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            PickerQuality.Items.Clear();
            downloadButton.IsEnabled = false;

            string format = PickerFormat.SelectedItem.ToString();

            if (format.Contains("mp3"))
            {
                foreach (string key in VideoItem.AvailableMP3.Keys)
                {
                    if (key.Contains("128"))
                    {
                        PickerQuality.Items.Add(key + " (Standard)");
                        continue;
                    }

                    PickerQuality.Items.Add(key);
                }

                PickerQuality.IsVisible = true;
            }
            else if (format.Contains("mp4"))
            {
                foreach (string key in VideoItem.AvailableMP4.Keys)
                {
                    PickerQuality.Items.Add(key);
                }

                PickerQuality.IsVisible = true;
            }
            else
            {
                PickerQuality.IsVisible = false;
            }
        }

        private void PickerQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            downloadButton.IsEnabled = true;
        }

        private async void DownloadButton_Clicked(object sender, EventArgs e)
        {

            await SetLoadingToButton(true);

            string Y2mateVideoKey;

            if (PickerFormat.SelectedItem.ToString().Contains("mp3"))
            {
                string Quality = PickerQuality.SelectedItem.ToString();
                Quality = Quality.Split(' ')[0].ToString();

                if (VideoItem.AvailableMP3.Count > 0)
                {
                    Y2mateVideoKey = VideoItem.AvailableMP3[Quality];
                }
                else
                {
                    // Error message
                    await DisplayAlert("Nastala chyba!", $"Nepodařilo se najít klíče k videím!", "OK");

                    return;
                }

            }
            else if (PickerFormat.SelectedItem.ToString().Contains("mp4"))
            {
                string Quality = PickerQuality.SelectedItem.ToString();
                Quality = Quality.Split(' ')[0].ToString();

                if (VideoItem.AvailableMP4.Count > 0)
                {
                    Y2mateVideoKey = VideoItem.AvailableMP4[Quality];
                }
                else
                {
                    // Error message
                    await DisplayAlert("Nastala chyba!", $"Nepodařilo se najít klíče k videím!", "OK");

                    return;
                }
            }
            else
            {
                // Error message
                await DisplayAlert("Nastala chyba!", $"Musí se vybrat mezi mp3/mp4 !", "OK");
                return;
            }



            JObject response = await WebApi.DownloadVideo(VideoItem, Y2mateVideoKey);

            bool Success = bool.Parse(response["Success"]?.ToString() ?? "false");
            string ErrorMessage = response["ErrorMessage"]?.ToString() ?? string.Empty;
            string DownloadLink;

            if (Success && (ErrorMessage == string.Empty))
            {
                string responseHtml = response["ResponseHtml"]?.ToString() ?? string.Empty;

                JObject LoadedResponse;
                try
                {
                    LoadedResponse = JObject.Parse(responseHtml);

                    string responseStatus = LoadedResponse["status"]?.ToString() ?? string.Empty;
                    string responseMessage = LoadedResponse["mess"]?.ToString() ?? string.Empty;
                    string convertedStatus = LoadedResponse["c_status"]?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(responseMessage))
                    {
                        // Error in message
                        await DisplayAlert("Zpráva", $"Y2mate vrátil zprávu:\n\n{response}", "OK");
                        await SetLoadingToButton(false);

                        return;
                    }
                    else if (responseStatus.ToLower() != "ok")
                    {
                        // Unknown error
                        await DisplayAlert("Nastala chyba!", $"Nastala neznámá chyba. Y2mate vrátil:\n\n{response}", "OK");
                        await SetLoadingToButton(false);

                        return;
                    }


                    if (convertedStatus == "CONVERTED")
                    {
                        DownloadLink = LoadedResponse["dlink"]?.ToString() ?? string.Empty;

                    }
                    else
                    {
                        // Not converted yet
                        await DisplayAlert("Oznámení", $"Soubor se ještě připravuje. Zkuste to za chvíli.", "OK");
                        await SetLoadingToButton(false);

                        return;
                    }


                }
                catch (Exception)
                {
                    await DisplayAlert("Nastala chyba!", Messages.EmptyResponseError, "OK");
                    await SetLoadingToButton(false);
                    return;
                }

            }
            else
            {
                await DisplayAlert("Nastala chyba!", ErrorMessage, "OK");
                await SetLoadingToButton(false);
                return;
            }


            if (string.IsNullOrEmpty(DownloadLink))
            {
                // No download link error
                await DisplayAlert("Nastala chyba!", $"Nepodařilo se získat odkaz na stažení souboru! Server vrátil:\n\n{response}", "OK");
                await SetLoadingToButton(false);

                return;
            }

            await DisplayAlert("Odkaz", $"{DownloadLink}", "OK");
            await SetLoadingToButton(false);
        }
    }
}