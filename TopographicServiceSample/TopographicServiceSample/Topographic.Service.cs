namespace Topographic
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.AppService;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Foundation.Collections;
    using Windows.Graphics.Printing3D;
    using Windows.Storage;
    using Windows.System;
    using Windows.System.Threading;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    /// <summary>
    /// Topographic Service 
    /// </summary>
    public sealed class Service
    {
        #region Microsoft Affiliate Program
        /// <summary>
        // Put your Microsoft Affiliate program ID here to earn money when this 
        // http://www.microsoftaffiliates.com/ 
        /// </summary>
        private const string ProgramId = "Put your Affiliate program ID here";

        /// <summary>
        /// Put your Microsoft Affiliate site ID here.
        /// </summary>
        private const string SiteId = "Put your affiliate site ID here";
        #endregion

        #region Localizable Strings
        private const string DialogContent = "This app uses Topographic app to create 3D topographic models that can be printed to a 3D printer or printed via a 3D print service.  Press [Install] to go to the Windows Store to install the Topographic app.  Once the app is installed, please try again.";
        private const string DialogTitle = "Install Topographic app";
        private const string DialogInstall = "Install";
        private const string DialogCancel = "Cancel";
        #endregion

        #region Topographic Service Constants
        /// <summary>
        /// Do not change this
        /// </summary>
        private const string AppServiceName = "com.charette.topographic";

        /// <summary>
        /// Do not change this
        /// </summary>
        private const string PackageFamilyName = "49752MichaelS.Scherotter.Topographic_9eg5g21zq32qm";
        private Printing3D3MFPackage _package;
        private Parameters _parameters;
        #endregion

        private Service()
        {
        }

        #region Methods
        /// <summary>
        /// Generate a 3MF file that contains a 3D topographic model with an 
        /// optional map printed on the top
        /// </summary>
        /// <param name="southLatitude">south latitude in WGS84 coordinates</param>
        /// <param name="westLongitude">west longitude in WGS84 coordinates</param>
        /// <param name="northLatitude">north latitude in WGS84 coordinates</param>
        /// <param name="eastLongitude">east longitude in WGS84 coordinates</param>
        /// <param name="mapImage">(optional) the image of a map that will be 
        /// put on top of the model</param>
        /// <example>
        /// // this created a 3MF file of Hawaii and then send it to 3D Builder to print
        /// var file = await Topographic.Service.GenerateTopographicModelAsync(18.8749,-156.2731,20.3203,-154.7388, mapImage);
        /// 
        /// var PackageFamilyName3DBuilder = "Microsoft.3DBuilder_8wekyb3d8bbwe";
        /// 
        /// var options = new LauncherOptions
        /// {
        ///     TargetApplicationPackageFamilyName = PackageFamilyName3DBuilder,
        ///     FallbackUri = new Uri("ms-windows-store:PDP?PFN=" + PackageFamilyName3DBuilder);
        /// };
        /// 
        /// await Launcher.LaunchFileAsync(file);</example>
        /// <returns>a new 3MF file</returns>
        public static async Task<StorageFile> GenerateTopographicModelAsync(
            Parameters parameters)
        {
            var service = await ConnectAsync();

            if (service == null)
            {
                return null;
            }

            using (service)
            {
                var valueSet = new ValueSet();

                valueSet["SouthLatitude"] = parameters.SouthLatitude;
                valueSet["WestLongitude"] = parameters.WestLongitude;
                valueSet["NorthLatitude"] = parameters.NorthLatitude;
                valueSet["EastLongitude"] = parameters.EastLongitude;

                if (parameters.MapImage != null)
                {
                    valueSet["ImageToken"] = SharedStorageAccessManager.AddFile(parameters.MapImage);
                }

                var response = await service.SendMessageAsync(valueSet);

                if (response.Status == AppServiceResponseStatus.Success)
                {
                    var token = response.Message["FileToken"] as string;

                    // this is a 3MF file that you can now display, edit and print
                    var file = await SharedStorageAccessManager.RedeemTokenForFileAsync(token);

                    return file;
                }
            }

            return null;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the Microsoft Affiliate download URL
        /// </summary>
        private static Uri AffiliateDownloadUri
        {
            get
            {
                var storeUriString = string.Format(
                    CultureInfo.InvariantCulture,
                    "https://microsoft.td/pdp/?PFN={0}",
                    PackageFamilyName);

                var escapedUri = Uri.EscapeDataString(storeUriString);

                var uriString = string.Format(
                    CultureInfo.InvariantCulture,
                    "http://clkde.tradedoubler.com/click?p={0}&a={1}&g=0&url={2}",
                    ProgramId,
                    SiteId,
                    escapedUri);

                System.Diagnostics.Debug.WriteLine(uriString);

                return new Uri(uriString);
            }

        }
        #endregion

        #region Implementation
        /// <summary>
        /// Connect to the service
        /// </summary>
        /// <returns>an open app service connection if successful, null if the connection failed</returns>
        private static async Task<AppServiceConnection> ConnectAsync()
        {
            var service = new AppServiceConnection
            {
                AppServiceName = AppServiceName,
                PackageFamilyName = PackageFamilyName,
            };

            var status = await service.OpenAsync();

            switch (status)
            {
                case AppServiceConnectionStatus.Success:
                    return service;

                case AppServiceConnectionStatus.AppNotInstalled:
                    await ShowInstallDialogAsync();
                    return null;

                default:
                    return null;
            }
        }

        private static async Task ShowInstallDialogAsync()
        {
            var messageDialog = new MessageDialog(DialogContent, DialogTitle);

            messageDialog.CancelCommandIndex = 1;
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.Commands.Add(new UICommand(DialogInstall, OnInstall, "install"));
            messageDialog.Commands.Add(new UICommand(DialogCancel, delegate { }, "cancel"));

            await messageDialog.ShowAsync();
        }

        private static async void OnInstall(IUICommand command)
        {
            var options = new LauncherOptions
            {
                DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseHalf
            };

            await Launcher.LaunchUriAsync(AffiliateDownloadUri, options);
        }
        #endregion

        public struct Parameters
        {
            public double SouthLatitude;
            public double WestLongitude;
            public double NorthLatitude;
            public double EastLongitude;
            public StorageFile MapImage;
        }
    }
}
