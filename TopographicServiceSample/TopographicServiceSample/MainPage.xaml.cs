using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Graphics.Printing3D;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Threading;
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
        private Printing3D3MFPackage _package;
        private Geopoint _topRight;
        private Geopoint _bottomLeft;
        private StorageFile _imageFile;

        public MainPage()
        {
            this.InitializeComponent();

            this.Map.Style = Windows.UI.Xaml.Controls.Maps.MapStyle.Terrain;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void OnPrint(object sender, RoutedEventArgs e)
        {
            this.SetButtonsEnabled(false);

            await PrepareForPrintingAsync();

            await Task.Run(async delegate
            {
                 await CreatePackageAsync();
            });

            var manager = Print3DManager.GetForCurrentView();

            manager.TaskRequested += Manager_TaskRequested;

            await Print3DManager.ShowPrintUIAsync();

            manager.TaskRequested -= Manager_TaskRequested;

            this.SetButtonsEnabled(true);
        }

        /// <summary>
        /// Because of a bug, we need to copy the file to another stream
        /// </summary>
        /// <returns>an async task</returns>
        private async Task CreatePackageAsync()
        {
            var file = await Create3MFAsync();

            var fileStream = await file.OpenStreamForReadAsync();

            var memoryStream = new MemoryStream();

            await fileStream.CopyToAsync(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            this._package = await Printing3D3MFPackage.LoadAsync(memoryStream.AsRandomAccessStream());
        }

        private void Manager_TaskRequested(Windows.Graphics.Printing3D.Print3DManager sender, Windows.Graphics.Printing3D.Print3DTaskRequestedEventArgs args)
        {
            Print3DTaskSourceRequestedHandler sourceHandler = delegate (Print3DTaskSourceRequestedArgs sourceRequestedArgs)
            {
                sourceRequestedArgs.SetSource(_package);
            };

            args.Request.CreateTask("Topographic Map", "Default", sourceHandler);
        }

        private async Task PrepareForPrintingAsync()
        {
            this.Map.GetLocationFromOffset(new Point(this.Map.ActualWidth, 0), out _topRight);
            this.Map.GetLocationFromOffset(new Point(0, this.Map.ActualHeight), out _bottomLeft);

            this._imageFile = await CreateImageFileAsync();
        }

        private async System.Threading.Tasks.Task<Windows.Storage.StorageFile> Create3MFAsync()
        {
            var parameters = new Topographic.Service.Parameters
            {
                SouthLatitude = _bottomLeft.Position.Latitude,
                WestLongitude = _bottomLeft.Position.Longitude,
                NorthLatitude = _topRight.Position.Latitude,
                EastLongitude = _topRight.Position.Longitude,
                MapImage = _imageFile
            };

            var file = await Topographic.Service.GenerateTopographicModelAsync(parameters);
            return file;
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

        private async void OnSave(object sender, RoutedEventArgs e)
        {
            this.SetButtonsEnabled(false);

            await PrepareForPrintingAsync();

            var fileSavePicker = new FileSavePicker
            {
                SuggestedFileName = "Topography.3mf"
            };

            try
            {
                fileSavePicker.SuggestedStartLocation = PickerLocationId.Objects3D;
            }
            catch
            {
                fileSavePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            }

            fileSavePicker.FileTypeChoices.Add("3MF format", new string[] { ".3mf" });

            var file = await fileSavePicker.PickSaveFileAsync();

            if (file != null)
            {
                var file2 = await Create3MFAsync();

                await file2.MoveAndReplaceAsync(file);
            }

            this.SetButtonsEnabled(true);
        }

        private async void On3DBuilder(object sender, RoutedEventArgs e)
        {
            this.SetButtonsEnabled(false);

            await PrepareForPrintingAsync();

            var file = await Create3MFAsync();

            var options = new Windows.System.LauncherOptions();

            // Set 3d Builder as the target app
            options.TargetApplicationPackageFamilyName = "Microsoft.3DBuilder_8wekyb3d8bbwe";

            // Launch 3d Builder with any 2D image
            await Windows.System.Launcher.LaunchFileAsync(file, options);

            this.SetButtonsEnabled(true);
        }

        private void SetButtonsEnabled(bool isEnabled)
        {
            var commandBar = this.BottomAppBar as CommandBar;

            foreach (var button in commandBar.PrimaryCommands.OfType<AppBarButton>())
            {
                button.IsEnabled = isEnabled;
            }
        }
    }
}
