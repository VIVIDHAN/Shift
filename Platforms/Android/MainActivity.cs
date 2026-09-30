using Android.App;
using Android.Content.PM;
using Android.OS;

namespace DeliveryApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            // Set status bar to Deep Blue (#1E4ED8) with white text/icons
            Window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);
            Window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#1E4ED8"));
            
            // Remove LightStatusBar so the text/icons stay white (default for dark backgrounds)
            Window.DecorView.SystemUiVisibility = 0; 
        }
    }
}
