# Topographic Service
Enable your Universal Windows App to use the [Topographic](https://www.microsoft.com/store/apps/9nblggh5xrh3) app for Windows to generate 3D topographic models of any location on Earth.
## Sample
The [TopographicServiceSample](https://github.com/mscherotter/TopographicService/tree/master/TopographicServiceSample) 
project contains the [Topographic.Service](https://github.com/mscherotter/TopographicService/blob/master/TopographicServiceSample/TopographicServiceSample/Topographic.Service.cs) 
class which has all the code to communicate with the Topographic app.  

If you want to use the Topographic app service, then you should sign up with 
the [Microsoft Affiliate Program](http://microsoftaffiliates.com/) so you can earn referral revenue.  Watch [this
video](https://channel9.msdn.com/Blogs/Windows-Store/Using-the-Microsoft-Affiliate-Program-to-earn-additional-7-on-Windows-Store-sales) for more details.

See the [sample code](https://github.com/mscherotter/TopographicService/blob/master/TopographicServiceSample/TopographicServiceSample/PrintSamples.cs) 
to see how easy it is to generate Topographic maps given any 4 lat/long coordinates and an optional image.

```
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
                NorthLatitude = 36.1180,
                EastLongitude = -112.1240,
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
```

