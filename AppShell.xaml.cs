namespace MauiMedicalApp;
using MauiMedicalApp.Pages; 

  public partial class AppShell : Shell
  {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("HealthAdvicePage", typeof(HealthAdvicePage));
            Routing.RegisterRoute("RiskPredictPage", typeof(RiskPredictPage));
            Routing.RegisterRoute("SmokingRecordPage", typeof(SmokingRecordPage));
            Routing.RegisterRoute("SmokingChartPage", typeof(SmokingChartPage));
            Routing.RegisterRoute("SportRecordPage", typeof(SportRecordPage));
            Routing.RegisterRoute("ImagePredictPage", typeof(ImagePredictPage));
            Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
            Routing.RegisterRoute("DietRecordPage", typeof(DietRecordPage));
    }
  }

