namespace MauiMedicalApp.Pages;
using MauiMedicalApp.ViewModels;
public partial class ImagePredictPage : ContentPage
{
	public ImagePredictPage()
	{
		InitializeComponent();
        BindingContext = new ImagePredictPageViewModel();
    }
}