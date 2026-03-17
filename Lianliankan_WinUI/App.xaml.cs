using Lianliankan_WinUI.Models;
using Lianliankan_WinUI.Pages;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Lianliankan_WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {

        private Windows.Graphics.SizeInt32 m_InitialSize;
        private int m_RowFactor = 9;
        private int m_ColumnFactor = 12;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();

            m_InitialSize = this.m_window.AppWindow.ClientSize;

            // Create a Frame to act as the navigation context and navigate to the first page
            Frame rootFrame = new Frame();
            rootFrame.NavigationFailed += OnNavigationFailed;

            rootFrame.Navigating += RootFrame_Navigating;

            // Navigate to the first page, configuring the new page
            // by  passing required information as a navigation parameter
            rootFrame.Navigate(typeof(MainMenuPage), args.Arguments, new DrillInNavigationTransitionInfo());

            // Place the frame in the current window
            m_window.Content = rootFrame;
			// 临时措施，强制设置为浅色主题，由于游戏界面是为浅色主题设计的，强制设置为浅色主题可以避免一些控件在深色主题下显示异常的问题
			if (m_window.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme.Light;
            }
			// Ensure the MainWindow is active
			m_window.Activate();
        }

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var parameter = e.Parameter;

            if (parameter is GameOptions gameOptions)
            {
                this.m_window.AppWindow.ResizeClient(new Windows.Graphics.SizeInt32((int)Math.Round(m_InitialSize.Width * ((double)gameOptions.UserSetColumns / m_ColumnFactor)), (int)Math.Round(m_InitialSize.Height * ((double)gameOptions.UserSetRows / m_RowFactor))));
            }
            else
            {
                this.m_window.AppWindow.ResizeClient(m_InitialSize);
            }

            IntPtr hWnd = WindowNative.GetWindowHandle(this.m_window);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var displayArea = DisplayArea.GetFromWindowId(wndId,DisplayAreaFallback.Nearest);

            var halfScreenWidth = displayArea.WorkArea.Width / 2;
            var halfWindowWidth = this.m_window.AppWindow.Size.Width / 2;
            var x = halfScreenWidth - halfWindowWidth;

            var halfScreenHeight = displayArea.WorkArea.Height / 2;
            var halfWindowHieght = this.m_window.AppWindow.Size.Height / 2;
            var y = halfScreenHeight - halfWindowHieght;
            // center the window
            this.m_window.AppWindow.Move(new Windows.Graphics.PointInt32(x>0?x:0, y>0?y:0));
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new NotImplementedException("Failed to load Page " + e.SourcePageType.FullName);
        }

        private Window m_window;
    }
}
