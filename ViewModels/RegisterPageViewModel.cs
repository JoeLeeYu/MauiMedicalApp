using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Storage;
using System.Reflection;
using System.Xml.Linq;

namespace MauiMedicalApp.ViewModels
{
    public partial class RegisterPageViewModel : ObservableObject
    {
        private readonly HttpClient _client = new HttpClient { BaseAddress = new Uri("http://172.20.10.2:8000") };


        [ObservableProperty] private string account = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private string gender = string.Empty;
        [ObservableProperty] private DateTime birthday = DateTime.Today;   
        [ObservableProperty] private string height = string.Empty;
        [ObservableProperty] private string weight = string.Empty;

    
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isStatusVisible;

       

        //註冊
        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Account) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Gender) ||
                string.IsNullOrWhiteSpace(Height) ||
                string.IsNullOrWhiteSpace(Weight))
            {
                ShowStatus("請輸入所有欄位");
                return;
            }

            if (!double.TryParse(Height, out double h) ||
                !double.TryParse(Weight, out double w))
            {
                ShowStatus("身高/體重格式錯誤");
                return;
            }

            var payload = new
            {
                account = Account,
                password = Password,
                name = Name,
                gender = Gender,
                birthday = Birthday.ToString("yyyy-MM-dd"),
                height = h,
                weight = w
            };

            try
            {
                var response = await _client.PostAsJsonAsync("auth/register", payload);
                var jsonString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ShowStatus($"註冊失敗：{jsonString}");
                    return;
                }

                var json = JsonDocument.Parse(jsonString).RootElement;

                int userId = json.GetProperty("userId").GetInt32();

                // 儲存使用者資料
                Preferences.Set("UserId", userId);
                Preferences.Set("Name", Name);
                Preferences.Set("Gender", Gender);
                Preferences.Set("Birthday", Birthday.ToString("yyyy-MM-dd"));
                Preferences.Set("Height", Height);
                Preferences.Set("Weight", Weight);

                await Shell.Current.DisplayAlert("成功", "註冊成功", "確定");

                await Shell.Current.GoToAsync("//HomePage");
            }
            catch (Exception ex)
            {
                ShowStatus($"錯誤：{ex.Message}");
            }
        }

        private void ShowStatus(string msg)
        {
            StatusMessage = msg;
            IsStatusVisible = true;
        }
    }
}
