using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Json;

namespace MauiMedicalApp.ViewModels;

public partial class SmokingRecordViewModel : ObservableObject
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://192.168.1.106:8000/")
    };

    private const int UserId = 1; // demo 用

    [ObservableProperty] double lungHealthProgress = 0.8;
    [ObservableProperty] string lungStatusText = "呼吸順暢";
    [ObservableProperty] Color lungStatusColor = Colors.Green;
    [ObservableProperty] Color lungBubbleColor = Color.FromArgb("#32ADE6");
    [ObservableProperty] string savedMoneyText = "$ 0 TWD";
    [ObservableProperty] string cleanTimeText = "尚未開始紀錄";
    [ObservableProperty] string motivationQuote = "每一次拒絕菸癮，都是對未來健康的最好投資。";

    private DateTime _lastSmokeTime = DateTime.Now;
    private int _resistCount = 0;

    public SmokingRecordViewModel()
    {
        UpdateStatus();
    }

    // ================== 抽了一支 ==================

    [RelayCommand]
    private async Task Smoke()
    {
        await SendRecordToApi("Smoke", 1);

        _lastSmokeTime = DateTime.Now;
        _resistCount = 0;
        LungHealthProgress = Math.Clamp(LungHealthProgress - 0.25, 0.05, 1.0);

        UpdateLungVisuals();
        UpdateStatus();
        MotivationQuote = "沒關係，下一分鐘是全新的開始。";

        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
    }

    // ================== 忍住了 ==================

    [RelayCommand]
    private async Task Resist()
    {
        await SendRecordToApi("Resist", 1);

        _resistCount++;
        LungHealthProgress = Math.Clamp(LungHealthProgress + 0.06, 0.0, 1.0);

        UpdateLungVisuals();
        UpdateStatus();
        MotivationQuote = "做的太棒了！尼古丁正在離開你的大腦！";

        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
    }

    // ================== API 傳輸（重點修正） ==================

    private async Task SendRecordToApi(string actionType, int amount)
    {
        try
        {
            var payload = new
            {
                userId = UserId,                      // ✅ int
                type = actionType,                    // "Smoke" / "Resist"
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                amount = amount                       // ✅ 必送
            };

            var response = await _httpClient
                .PostAsJsonAsync("smoking", payload);

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"API Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Exception: {ex.Message}");
        }
    }

    // ================== UI 狀態 ==================

    private void UpdateLungVisuals()
    {
        if (LungHealthProgress < 0.4)
        {
            LungStatusText = "肺部感到沉重";
            LungStatusColor = Color.FromArgb("#FF3B30");
            LungBubbleColor = Color.FromArgb("#4A4A4A");
        }
        else if (LungHealthProgress < 0.75)
        {
            LungStatusText = "正在代謝毒素";
            LungStatusColor = Color.FromArgb("#FF9500");
            LungBubbleColor = Color.FromArgb("#FFCC00");
        }
        else
        {
            LungStatusText = "呼吸極度順暢";
            LungStatusColor = Color.FromArgb("#34C759");
            LungBubbleColor = Color.FromArgb("#32ADE6");
        }
    }

    private void UpdateStatus()
    {
        var timePassed = DateTime.Now - _lastSmokeTime;
        CleanTimeText = $"已堅持 {timePassed.Hours} 小時 {timePassed.Minutes} 分鐘";
        SavedMoneyText = $"$ {(_resistCount * 12 + timePassed.TotalHours * 5):F1} TWD";
    }

    [RelayCommand]
    async Task GoSmokingChart()
    {
        await Shell.Current.GoToAsync("SmokingChartPage");
    }
}
