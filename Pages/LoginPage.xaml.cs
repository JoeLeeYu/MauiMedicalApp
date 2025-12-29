namespace MauiMedicalApp.Pages;
using MauiMedicalApp.ViewModels;
public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
        BindingContext = new LoginViewModel();
    }
}