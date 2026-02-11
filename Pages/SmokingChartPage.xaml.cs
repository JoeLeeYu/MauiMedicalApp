using MauiMedicalApp.ViewModels;

namespace MauiMedicalApp.Pages;

public partial class SmokingChartPage : ContentPage
{
    public SmokingChartPage()
    {
        InitializeComponent();

    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SmokingChartViewModel vm)
        {
            await vm.LoadDataCommand.ExecuteAsync(null);
        }
    }
}
