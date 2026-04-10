using CommunityToolkit.WinUI.Converters;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lianliankan_WinUI.Converters
{
	public class InversedBoolToVisibilityConverter : BoolToObjectConverter
	{
		public InversedBoolToVisibilityConverter()
		{
			base.TrueValue = Visibility.Collapsed;
			base.FalseValue = Visibility.Visible;
		}
	}
}
