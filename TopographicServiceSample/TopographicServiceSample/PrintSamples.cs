namespace TopographicServiceSample
{
    using System;
    using System.Threading.Tasks;

    public class PrintSamples
    {
        public async Task PrintGrandCanyonAsync()
        {
            var parameters = new Topographic.Service.Parameters
            {
                SouthLatitude = 36.0915,
                WestLongitude = -112.1615,
                NorthLatitude = 36.118083698675036,
                EastLongitude = -112.12408086284995,
            };

            var file = await Topographic.Service.GenerateTopographicModelAsync(parameters);

            var options = new Windows.System.LauncherOptions
            {
                TargetApplicationPackageFamilyName = "Microsoft.3DBuilder_8wekyb3d8bbwe"
            };

            await Windows.System.Launcher.LaunchFileAsync(file, options);
        }
    }
}
