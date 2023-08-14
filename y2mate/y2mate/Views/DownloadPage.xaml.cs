using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    public partial class DownloadPage : ContentPage
    {
        public FoundVideoModel VideoItem { get; }
        private bool terminateRequest = false;

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
            if (downloadButton.Text.ToLower() == "zrušit stahování")
            {
                terminateRequest = true;
                return;
            }


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
                    await DisplayAlert("Nastala chyba!", Messages.KeysNotFound, "OK");

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
                    await DisplayAlert("Nastala chyba!", Messages.KeysNotFound, "OK");

                    return;
                }
            }
            else
            {
                // Error message
                await DisplayAlert("Nastala chyba!", $"Musí se vybrat mezi mp3/mp4 !", "OK");
                return;
            }



            JObject response = await WebApi.GetVideoUrl(VideoItem, Y2mateVideoKey);

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

                    } else if (convertedStatus == "FAILED")
                    {
                        // Not converted yet
                        await DisplayAlert("Chyba", Messages.AnErrorHasOccurred, "OK");
                        await SetLoadingToButton(false);

                        return;
                    }
                    else
                    {
                        // Not converted yet
                        await DisplayAlert("Oznámení", Messages.VideoStillConverting, "OK");
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


            try
            {
                await DownloadFile(DownloadLink);
            }
            catch (Exception ex)
            {
                if (terminateRequest)
                {
                    await DisplayAlert("Oznámení", "Stahování bylo zrušeno.", "OK");
                }
                else
                {
                    // Error message
                    await DisplayAlert("Nastala chyba!", $"{ex.Message}", "OK");
                }
            }
            finally
            {
                DownloadLabel.IsVisible = false;
                DownloadProgressBar.IsVisible = false;
                DownloadProgressBar.Progress = 0;

                DownloadedDataLabel.IsVisible = false;
                DownloadedDataLabel.Text = string.Empty;

                downloadButton.Text = "Stáhnout";

                await SetLoadingToButton(false);
                terminateRequest = false;

                // Enable button back
                Shell.SetBackButtonBehavior(this, new BackButtonBehavior
                {
                    IsEnabled = true
                });
            }

            await SetLoadingToButton(false);
        }

        async private Task DownloadFile(string fileUrl)
        {
            // Download file to Android's download folder

            terminateRequest = false;

            string videoName = WebApi.RemoveForbiddenChars(videoTitle.Text);
            if (string.IsNullOrEmpty(videoName))
            {
                // Replace name with MD5 hash if empty

                videoName = WebApi.GenerateMD5Hash(DateTime.Now.ToString());
                await DisplayAlert("Zpráva", $"Název videa se nepodařilo získat a tudíž byl nahrazen náhodnými znaky.", "Rozumím");
            }


            bool doesExist;
            bool rename;
            string videoFormat;
            string filePath;

            while (true)
            {
                // Ask user to select name
                string newVideoName = await DisplayPromptAsync("Uložit", "Zvolte název souboru:", cancel: "Zrušit stahování", initialValue: videoName);

                if (newVideoName == null)
                {
                    // Download canceled by user

                    await SetLoadingToButton(false);
                    return;
                }

                newVideoName = WebApi.RemoveForbiddenChars(newVideoName);

                if (!string.IsNullOrEmpty(newVideoName))
                {
                    videoName = newVideoName.ToString();
                }

                videoFormat = PickerFormat.SelectedItem.ToString().Split(' ')[0];
                filePath = Path.Combine(GlobalConfig.DownloadFolder, $"{videoName}.{videoFormat}");

                doesExist = System.IO.File.Exists(filePath);

                if (doesExist)
                {
                    rename = await DisplayAlert("Problém", $"Video se stejným názvem již ve složce existuje.", "Přejmenovat", "Přepsat");

                    if (rename)
                    {
                        // Back to selecting name
                        continue;
                    }
                    else
                    {
                        // Overwrite (delete) file
                        System.IO.File.Delete(filePath);
                        break;
                    }
                }
                else
                {
                    // Does not exists
                    break;
                }
            }


            // Disable button back
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                //IconOverride = "icon_transparent.png",
                IsEnabled = false
            });

            await SetLoadingToButton(false);
            downloadButton.Text = "Zrušit stahování";

            // Show download widgets
            DownloadLabel.IsVisible = true;
            DownloadProgressBar.IsVisible = true;
            DownloadedDataLabel.IsVisible = true;

            PickerFormat.IsEnabled = false;
            PickerQuality.IsEnabled = false;



            using HttpClient httpClient = new HttpClient();

            // Creating request
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, fileUrl);

            request.Headers.Add("Referer", "https://www.y2mate.com/");
            request.Headers.Add("user-agent", GlobalConfig.UserAgent);

            // Sending GET request
            using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            using Stream responseStream = await response.Content.ReadAsStreamAsync();

            long totalBytes = response.Content.Headers.ContentLength ?? -1;
            long receivedBytes = 0;
            byte[] buffer = new byte[4096];
            int bytesRead;

            // Opening file
            using (Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    // Terminate download
                    if (terminateRequest)
                    {
                        System.IO.File.Delete(filePath);

                        httpClient.CancelPendingRequests();
                        httpClient.Dispose();
                        responseStream.Close();
                        responseStream.Dispose();
                        return;
                    }

                    // Writing to file
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    receivedBytes += bytesRead;
                    double progress = (double)receivedBytes / totalBytes;
                    DownloadProgressBar.Progress = progress;
                    DownloadedDataLabel.Text = $"{Math.Round(receivedBytes / 1_000_000.0, 2)} / {Math.Round(totalBytes / 1_000_000.0, 2)} MB";
                }
            }


            bool OpenFile = await DisplayAlert("Úspěch", $"Video bylo úspěšně staženo do složky Downloads.", "Otevřít", "OK");
            await Shell.Current.GoToAsync("..");

            if (OpenFile)
            {
                await Launcher.OpenAsync
                    (new OpenFileRequest()
                    {
                        File = new ReadOnlyFile(filePath)
                    }
                );
            }
        }
    }
}