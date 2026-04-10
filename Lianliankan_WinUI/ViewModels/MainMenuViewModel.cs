using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lianliankan_WinUI.Models;
using Lianliankan_WinUI.Pages;
using Lianliankan_WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Globalization;

namespace Lianliankan_WinUI.ViewModels
{
	public partial class MainMenuViewModel : ObservableObject
	{
		private readonly INavigationService _navigationService;
		[ObservableProperty]
		public partial AppData AppData { get; set; }

		public MainMenuViewModel(INavigationService navService, AppData appData)
		{
			_navigationService = navService;
			AppData = appData;
		}

		public void Initialize()
		{
			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			// Read data from a simple setting.
			Object rankInfoes = localSettings.Values["RankInfoes"];

			if (rankInfoes != null)
			{
				AppData.RankInfoes = JsonSerializer.Deserialize<List<RankInfo>>(rankInfoes as string);
			}
			else
			{
				localSettings.Values["RankInfoes"] = JsonSerializer.Serialize(AppData.RankInfoes);
			}

			var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
			AppData.RankInfoes.ForEach(r =>
			{
				var levelDesc = resourceLoader.GetString($"{Enum.GetName(typeof(GameLevels), r.GameLevel)}/Content");
				r.LevelDescription = $"{levelDesc}({r.UserSetRow}X{r.UserSetColumn})";
			});
		}
		public void CleanUp()
		{
			
		}

		[RelayCommand]
		private static async Task ChangeToCNLanguage()
		{
			if (ApplicationLanguages.PrimaryLanguageOverride != "zh-Hans-CN")
			{
				ApplicationLanguages.PrimaryLanguageOverride = "zh-CN";

				var uri = new Uri("lianliankanwinui:");
				await Windows.System.Launcher.LaunchUriAsync(uri, new Windows.System.LauncherOptions());
				Application.Current.Exit();
			}
		}

		[RelayCommand]
		private static async Task ChangeToENLanguage()
		{
			if (ApplicationLanguages.PrimaryLanguageOverride != "en-US")
			{
				ApplicationLanguages.PrimaryLanguageOverride = "en-US";

				var uri = new Uri("lianliankanwinui:");
				await Windows.System.Launcher.LaunchUriAsync(uri, new Windows.System.LauncherOptions());
				Application.Current.Exit();
			}
		}

		[RelayCommand]
		private void StartGame(GameLevels level)
		{
			// 简单难度默认值
			int userSetColumns = 8;
			int userSetRows = 6;

			GameLevels gameLevels = GameLevels.Easy;

			// 设定中等难度
			if (level == GameLevels.Medium)
			{
				userSetRows = 7;
				userSetColumns = 10;
				gameLevels = GameLevels.Medium;
			}
			// 设定困难难度
			if (level == GameLevels.Hard)
			{
				userSetRows = 9;
				userSetColumns = 12;
				gameLevels = GameLevels.Hard;
			}
			// 自定义难度跳转到自定义窗口
			if (level == GameLevels.Custom)
			{
				_navigationService.NavigateTo(nameof(DifficultySettingPage),null, new CommonNavigationTransitionInfo());
				return;
			}

			_navigationService.NavigateTo(nameof(GameWindowPage), new GameOptions
			{
				UserSetRows = userSetRows,
				UserSetColumns = userSetColumns,
				GameLevels = gameLevels
			}, new CommonNavigationTransitionInfo());
		}
	}
}
