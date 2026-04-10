using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lianliankan_WinUI.Services
{
	public interface INavigationService
	{
		public void RegisterPage(string key, Type pageType);
		public void Initialize(Frame frame);
		void NavigateTo(string pageKey, object parameter = null, NavigationTransitionInfo infoOverride = null);
	}
}
