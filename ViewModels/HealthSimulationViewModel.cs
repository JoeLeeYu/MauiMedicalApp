using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Devices; // 用於觸覺回饋

namespace MauiMedicalApp.ViewModels
{
    public partial class HealthSimulationViewModel : ObservableObject
    {
        // 基準分數（假設這是使用者目前的真實狀況，用於對比改善程度）
        private const double BaseLineScore = 55.0;

        // ===== 使用者輸入 =====
        [ObservableProperty]
        double smokingPerWeek = 3;

        [ObservableProperty]
        double exerciseMinutes = 60;

        [ObservableProperty]
        double dailyCalories = 2200;

        // ===== 輸出 =====
        [ObservableProperty]
        int healthScore;

        [ObservableProperty]
        string riskText = "";

        [ObservableProperty]
        Color riskColor = Colors.Gray;

        [ObservableProperty]
        string aiExplanation = "";

        // 新增：改善百分比文字 (例如：風險降低 15%)
        [ObservableProperty]
        string improvementText = "";

        public HealthSimulationViewModel()
        {
            Recalculate();
        }

        // ===== Slider 改變時自動觸發 =====
        // 加入 HapticFeedback 讓發表演示時手感更好
        partial void OnSmokingPerWeekChanged(double value) => UpdateWithFeedback();
        partial void OnExerciseMinutesChanged(double value) => UpdateWithFeedback();
        partial void OnDailyCaloriesChanged(double value) => UpdateWithFeedback();

        private void UpdateWithFeedback()
        {
            Recalculate();

            // 只有在實機運行時執行輕微震動
            try
            {
                #if MOBILE
                                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
                #endif
            }
            catch { /* 忽略不支援設備的錯誤 */ }
        }

        // ===== 核心計算 =====
        void Recalculate()
        {
            // 基礎分數計算邏輯
            double score =
                smokingPerWeek * 4 +
                (300 - exerciseMinutes) * 0.1 +
                (dailyCalories - 2000) * 0.02;

            score = Math.Clamp(score, 5, 95); // 留一點邊界感比較美觀
            HealthScore = (int)score;

            // 計算與基準點的差異
            double diff = BaseLineScore - score;
            if (diff >= 0)
                ImprovementText = $"風險較目前降低了 {Math.Abs(diff):F0}%";
            else
                ImprovementText = $"風險較目前上升了 {Math.Abs(diff):F0}%";

            // 根據分數判定等級與 Apple Style 顏色
            if (HealthScore < 35)
            {
                RiskText = "表現優異";
                RiskColor = Color.FromArgb("#34C759"); // iOS Green
                AiExplanation = "太棒了！目前的模擬顯示，增加運動量並控制菸草攝取能顯著改善您的心血管健康預測。";
            }
            else if (HealthScore < 65)
            {
                RiskText = "中度風險";
                RiskColor = Color.FromArgb("#FF9500"); // iOS Orange
                AiExplanation = "風險處於中等水平。若能進一步減少每週抽菸次數，模擬得分將會大幅進入健康區間。";
            }
            else
            {
                RiskText = "需要關注";
                RiskColor = Color.FromArgb("#FF3B30"); // iOS Red
                AiExplanation = "警告：目前的生活型態模擬顯示長期健康風險偏高。建議優先從增加每日步數與飲食控制開始改善。";
            }
        }
    }
}