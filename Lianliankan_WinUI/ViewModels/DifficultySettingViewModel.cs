using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lianliankan_WinUI.Messages;
using Lianliankan_WinUI.Models;
using Lianliankan_WinUI.Pages;
using Lianliankan_WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.System;

namespace Lianliankan_WinUI.ViewModels
{
	public partial class DifficultySettingViewModel : ObservableObject
	{
		//private Grid ShowPad { get; set; }
		[ObservableProperty]
		public partial UIElementCollection ShowPadChildren { get; set; }
		[ObservableProperty]
		public partial RowDefinitionCollection RowDefinitions { get; set; }
		[ObservableProperty]
		public partial ColumnDefinitionCollection ColumnDefinitions { get; set; }
		[ObservableProperty]
		public partial int VerticalSliderValue { get; set; } = 1;

		partial void OnVerticalSliderValueChanged(int value)
		{
			ShowPadVisible = true;
			WeakReferenceMessenger.Default.Send(new DifficultySliderChangeMessage(VerticalSliderValue, HorizontalSliderValue));
		}
		[ObservableProperty]
		public partial int HorizontalSliderValue { get; set; } = 1;

		partial void OnHorizontalSliderValueChanged(int value)
		{
			ShowPadVisible = true;
			WeakReferenceMessenger.Default.Send(new DifficultySliderChangeMessage(VerticalSliderValue, HorizontalSliderValue));
		}

		[ObservableProperty]
		public partial bool ShowPadVisible { get; set;  }

		[ObservableProperty]
		public partial string GameDifficultyText { get; set; }

		private readonly INavigationService _navigationService;
		public DifficultySettingViewModel(INavigationService navService)
		{
			_navigationService = navService;
		}

		[RelayCommand]
		private void ReturnToMenu()
		{
			_navigationService.NavigateTo(nameof(MainMenuPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
		}

		[RelayCommand]
		private void StartGame()
		{
			if (VerticalSliderValue * HorizontalSliderValue % 2 == 1)
			{
				WeakReferenceMessenger.Default.Send(new WrongRowColumnSetPromptMessage());
				return;
			}
			_navigationService.NavigateTo(nameof(GameWindowPage), new GameOptions() { UserSetRows = VerticalSliderValue, UserSetColumns = HorizontalSliderValue, GameLevels = GameLevels.Custom }, new CommonNavigationTransitionInfo());
		}

		[RelayCommand]
		private void PointerExitFromSliderOrGamePad()
		{
			ShowPadVisible = false;

			var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
			var gameCustomHint = resourceLoader.GetString("GameCustomHintText");
			var row = resourceLoader.GetString("Row");
			var column = resourceLoader.GetString("Column");
			var rowColumnLinkText = resourceLoader.GetString("RowColumnLinkText");

			GameDifficultyText = $"{gameCustomHint} {VerticalSliderValue} {row} {rowColumnLinkText} {HorizontalSliderValue} {column}!";
		}
	}
}
