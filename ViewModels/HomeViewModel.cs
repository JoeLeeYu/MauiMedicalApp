using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiMedicalApp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [RelayCommand]
    async Task GoHealthAdvice()
    {
        await Shell.Current.GoToAsync("HealthAdvicePage");
    }

    [RelayCommand]
    async Task GoRiskPredict()
    {
        await Shell.Current.GoToAsync("RiskPredictPage");
    }
    [RelayCommand]
    async Task GoSmokingRecord()
    {
        await Shell.Current.GoToAsync("SmokingRecordPage");
    }
    [RelayCommand]
    async Task GoSmokingChart()
    {
        await Shell.Current.GoToAsync("SmokingChartPage");
    }
    [RelayCommand]
    async Task GoImagePredict()
    {
        await Shell.Current.GoToAsync("ImagePredictPage");
    }
    [RelayCommand]
    async Task GoDietRecord()
    {
        await Shell.Current.GoToAsync("DietRecordPage");
    }
    [RelayCommand]
    async Task GoSportRecord()
    {
        await Shell.Current.GoToAsync("SportRecordPage");
    }
}
