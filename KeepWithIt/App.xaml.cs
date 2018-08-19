﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

		internal Workout AWeirdPlaceForAWorkoutObjectThatIsViolatingCodingPrincipals = null;
		internal bool WasThatComplicatedNavigationalMessFromANewWorkout = false;

		public App() {

			this.InitializeComponent();
			this.Suspending += OnSuspending;

			WorkoutManager.LoadWorkouts();
			ElementSoundPlayer.State = ElementSoundPlayerState.On;

		}

		protected async override void OnFileActivated(FileActivatedEventArgs args) {
			Frame rootFrame = Window.Current.Content as Frame;
			if(rootFrame == null) {
				setRootFrame(rootFrame);
			}

			if(args.Files.Count == 1) {
				var success = await WorkoutManager.AddWorkout(args.Files[0] as StorageFile);
				if(success) {
					var workout = WorkoutManager.Workouts.Last();
					rootFrame.Navigate(typeof(MainPage),workout);
				} else {
					rootFrame.Navigate(typeof(MainPage));
				}
			} else {
				foreach(var file in args.Files) {
					await WorkoutManager.AddWorkout(file as StorageFile);
				}
				rootFrame.Navigate(typeof(MainPage),new UselessPotato());
			}
			Window.Current.Activate();
		}

		private void setRootFrame(Frame frame) {
			frame = new Frame();
			frame.NavigationFailed += OnNavigationFailed;
			Window.Current.Content = frame;
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e) {
			Frame rootFrame = Window.Current.Content as Frame;

			if(rootFrame == null) {

				setRootFrame(rootFrame);

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
			WorkoutManager.SaveWorkouts();
			deferral.Complete();
		}
	}
}
