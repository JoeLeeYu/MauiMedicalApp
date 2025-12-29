using CommunityToolkit.Mvvm.ComponentModel;
using Microcharts;
using SkiaSharp;

namespace MauiMedicalApp.ViewModels
{
    public partial class SmokingChartViewModel : ObservableObject
    {
        [ObservableProperty]
        private Chart smokingChart;

        [ObservableProperty]
        private string indicatorText = "等待資料...";

        [ObservableProperty]
        private SKColor indicatorColor = SKColors.Gray;

        public async Task LoadChartAsync()
        {
            // 這裡未來接 API
            // var records = await http.GetFromJsonAsync<List<SmokingRecord>>(url);

            await Task.CompletedTask;

            // ❗不做任何資料處理，等後端
            smokingChart = null;
            indicatorText = "等待後端提供資料";
            indicatorColor = SKColors.Gray;
        }
    }
}
