using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
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

        private void DownloadButton_Clicked(object sender, EventArgs e)
        {

        }
    }
}