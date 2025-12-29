using MauiMedicalApp.ViewModels;

namespace MauiMedicalApp.Pages;

public partial class SportRecordPage : ContentPage
{
    public SportRecordPage()
    {
        InitializeComponent();
        BindingContext = new SportRecordViewModel();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is SportRecordViewModel vm)
        {
            await vm.InitAsync();
        }
    }

}
