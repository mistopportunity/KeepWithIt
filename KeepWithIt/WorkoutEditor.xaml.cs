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

	public sealed partial class WorkoutEditor:Page {
		public WorkoutEditor() {
			this.InitializeComponent();
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

		bool inSubEditor = false;

		private void CoreWindow_KeyPressEvent(CoreWindow sender,KeyEventArgs args) {
			if(inSubEditor) {

			} else {
				switch(args.VirtualKey) {
					case VirtualKey.Back:
					case VirtualKey.GoBack:
					case VirtualKey.Escape:
					case VirtualKey.GamepadB:
					case VirtualKey.NavigationCancel:
						Frame.GoBack();
						break;
					case VirtualKey.GamepadA:
					case VirtualKey.Enter:
					case VirtualKey.NavigationAccept:

						break;
					case VirtualKey.Up:
					case VirtualKey.GamepadDPadUp:
					case VirtualKey.NavigationUp:

						break;
					case VirtualKey.Down:
					case VirtualKey.GamepadDPadDown:
					case VirtualKey.NavigationDown:

						break;
					case VirtualKey.Left:
					case VirtualKey.GamepadDPadLeft:
					case VirtualKey.NavigationLeft:

						break;
					case VirtualKey.Right:
					case VirtualKey.GamepadDPadRight:
					case VirtualKey.NavigationRight:

						break;
				}
			}
		}


		private void CurrentView_BackRequested(object sender,BackRequestedEventArgs e) {
			if(!inSubEditor) {
				Frame.GoBack();
			}
		}

	}
}
