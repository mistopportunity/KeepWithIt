using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
//Todo make this page

namespace KeepWithIt {
	public sealed partial class WorkoutPage:Page {
		public WorkoutPage() {
			this.InitializeComponent();

		}

		private Workout currentWorkout;

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			Window.Current.CoreWindow.KeyDown += CoreWindow_KeyPressEvent;

			currentWorkout = e.Parameter as Workout;

			titleBlock.Text = currentWorkout.Name;

			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
			currentView.BackRequested += CurrentView_BackRequested;
		}

		private void CoreWindow_KeyPressEvent(CoreWindow sender,KeyEventArgs args) {

		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
			base.OnNavigatingFrom(e);
			Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyPressEvent;

			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			currentView.BackRequested -= CurrentView_BackRequested;

		}

		private bool completedWorkout = false;
		private void CurrentView_BackRequested(object sender,BackRequestedEventArgs e) {
			if(completedWorkout) {
				currentWorkout.AddDate(DateTime.Today);
			}
			Frame.GoBack();
		}

		private void Page_LayoutUpdated(object sender,object e) {

		}
	}
}
