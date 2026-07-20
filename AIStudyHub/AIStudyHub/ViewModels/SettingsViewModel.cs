using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIStudyHub.Data;
using AIStudyHub.Models;

namespace AIStudyHub.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _apiEndpoint = string.Empty;

        [ObservableProperty]
        private string _apiKey = string.Empty;

        [ObservableProperty]
        private string _aiModel = string.Empty;

        public SettingsViewModel()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            using var db = new AppDbContext();
            ApiEndpoint = db.AppSettings.Find("ApiEndpoint")?.Value ?? string.Empty;
            ApiKey = db.AppSettings.Find("ApiKey")?.Value ?? string.Empty;
            AiModel = db.AppSettings.Find("AiModel")?.Value ?? string.Empty;
        }

        [RelayCommand]
        private void SaveSettings()
        {
            using var db = new AppDbContext();
            
            var endpoint = db.AppSettings.Find("ApiEndpoint");
            if (endpoint != null) endpoint.Value = ApiEndpoint;
            else db.AppSettings.Add(new AppSetting { Key = "ApiEndpoint", Value = ApiEndpoint });

            var key = db.AppSettings.Find("ApiKey");
            if (key != null) key.Value = ApiKey;
            else db.AppSettings.Add(new AppSetting { Key = "ApiKey", Value = ApiKey });

            var model = db.AppSettings.Find("AiModel");
            if (model != null) model.Value = AiModel;
            else db.AppSettings.Add(new AppSetting { Key = "AiModel", Value = AiModel });

            db.SaveChanges();
            MessageBox.Show("Đã lưu cấu hình thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
