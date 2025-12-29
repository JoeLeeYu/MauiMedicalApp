using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace MauiMedicalApp.ViewModels;

public partial class SmokingRecordViewModel : ObservableObject
{
    [ObservableProperty]
    int smokeCount = 0;

    [ObservableProperty]
    double tarPerCig = 10;

    [ObservableProperty]
    double pricePerCig = 13; // 每支 13 元

    public string SmokeCountText => $"{SmokeCount} 支";
    public string MoneyText => $"${SmokeCount * PricePerCig}";
    public string TarText => $"{SmokeCount * TarPerCig} mg";

    public ObservableCollection<int> TarOptions { get; set; } = new()
    {
        5, 8, 10, 12, 15, 20
    };

    [RelayCommand]
    void AddSmoke()
    {
        SmokeCount++;
        OnPropertyChanged(nameof(SmokeCountText));
        OnPropertyChanged(nameof(MoneyText));
        OnPropertyChanged(nameof(TarText));
    }

    [RelayCommand]
    async Task ViewChart()
    {
        // ✅ 導向吸菸圖表頁
        await Shell.Current.GoToAsync("SmokingChartPage");
    }
}
