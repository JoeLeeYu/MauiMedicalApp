using MauiMedicalApp.ViewModels;

namespace MauiMedicalApp.Pages;

public partial class SmokingChartPage : ContentPage
{
    public SmokingChartPage()
    {
        InitializeComponent();

        // ✅ 使用正確的 ViewModel 名稱
        BindingContext = new SmokingChartViewModel();

        Loaded += async (s, e) =>
        {
            if (BindingContext is SmokingChartViewModel vm)
                await vm.LoadChartAsync();
        };
    }
}
