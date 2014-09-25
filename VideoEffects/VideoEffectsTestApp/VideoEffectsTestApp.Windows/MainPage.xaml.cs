﻿using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VideoEffects;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoEffectsTestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!ShaderEffectDefinitionNv12.TestGraphicsDeviceSupport())
            {
                EffectTypeShaderNv12.Foreground = new SolidColorBrush(Colors.Gray);
                EffectTypeShaderNv12.IsEnabled = false;
            }
        }

        private async void Transcode_Click(object sender, RoutedEventArgs e)
        {
            Transcode.IsEnabled = false;

            StorageFile source = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Car.mp4"));
            StorageFile destination = await KnownFolders.VideosLibrary.CreateFileAsync("VideoEffectsTestApp.MediaTranscoder.mp4", CreationCollisionOption.ReplaceExisting);

            var definition = await CreateEffectDefinitionAsync();

            var transcoder = new MediaTranscoder();
            transcoder.AddVideoEffect(definition.ActivatableClassId, true, definition.Properties);

            PrepareTranscodeResult transcode = await transcoder.PrepareFileTranscodeAsync(source, destination, MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Qvga));
            await transcode.TranscodeAsync();

            Transcode.IsEnabled = true;
        }

        private async void StartMediaCapturePreview_Click(object sender, RoutedEventArgs e)
        {
            StartCaptureElementPreview.IsEnabled = false;

            // Skip if no camera
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            if (devices.Count == 0)
            {
                return;
            }

            var definition = await CreateEffectDefinitionAsync();

            var capture = new MediaCapture();
            await capture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video
            });
            await capture.AddEffectAsync(MediaStreamType.VideoPreview, definition.ActivatableClassId, definition.Properties);

            CapturePreview.Source = capture;
            await capture.StartPreviewAsync();

            StartCaptureElementPreview.IsEnabled = true;
        }

        private async void StartMediaCaptureRecord_Click(object sender, RoutedEventArgs e)
        {
            StartCaptureElementRecord.IsEnabled = false;

            // Skip if no camera
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            if (devices.Count == 0)
            {
                return;
            }

            StorageFile destination = await KnownFolders.VideosLibrary.CreateFileAsync("VideoEffectsTestApp.MediaCapture.mp4", CreationCollisionOption.ReplaceExisting);

            var definition = await CreateEffectDefinitionAsync();

            var capture = new MediaCapture();
            await capture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video
            });
            await capture.AddEffectAsync(MediaStreamType.VideoRecord, definition.ActivatableClassId, definition.Properties);

            await capture.StartRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Qvga), destination);

            await Task.Delay(3000);

            await capture.StopRecordAsync();

            StartCaptureElementRecord.IsEnabled = true;
        }

        private async void StartMediaElementPreview_Click(object sender, RoutedEventArgs e)
        {
            StartMediaElementPreview.IsEnabled = false;

            // Cleanup any previous playback
            MediaElementPreview.Stop();
            MediaElementPreview.Source = null;
            MediaElementPreview.RemoveAllEffects();

            var definition = await CreateEffectDefinitionAsync();

            MediaElementPreview.Source = new Uri("ms-appx:///Assets/Car.mp4");
            MediaElementPreview.AddVideoEffect(definition.ActivatableClassId, false, definition.Properties);

            StartMediaElementPreview.IsEnabled = true;
        }

        private async Task<IVideoEffectDefinition> CreateEffectDefinitionAsync()
        {
            switch (EffectType.SelectedIndex)
            {
                case 0: 
                    return new LumiaEffectDefinition(() =>
                {
                    return new IFilter[]
                    {
                        new AntiqueFilter(),
                        new FlipFilter(FlipMode.Horizontal)
                    };
                });

                case 1:
                    IBuffer shaderY = await PathIO.ReadBufferAsync("ms-appx:///Invert_093_NV12_Y.cso");
                    IBuffer shaderUV = await PathIO.ReadBufferAsync("ms-appx:///Invert_093_NV12_UV.cso");
                    return new ShaderEffectDefinitionNv12(shaderY, shaderUV);

                case 2:
                    IBuffer shader = await PathIO.ReadBufferAsync("ms-appx:///Invert_093_RGB32.cso");
                    return new ShaderEffectDefinitionBgrx8(shader);

                default:
                    throw new ArgumentException("Invalid effect type");
            }
        }

        private void ClearPreview_Click(object sender, RoutedEventArgs e)
        {
            if (CapturePreview.Source != null)
            {
                CapturePreview.Source.Dispose();
                CapturePreview.Source = null;
            }

            MediaElementPreview.Stop();
            MediaElementPreview.RemoveAllEffects();
            MediaElementPreview.Source = null;
        }
    }
}
