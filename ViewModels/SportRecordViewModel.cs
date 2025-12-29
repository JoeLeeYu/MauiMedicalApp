using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json;

namespace MauiMedicalApp.ViewModels;

public partial class SportRecordViewModel : ObservableObject
{
    private readonly HttpClient _httpClient = new();

    //使用者 
    public string UserName => HealthAdviceViewModel.GlobalUserName;

    [ObservableProperty]
    private int userAge = 30; // 之後可從使用者 API 帶入

    // 運動類型 
    public ObservableCollection<string> SportTypes { get; } = new();

    [ObservableProperty] private string selectedSport;
    [ObservableProperty] private string customSportName;
    [ObservableProperty] private bool isCustomSport;

    //  輸入
    [ObservableProperty] private string inputMinutes;
    [ObservableProperty] private string inputDistance;

    //紀錄 
    public ObservableCollection<SportRecord> Records { get; } = new();

    //  UI 
    [ObservableProperty] private double weeklyProgress;
    [ObservableProperty] private string weeklyProgressText;

    public SportRecordViewModel()
    {
        BuildSportTypesByAge();
        _ = LoadSportRecords();   //  初始化查詢
    }
    public class SportApiRecord
    {
        public int id { get; set; }
        public string sportType { get; set; }
        public int durationMinutes { get; set; }
        public double? distanceKm { get; set; }
        public string createdAt { get; set; }
    }


    public class SportRecord
    {
        public string Type { get; set; }
        public int Minutes { get; set; }
        public double Distance { get; set; }
        public DateTime Date { get; set; }

        public string DisplayInfo =>
            Distance > 0
                ? $"{Minutes} 分鐘 · {Distance} km"
                : $"{Minutes} 分鐘";
    }
    public class SportRecordResponse
    {
        public List<SportRecordDto> records { get; set; }
    }

    public class SportRecordDto
    {
        public string sportType { get; set; }
        public int durationMinutes { get; set; }
        public double? distanceKm { get; set; }
        public string createdAt { get; set; }
    }

    public async Task InitAsync()
    {
        int userId = Preferences.Get("userId", 0);
        if (userId == 0) return;

        var resp = await _httpClient.GetAsync(
            $"http://172.20.10.2:8000/sport/user/{userId}");

        if (!resp.IsSuccessStatusCode)
            return;

        var json = await resp.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<SportRecordDto>>(json);

        Records.Clear();

        foreach (var r in list)
        {
            Records.Add(new SportRecord
            {
                Type = r.sportType,
                Minutes = r.durationMinutes,
                Distance = r.distanceKm ?? 0,
                Date = DateTime.Parse(r.createdAt)
            });
        }

        UpdateWeeklyProgress();
    }


    // 初始化：查詢歷史運動紀錄
    private async Task LoadSportRecords()
    {
        if (string.IsNullOrWhiteSpace(UserName))
            return;

        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<SportApiRecord>>(
                $"http://172.20.10.2:8000/sport/user/{UserName}");

            Records.Clear();

            foreach (var r in result ?? new())
            {
                Records.Add(new SportRecord
                {
                    Type = r.sportType,
                    Minutes = r.durationMinutes,
                    Distance = r.distanceKm ?? 0,
                    Date = DateTime.ParseExact(
                        r.createdAt,
                        "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture)
                });
            }

            UpdateWeeklyProgress();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "錯誤",
                $"無法載入運動紀錄：{ex.Message}",
                "OK");
        }
    }
    // 新增運動
    [RelayCommand]
    private async Task AddSport()
    {
        if (!int.TryParse(InputMinutes, out int mins) || mins <= 0)
        {
            await Shell.Current.DisplayAlert("錯誤", "請輸入正確的運動時間", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedSport))
        {
            await Shell.Current.DisplayAlert("錯誤", "請選擇運動類型", "OK");
            return;
        }

        double.TryParse(InputDistance, out double dist);

        int userId = Preferences.Get("userId", 0);

        if (userId <= 0)
        {
            await Shell.Current.DisplayAlert("錯誤", "尚未登入", "OK");
            return;
        }

        var apiData = new
        {
            userId = userId,
            sportType = SelectedSport,
            durationMinutes = mins,
            distanceKm = dist == 0 ? (double?)null : dist

        };

        var response = await _httpClient.PostAsJsonAsync(
            "http://172.20.10.2:8000/sport/add",
            apiData
        );

        string respText = await response.Content.ReadAsStringAsync();

        await Shell.Current.DisplayAlert(
            "API 回應",
            $"Status: {(int)response.StatusCode}\n{respText}",
            "OK"
        );

        if (!response.IsSuccessStatusCode)
            return;

        Records.Insert(0, new SportRecord
        {
            Type = SelectedSport,
            Minutes = mins,
            Distance = dist,
            Date = DateTime.Now
        });

        UpdateWeeklyProgress();

        InputMinutes = "";
        InputDistance = "";
    }



    // 週進度邏輯

    private void UpdateWeeklyProgress()
    {
        int delta = ((int)DateTime.Now.DayOfWeek + 6) % 7;
        var weekStart = DateTime.Now.Date.AddDays(-delta);
        var today = DateTime.Now.Date;

        var thisWeek = Records.Where(r => r.Date >= weekStart).ToList();

        double totalMinutes = thisWeek.Sum(r => r.Minutes);
        double todayMinutes = Records
            .Where(r => r.Date.Date == today)
            .Sum(r => r.Minutes);

        bool isTeen = UserAge < 18;
        bool isSenior = UserAge >= 65;

        int specialCount = isSenior
            ? thisWeek.Count(r =>
                r.Type.Contains("太極") ||
                r.Type.Contains("伸展") ||
                r.Type.Contains("平衡") ||
                r.Type.Contains("關節"))
            : thisWeek.Count(r => r.Type.Contains("肌力"));

        double timeGoal = isTeen ? 60.0 : 150.0;
        int specialGoal = isTeen ? 3 : 2;

        if (isTeen)
        {
            WeeklyProgress = Math.Min(todayMinutes / timeGoal, 1.0);
            WeeklyProgressText =
                $"今日運動 {todayMinutes:0} 分鐘 / {timeGoal} 分鐘\n" +
                $"肌力訓練 {specialCount}/{specialGoal} 次";
        }
        else if (isSenior)
        {
            WeeklyProgress = Math.Min(totalMinutes / timeGoal, 1.0);
            WeeklyProgressText =
                $"本週運動 {totalMinutes:0} 分鐘 / {timeGoal} 分鐘\n" +
                $"平衡/柔軟度訓練 {specialCount}/{specialGoal} 次";
        }
        else
        {
            WeeklyProgress = Math.Min(totalMinutes / timeGoal, 1.0);
            WeeklyProgressText =
                $"本週運動 {totalMinutes:0} 分鐘 / {timeGoal} 分鐘\n" +
                $"肌力訓練 {specialCount}/{specialGoal} 次";
        }
    }

    // 年齡導向運動清單

    private void BuildSportTypesByAge()
    {
        SportTypes.Clear();

        IEnumerable<string> list;

        if (UserAge <= 17)
            list = new[] { "籃球", "跳繩", "足球", "游泳", "慢跑", "肌力訓練" };
        else if (UserAge <= 64)
            list = new[] { "快走", "慢跑", "游泳", "登山", "腳踏車", "太極拳", "肌力訓練" };
        else
            list = new[] { "健走", "太極拳", "伸展操", "平衡訓練" };

        foreach (var item in list)
            SportTypes.Add(item);

        SportTypes.Add("自行輸入");
        SelectedSport = SportTypes.First();
    }
}


