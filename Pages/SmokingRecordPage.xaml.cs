using MauiMedicalApp.ViewModels;

namespace MauiMedicalApp.Pages;

public partial class SmokingRecordPage : ContentPage
{
    public SmokingRecordPage()
    {
        InitializeComponent();
        BindingContext = new SmokingRecordViewModel();
    }
}
