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
		var navPage = new NavigationPage(new Views.LoginPage());
		navPage.BarBackgroundColor = Color.FromArgb("#F3F4F6");
		navPage.BarTextColor = Color.FromArgb("#111827");
		return new Window(navPage);
	}
}