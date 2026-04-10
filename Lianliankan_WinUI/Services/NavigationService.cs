using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Networking.Connectivity;

namespace Lianliankan_WinUI.Services
{
	public class NavigationService : INavigationService
	{
		private Frame _frame;
		private readonly Dictionary<string, Type> _pageRegistry = [];

		// 注册页面（在 App 启动时调用）
		public void RegisterPage(string key, Type pageType) => _pageRegistry.Add(key, pageType);

		// 初始化 Frame (在 MainWindow/Page 中设置）
		public void Initialize(Frame frame) => _frame = frame;

		public void NavigateTo(string pageKey, object parameter = null, NavigationTransitionInfo infoOverride=null)
		{
			if (_pageRegistry.TryGetValue(pageKey, out Type pageType))
			{
				_frame.Navigate(pageType, parameter,infoOverride);
			}
			else
				throw new ArgumentException($"Page {pageKey} not registered.");
		}
	}
}
