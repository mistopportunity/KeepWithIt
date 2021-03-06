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
			this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;

			ElementSoundPlayer.State = ElementSoundPlayerState.On;

			this.Suspending += OnSuspending;

		}

		protected async override void OnFileActivated(FileActivatedEventArgs args) {
			Frame rootFrame = Window.Current.Content as Frame;
			var thisIsANewFrame = rootFrame == null;
			if(thisIsANewFrame) {
				await WorkoutManager.LoadWorkouts();
				rootFrame = new Frame();
				rootFrame.NavigationFailed += OnNavigationFailed;
				Window.Current.Content = rootFrame;
			}
			if(WorkoutEditor.workout != null) {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				WorkoutManager.SaveWorkout(WorkoutEditor.workout);
				WorkoutEditor.workout = null;
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
			if(args.Files.Count == 1) {
				var success  = await WorkoutManager.AddWorkout(args.Files[0] as StorageFile);
				if(success) {
					var workout = WorkoutManager.Workouts.Last();
					rootFrame.Navigate(typeof(MainPage),workout);
				} else {
					rootFrame.Navigate(typeof(MainPage));
				}
			} else {
				var successOnce = false;
				foreach(var file in args.Files) {
					var passed = await WorkoutManager.AddWorkout(file as StorageFile);
					if(passed) {
						successOnce = true;
					}
				}
				if(successOnce) {
					rootFrame.Navigate(typeof(MainPage),new UselessPotato());
				}
			}
			Window.Current.Activate();
		}

		protected async override void OnLaunched(LaunchActivatedEventArgs e) {
			Frame rootFrame = Window.Current.Content as Frame;
			if(rootFrame == null) {
				await WorkoutManager.LoadWorkouts();
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

		private async void OnSuspending(object sender,SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			if(WorkoutEditor.workout != null) {
				await WorkoutManager.SaveWorkout(WorkoutEditor.workout);
			}
			deferral.Complete();
		}
	}
}
