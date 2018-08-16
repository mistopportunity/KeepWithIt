using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace KeepWithIt {
	sealed partial class App:Application {

		public App() {

			this.InitializeComponent();
			this.Suspending += OnSuspending;

			ElementSoundPlayer.State = ElementSoundPlayerState.On;

			WorkoutManager.LoadWorkouts();

		}

		protected async override void OnFileActivated(FileActivatedEventArgs args) {
			// TODO: Handle file activation
			// The number of files received is args.Files.Size
			// The name of the first file is args.Files[0].Name
			Frame rootFrame = Window.Current.Content as Frame;
			if(rootFrame == null) {
				rootFrame = new Frame();
				rootFrame.NavigationFailed += OnNavigationFailed;
				Window.Current.Content = rootFrame;
			}

			if(args.Files.Count == 1) {
				var success = await WorkoutManager.AddWorkout(args.Files[0]);
				if(success) {
					rootFrame.Navigate(typeof(MainPage),WorkoutManager.Workouts.Last());
				} else {
					rootFrame.Navigate(typeof(MainPage));
				}
			} else {
				foreach(var file in args.Files) {
					await WorkoutManager.AddWorkout(file);
				}
				rootFrame.Navigate(typeof(MainPage));
			}
			Window.Current.Activate();
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e) {
			Frame rootFrame = Window.Current.Content as Frame;

			if(rootFrame == null) {
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;


				Window.Current.Content = rootFrame;
			}

			if(e.PrelaunchActivated == false) {

				if(rootFrame.Content == null) {

					rootFrame.Navigate(typeof(MainPage),e.Arguments);
				}

				Window.Current.Activate();
			}
		}

		void OnNavigationFailed(object sender,NavigationFailedEventArgs e) {
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		private void OnSuspending(object sender,SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			deferral.Complete();
		}
	}
}
