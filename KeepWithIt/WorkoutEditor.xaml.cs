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

		private Workout workout;

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			Window.Current.CoreWindow.KeyDown += CoreWindow_KeyPressEvent;

			if(e.Parameter != null && e.Parameter is Workout) {
				workout = e.Parameter as Workout;
			} else {
				//create new workout with the workoutmanager
				
			}

			listBox.ItemsSource = workout.Segments;
			nameBox.Text = workout.Name;

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
			if(FocusManager.GetFocusedElement() == nameBox) {
				return;
			}
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

		private void Page_LayoutUpdated(object sender,object e) {
			if(ActualWidth > ActualHeight) {
				var gridLength = new GridLength(1,GridUnitType.Star);
				Column1.Width = gridLength;
				Column3.Width = gridLength;
			} else {
				var gridLength = new GridLength(0,GridUnitType.Star);
				Column1.Width = gridLength;
				Column3.Width = gridLength;
			}
		}

		private void nameBox_TextChanged(object sender,TextChangedEventArgs e) {

		}

		private void listView_ItemClick(object sender,ItemClickEventArgs e) {
			var item = e.ClickedItem as WorkoutSegment;
			item.Name = "Motherfucker you clicked me!";
			item.PreviewImage = null;
		}
	}
}
