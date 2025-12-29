using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Json;

namespace MauiMedicalApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly HttpClient _httpClient = new();

    [ObservableProperty]
    string username;

    [ObservableProperty]
    string password;

    [ObservableProperty]
    string statusMessage;

    [ObservableProperty]
    bool isStatusVisible = false;

    [RelayCommand]
    async Task Login()
    {
        IsStatusVisible = false;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "請輸入帳號與密碼！";
            IsStatusVisible = true;
            return;
        }

        try
        {
            var loginData = new
            {
                account = Username,
                password = Password
            };

            string url = "http://172.20.10.2:8000/auth/login";

            var response = await _httpClient.PostAsJsonAsync(url, loginData);

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null || result.error != null)
            {
                StatusMessage = result?.error ?? "帳號或密碼錯誤！";
                IsStatusVisible = true;
                return;
            }

            
            Preferences.Set("userId", result.userId);
            Preferences.Set("userName", result.name);

            
            await Shell.Current.GoToAsync("//HomePage");
        }
        catch (Exception ex)
        {
            StatusMessage = $"登入失敗：{ex.Message}";
            IsStatusVisible = true;
        }
    }

    [RelayCommand]
    async Task GoRegister()
    {
        await Shell.Current.GoToAsync("//RegisterPage");
    }

    public class LoginResponse
    {
        public string message { get; set; }
        public int userId { get; set; }
        public string name { get; set; }
        public string error { get; set; }
    }
}
