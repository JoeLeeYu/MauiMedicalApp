using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace MauiMedicalApp.ViewModels
{
    public partial class DietRecordViewModel : ObservableObject
    {
        private readonly HttpClient _client = new();

        // ===== 基本輸入 =====
        [ObservableProperty] private string item;
        [ObservableProperty] private string calories;

        // ===== 日期 =====
        [ObservableProperty] private DateTime selectedDate = DateTime.Today;

        // ===== 餐別（RadioButton）=====
        [ObservableProperty] private bool isBreakfast;
        [ObservableProperty] private bool isLunch;
        [ObservableProperty] private bool isDinner;
        [ObservableProperty] private bool isSnack;

        // ===== 類別（CheckBox）=====
        [ObservableProperty] private bool hasGrain;
        [ObservableProperty] private bool hasVegetable;
        [ObservableProperty] private bool hasProtein;
        [ObservableProperty] private bool hasDairy;
        [ObservableProperty] private bool hasFruit;
        [ObservableProperty] private bool hasFatNut;

        // ===== 清單 =====
        public ObservableCollection<DietRecord> Records { get; } = new();

        // ===== 當日總熱量 =====
        [ObservableProperty] private int dailyTotalCalories;

        public DietRecordViewModel()
        {
            LoadRecordsByDate();
        }

        // ======================
        // 儲存飲食紀錄
        // ======================
        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Item))
            {
                await Shell.Current.DisplayAlert("提醒", "請輸入飲食內容", "OK");
                return;
            }

            int userId = Preferences.Get("userId", 0);
            int cal = string.IsNullOrWhiteSpace(Calories) ? 0 : int.Parse(Calories);

            var data = new
            {
                userId = userId,
                item = Item,
                calories = cal,
                mealType = GetMealType(),
                categories = GetCategories(),
                recordTime = DateTime.Now
            };

            var response = await _client.PostAsJsonAsync(
                "http://172.20.10.2:8000/diet/add",
                data);

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("成功", "已新增飲食紀錄", "OK");

                // 清空
                Item = string.Empty;
                Calories = string.Empty;
                ClearSelections();

                LoadRecordsByDate();
            }
        }

        // ======================
        // 依日期載入資料
        // ======================
        [RelayCommand]
        private async void LoadRecordsByDate()
        {
            int userId = Preferences.Get("userId", 0);

            var result = await _client.GetFromJsonAsync<List<DietRecord>>(
                $"http://172.20.10.2:8000/diet/user/{userId}");

            Records.Clear();

            var filtered = result
                .Where(r => DateTime.Parse(r.RecordTime).Date == SelectedDate.Date)
                .OrderByDescending(r => DateTime.Parse(r.RecordTime))
                .ToList();

            foreach (var r in filtered)
                Records.Add(r);

            DailyTotalCalories = filtered.Sum(r => r.Calories ?? 0);
        }

        private string GetMealType()
        {
            if (IsBreakfast) return "早餐";
            if (IsLunch) return "午餐";
            if (IsDinner) return "晚餐";
            if (IsSnack) return "點心";
            return "";
        }

        private string GetCategories()
        {
            var list = new List<string>();
            if (HasGrain) list.Add("全榖雜糧類");
            if (HasVegetable) list.Add("蔬菜類");
            if (HasProtein) list.Add("豆魚蛋肉類");
            if (HasDairy) list.Add("乳品類");
            if (HasFruit) list.Add("水果類");
            if (HasFatNut) list.Add("油脂與堅果種子類");
            return string.Join("、", list);
        }

        private void ClearSelections()
        {
            IsBreakfast = IsLunch = IsDinner = IsSnack = false;
            HasGrain = HasVegetable = HasProtein = HasDairy = HasFruit = HasFatNut = false;
        }
    }

    public class DietRecord
    {
        public int RecordId { get; set; }
        public string Item { get; set; }
        public int? Calories { get; set; }
        public string MealType { get; set; }
        public string Categories { get; set; }
        public string RecordTime { get; set; }
    }
}
