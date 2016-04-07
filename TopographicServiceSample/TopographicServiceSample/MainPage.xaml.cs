using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TopographicServiceSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.Map.Style = Windows.UI.Xaml.Controls.Maps.MapStyle.Terrain;
        }

        private async void OnPrint(object sender, RoutedEventArgs e)
        {
            Geopoint topRight, bottomLeft;

            this.Map.GetLocationFromOffset(new Point(this.Map.ActualWidth, 0), out topRight);
            this.Map.GetLocationFromOffset(new Point(0, this.Map.ActualHeight), out bottomLeft);

            var imageFile = await CreateImageFileAsync();

            var parameters = new Topographic.Service.Parameters
            {
                SouthLatitude = bottomLeft.Position.Latitude,
                WestLongitude = bottomLeft.Position.Longitude,
                NorthLatitude = topRight.Position.Latitude,
                EastLongitude = topRight.Position.Longitude,
                MapImage = imageFile
            };

            try
            {
                var file = await Topographic.Service.GenerateTopographicModelAsync(parameters);

                var options = new Windows.System.LauncherOptions();

                // Set 3d Builder as the target app
                options.TargetApplicationPackageFamilyName = "Microsoft.3DBuilder_8wekyb3d8bbwe";

                // Launch 3d Builder with any 2D image
                await Windows.System.Launcher.LaunchFileAsync(file, options);
            }
            catch (System.Exception se)
            {

            }
        }

        private async System.Threading.Tasks.Task<Windows.Storage.StorageFile> CreateImageFileAsync()
        {
            var bitmap = new RenderTargetBitmap();

            await bitmap.RenderAsync(this.Map);

            var pixels = await bitmap.GetPixelsAsync();

            var imageFile = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync("map.png", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            using (var imageStream = await imageFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
            {

                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, imageStream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    pixels.ToArray());

                await encoder.FlushAsync();
            }

            return imageFile;
        }
    }
}
