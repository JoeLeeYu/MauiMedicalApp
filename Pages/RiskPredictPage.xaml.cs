using MauiMedicalApp.ViewModels;

namespace MauiMedicalApp.Pages;

public partial class RiskPredictPage : ContentPage
{
    private readonly RiskPredictViewModel vm;

    public RiskPredictPage()
    {
        InitializeComponent();
        vm = new RiskPredictViewModel();
        BindingContext = vm;
    }

    private async void OnPredictClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(AgeEntry.Text, out int ageValue))
        {
            await DisplayAlert("錯誤", "請輸入正確的年齡", "確定");
            return;
        }

        var data = new
        {
            AGE = ageValue,
            Male = MaleRadio.IsChecked ? 1 : 0,
            Female = FemaleRadio.IsChecked ? 1 : 0,
            SMOKING = SmokingYes.IsChecked ? 2 : 1,
            ALCOHOL_CONSUMING = AlcoholYes.IsChecked ? 2 : 1,
            CHEST_PAIN = ChestYes.IsChecked ? 2 : 1,
            SHORTNESS_OF_BREATH = BreathYes.IsChecked ? 2 : 1,
            COUGHING = CoughYes.IsChecked ? 2 : 1,
            PEER_PRESSURE = PeerYes.IsChecked ? 2 : 1,
            CHRONIC_DISEASE = ChronicYes.IsChecked ? 2 : 1,
            SWALLOWING_DIFFICULTY = SwallowYes.IsChecked ? 2 : 1,
            YELLOW_FINGERS = YellowYes.IsChecked ? 2 : 1,
            ANXIETY = AnxietyYes.IsChecked ? 2 : 1,
            FATIGUE = FatigueYes.IsChecked ? 2 : 1,
            ALLERGY = AllergyYes.IsChecked ? 2 : 1,
            WHEEZING = WheezingYes.IsChecked ? 2 : 1
        };

        await vm.CallApiAsync(data);
    }
}
