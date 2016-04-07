# Topographic Service
Enable your Universal Windows App to use the [Topographic](https://www.microsoft.com/store/apps/9nblggh5xrh3) app for Windows to generate 3D topographic models of any location on Earth.
## Sample
See the [sample code](https://github.com/mscherotter/TopographicService/blob/master/TopographicServiceSample/TopographicServiceSample/PrintSamples.cs) 
to see how easy it is to generate Topographic maps given any 4 lat/long coordinates and an optional image.
`
namespace TopographicServiceSample
{
    using System;
    using System.Threading.Tasks;

    public class PrintSamples
    {
	    // Open a model of the Grand Canyon in 3D Builder, ready to print
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
`

