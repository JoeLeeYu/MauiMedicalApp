using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Media;
using System.Net.Http.Headers;
using System.Text.Json;
namespace MauiMedicalApp.ViewModels;

public partial class ImagePredictPageViewModel : ObservableObject
{
    private string? _selectedPhotoPath = null;

    // 狀態文字（例如：掃描成功！）
    [ObservableProperty]
    string statusText = "請拍照或選擇影像";

    // 分析結果（例如：AI判斷結果...）
    [ObservableProperty]
    string resultText;

    // 是否顯示結果 Label
    [ObservableProperty]
    bool isResultVisible = false;

    // 要顯示在 UI 的圖片
    [ObservableProperty]
    ImageSource imageSource;

    // 是否顯示圖片
    [ObservableProperty]
    bool isImageVisible = false;

    // 是否可以按「分析圖片」
    [ObservableProperty]
    bool canAnalyze = false;

    [RelayCommand]
    async Task TakePhoto()
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    using Stream sourceStream = await photo.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);
                    await sourceStream.CopyToAsync(localFileStream);

                    _selectedPhotoPath = localFilePath;

                    ImageSource = ImageSource.FromFile(localFilePath);
                    IsImageVisible = true;
                    StatusText = "掃描成功！";

                    CanAnalyze = true;
                }
            }
            else
            {
                StatusText = "此裝置不支援相機功能";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"無法開啟相機：{ex.Message}";
        }
    }

  
    [RelayCommand]
    async Task PickPhoto()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo != null)
            {
                _selectedPhotoPath = photo.FullPath;
                using Stream stream = await photo.OpenReadAsync();

                ImageSource = ImageSource.FromStream(() => stream);
                IsImageVisible = true;
                StatusText = "已選擇圖片：" + photo.FileName;

                CanAnalyze = true;
            }
        }
        catch (FeatureNotSupportedException)
        {
            StatusText = "此裝置不支援相簿功能";
        }
        catch (PermissionException)
        {
            StatusText = "請允許相簿權限";
        }
        catch (Exception ex)
        {
            StatusText = $"無法選擇照片：{ex.Message}";
        }
    }

 
    [RelayCommand]
    async Task Analyze()
    {
        if (string.IsNullOrEmpty(_selectedPhotoPath))
        {
            StatusText = "請先選擇或拍照上傳圖片";
            return;
        }

        try
        {
            IsResultVisible = true;
            ResultText = "AI 分析中...";

            using var client = new HttpClient();
            var form = new MultipartFormDataContent();
            var userId = Preferences.Get("userId", -1);

            
            client.DefaultRequestHeaders.Add("userId", userId.ToString());

            var imgBytes = File.ReadAllBytes(_selectedPhotoPath);
            var byteContent = new ByteArrayContent(imgBytes);
            byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

            form.Add(byteContent, "file", Path.GetFileName(_selectedPhotoPath));

            // ⚠️ 你的後端 API
            var url = "http://172.20.10.2:8000/predict";

            var response = await client.PostAsync(url, form);
            var jsonStr = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<PredictionResult>(jsonStr);

            ResultText =
                $" AI 判斷結果：{data.prediction}\n\n" +
                $" 無癌機率：{data.ProbNoCancer:P2}\n" +
                $" 癌症機率：{data.ProbCancer:P2}";
        }
        catch (Exception ex)
        {
            ResultText = $"分析失敗：{ex.Message}";
        }
    }

    // -----------------------------
    // JSON 回傳格式
    // -----------------------------
    public class PredictionResult
    {
        public string prediction { get; set; }
        public float ProbCancer { get; set; }
        public float ProbNoCancer { get; set; }
    }
}
