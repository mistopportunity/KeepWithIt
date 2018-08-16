using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace KeepWithIt {
	public sealed partial class AboutPage:Page {
		public AboutPage() {
			this.InitializeComponent();
		}
		private void Page_LayoutUpdated(object sender,object e) {
			if(ActualWidth < ActualHeight) {
				var gridLength = new GridLength(0,GridUnitType.Star);
				Column1.Width = gridLength;
				Column3.Width = gridLength;
			} else {
				var gridLength = new GridLength(1,GridUnitType.Star);
				Column1.Width = gridLength;
				Column3.Width = gridLength;
			}
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			Window.Current.CoreWindow.KeyDown += CoreWindow_KeyPressEvent;
			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
			currentView.BackRequested += CurrentView_BackRequested;

		}
		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
			base.OnNavigatingFrom(e);
			Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyPressEvent;
			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			currentView.BackRequested -= CurrentView_BackRequested;
		}
		private void CurrentView_BackRequested(object sender,BackRequestedEventArgs e) {
			Frame.GoBack();
		}
		private void CoreWindow_KeyPressEvent(CoreWindow sender,KeyEventArgs args) {
			switch(args.VirtualKey) {
				case VirtualKey.Escape:
				case VirtualKey.GamepadB:
				case VirtualKey.NavigationCancel:
					Frame.GoBack();
					break;
			}
		}
	}
}
