namespace MauiMedicalApp.Pages;
using MauiMedicalApp.ViewModels;


public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
        BindingContext = new HomeViewModel();
    }
}