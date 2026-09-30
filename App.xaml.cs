using Microsoft.Extensions.DependencyInjection;

namespace DeliveryApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		Page rootPage;
		string savedPhone = Preferences.Get("DriverPhone", "");
		
		if (!string.IsNullOrEmpty(savedPhone))
		{
			rootPage = new Views.DashboardPage();
		}
		else
		{
			rootPage = new Views.LoginPage();
		}

		var navPage = new NavigationPage(rootPage);
		navPage.BarBackgroundColor = Color.FromArgb("#1E3A8A"); // Using our primary blue
		navPage.BarTextColor = Colors.White;
		return new Window(navPage);
	}
}