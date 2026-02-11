using System.Collections.ObjectModel;
using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiMedicalApp.ViewModels
{
    public partial class SmokingChartViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;

        private const int UserId = 1; // demo 用，之後可換登入使用者

        // ====== UI Binding ======

        [ObservableProperty] private DateTime selectedDate = DateTime.Now;
        [ObservableProperty] private DateTime todayDate = DateTime.Now;

        [ObservableProperty] private double successRatioWidth;
        [ObservableProperty] private double failureRatioWidth;

        [ObservableProperty] private string successCountText = "成功: 0 次";
        [ObservableProperty] private string failureCountText = "吸菸: 0 支";

        [ObservableProperty] private int smokeAmount = 1;

        public ObservableCollection<ChartRecordItem> HistoryRecords { get; } = new();

        public SmokingChartViewModel()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://192.168.1.106:8000/")
            };

            _ = LoadDataAsync();
        }

        // ====== Commands ======

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            await RefreshFromApiAsync();
        }

        [RelayCommand]
        private async Task AddPastRecordAsync()
        {
            try
            {
                var record = new
                {
                    userId = UserId,
                    type = "Smoke",
                    timestamp = SelectedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    amount = SmokeAmount
                };

                var response = await _httpClient.PostAsJsonAsync("smoking", record);

                if (response.IsSuccessStatusCode)
                {
                    SmokeAmount = 1; // UX：補記後重設
                    await RefreshFromApiAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "錯誤", "補記失敗", "OK");
                }
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert(
                    "錯誤", "無法連線至後端伺服器", "OK");
            }
        }

        // ====== 核心：完全依 API ======

        private async Task RefreshFromApiAsync()
        {
            try
            {
                var stats = await _httpClient
                    .GetFromJsonAsync<SmokingStatsResponse>(
                        $"smoking/stats/{UserId}");

                if (stats == null) return;

                // --- 上方比例 ---
                double total = stats.SuccessCount + stats.FailureCount;
                if (total <= 0) total = 1;

                SuccessRatioWidth = (stats.SuccessCount / total) * 280;
                FailureRatioWidth = (stats.FailureCount / total) * 280;

                SuccessCountText = $"成功: {stats.SuccessCount} 次";
                FailureCountText = $"吸菸: {stats.FailureCount} 支";

                // --- 歷史清單 ---
                HistoryRecords.Clear();

                foreach (var h in stats.History)
                {
                    if (h.Type == "Smoke")
                    {
                        HistoryRecords.Add(new ChartRecordItem
                        {
                            Title = "吸菸紀錄",
                            TimeString = h.Time,
                            Icon = "🚬",
                            IconBgColor = Color.FromArgb("#F2F2F7"),
                            ValueChangeText = $"-{h.Amount} 支",
                            ValueColor = Color.FromArgb("#FF3B30")
                        });
                    }
                    else if (h.Type == "Resist")
                    {
                        HistoryRecords.Add(new ChartRecordItem
                        {
                            Title = "成功忍住",
                            TimeString = h.Time,
                            Icon = "🧘",
                            IconBgColor = Color.FromArgb("#E8F5E9"),
                            ValueChangeText = $"+{h.Amount} 次",
                            ValueColor = Color.FromArgb("#34C759")
                        });
                    }
                }
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert(
                    "錯誤", "讀取吸菸紀錄失敗", "OK");
            }
        }
    }

    // ===== DTO 對齊 FastAPI =====

    public class SmokingStatsResponse
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<SmokingHistoryDto> History { get; set; }
    }

    public class SmokingHistoryDto
    {
        public string Type { get; set; }
        public int Amount { get; set; }
        public string Time { get; set; }
    }

    // ===== UI Model =====

    public class ChartRecordItem
    {
        public string Title { get; set; }
        public string TimeString { get; set; }
        public string Icon { get; set; }
        public Color IconBgColor { get; set; }
        public string ValueChangeText { get; set; }
        public Color ValueColor { get; set; }
    }
}
