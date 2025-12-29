using MauiMedicalApp.ViewModels;
namespace MauiMedicalApp.Pages;

public partial class HealthAdvicePage : ContentPage
{
    public HealthAdvicePage()
    {
        InitializeComponent();
        BindingContext = new HealthAdviceViewModel();
    }
}