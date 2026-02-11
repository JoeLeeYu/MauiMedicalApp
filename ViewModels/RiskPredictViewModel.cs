// RiskPredictViewModel.cs (最終 Event 模式版本)
using CommunityToolkit.Mvvm.ComponentModel;
// 移除 CommunityToolkit.Mvvm.Input 的 using
using System.Text.Json;
using System.Text;

namespace MauiMedicalApp.ViewModels;

public partial class RiskPredictViewModel : ObservableObject
{
    private readonly HttpClient _client = new HttpClient();

    [ObservableProperty]
    private string resultText = "尚未檢測";

    // 保持原本的名稱 CallApiAsync，不再使用 [RelayCommand]
    public async Task CallApiAsync(object data)
    {
        try
        {
            string url = "http://192.168.1.106:8000/risk";
            string json = JsonSerializer.Serialize(data);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;
            string predictionStr = root.GetProperty("prediction").GetString();
            double probNoCancer = root.GetProperty("ProbNoCancer").GetDouble();
            double probCancer = root.GetProperty("ProbCancer").GetDouble();

            int prediction = int.Parse(predictionStr);

            string lowRisk = (probNoCancer * 100).ToString("F2") + "%";
            string highRisk = (probCancer * 100).ToString("F2") + "%";

            ResultText = (prediction == 1
                          ? " 高風險（可能罹患肺癌）"
                          : " 低風險（可能未罹患肺癌）") +
                         $"\n各類別預測機率:\n低風險: {lowRisk}\n高風險: {highRisk}";
        }
        catch (Exception ex)
        {
            ResultText = $" API 錯誤\n{ex.Message}";
        }
    }
}