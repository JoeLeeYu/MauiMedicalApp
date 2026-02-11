using MauiMedicalApp.ViewModels;

namespace MauiMedicalApp.Pages;

public partial class SmokingRecordPage : ContentPage
{
    private bool _isAnimating = false;

    public SmokingRecordPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isAnimating = true;
        StartBreathingAnimation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isAnimating = false; // 離開頁面停止動畫，省電
    }

    private async void StartBreathingAnimation()
    {
        while (_isAnimating)
        {
            await BreathBubble.ScaleTo(1.15, 2000, Easing.SinInOut);
            await BreathBubble.ScaleTo(1.0, 2000, Easing.SinInOut);
        }
    }
}
