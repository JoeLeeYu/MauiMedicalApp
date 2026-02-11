using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MauiMedicalApp.ViewModels
{
    public class UserProfile
    {
        public int userId { get; set; }
        public string name { get; set; }
        public string birthday { get; set; }
        public double height { get; set; }
        public double weight { get; set; }
        public string gender { get; set; }
    }

    public partial class HealthAdviceViewModel : ObservableObject
    {
        private readonly HttpClient _client = new HttpClient
        {
            BaseAddress = new Uri("http://192.168.1.106:8000/")
        };

        public static string GlobalUserName { get; set; }

        // ====== 基本資料 ======
        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private DateTime birthDate = DateTime.Today;
        [ObservableProperty] private string height = string.Empty;
        [ObservableProperty] private string weight = string.Empty;

        // ====== Picker 選項 ======
        public ObservableCollection<string> SexOptions { get; } =
            new() { "男", "女", "不透露/其他" };

        [ObservableProperty] private string selectedSex = "男";

        public ObservableCollection<string> ActivityOptions { get; } =
            new() { "久坐（幾乎不運動）", "輕量（每週 1–2 次）", "中等（每週 3–4 次）", "高強度（每週 5 次以上）" };

        [ObservableProperty] private string selectedActivity = "久坐（幾乎不運動）";

        public ObservableCollection<string> GoalOptions { get; } =
            new() { "維持", "減脂", "增肌", "體態雕塑" };

        [ObservableProperty] private string selectedGoal = "維持";

        // ====== 健康狀況 CheckBox ======
        [ObservableProperty] private bool hasJointIssue;
        [ObservableProperty] private bool hasHeartIssue;
        [ObservableProperty] private bool isPregnant;

        // ====== 結果輸出 ======
        [ObservableProperty] private string bmiValue = "—";
        [ObservableProperty] private string bmiCategory = "—";
        [ObservableProperty] private string keyHint = "按『產生建議』取得結果";
        [ObservableProperty] private string exerciseAdvice = "—";
        [ObservableProperty] private string dietAdvice = "—";

        // ====== 建構子：自動載入個人資料 ======
        public HealthAdviceViewModel()
        {
            _ = LoadUserProfile();
        }

        // ====== 自動載入使用者資料 ======
        public async Task LoadUserProfile()
        {
            try
            {
                int userId = Preferences.Get("userId", 0);
                if (userId == 0)
                {
                    await Shell.Current.DisplayAlert("錯誤", "未登入使用者", "確定");
                    return;
                }

                string url = $"auth/user/{userId}";
                var profile = await _client.GetFromJsonAsync<UserProfile>(url);

                if (profile != null)
                {
                    Name = profile.name;
                    BirthDate = DateTime.Parse(profile.birthday);
                    Height = profile.height.ToString();
                    Weight = profile.weight.ToString();
                    SelectedSex = profile.gender;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("錯誤", $"載入使用者資料失敗：{ex.Message}", "確定");
            }
        }

        // 健康建議產生 
        [RelayCommand]
        private async Task GenerateAdvice()
        {
            if (!double.TryParse(Height, out double heightCm) ||
                !double.TryParse(Weight, out double weightKg))
            {
                await Shell.Current.DisplayAlert("錯誤", "請輸入正確的身高與體重", "確定");
                return;
            }

            int userId = Preferences.Get("userId", 0);

            var data = new
            {
                userId = userId,
                Name = Name,
                Birthday = BirthDate.ToString("yyyy-MM-dd"),
                Height = heightCm,
                Weight = weightKg,
                Gender = SelectedSex,
                ActivityLevel = SelectedActivity,
                Goal = SelectedGoal,
                SpecialCondition =
                    $"{(HasJointIssue ? "關節 " : "")}{(HasHeartIssue ? "心血管 " : "")}{(IsPregnant ? "孕婦 " : "")}"
            };

            try
            {
                string apiUrl = "http://192.168.1.106:8000/advice";
                var response = await _client.PostAsJsonAsync(apiUrl, data);
                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                BmiValue = json.GetProperty("bmi").GetDouble().ToString("0.0");
                BmiCategory = json.GetProperty("status").GetString();
                KeyHint = $"每日建議熱量：{json.GetProperty("tdee").GetDouble():0} kcal";
                ExerciseAdvice = json.GetProperty("suggestion").GetString();
                DietAdvice = "請依建議調整飲食與作息";

                GlobalUserName = Name;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("錯誤", $"API 錯誤：{ex.Message}", "確定");
            }
        }

    }
}
