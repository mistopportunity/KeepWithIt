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
//Todo: audio on this page because I had to write it without headphones because my computer is across the fucking room

namespace KeepWithIt {
	public sealed partial class WorkoutPage:Page {
		public WorkoutPage() {
			this.InitializeComponent();
			timer = new DispatcherTimer();
			timer.Tick += Timer_Tick;
			timer.Interval = new TimeSpan(0,0,1);
		}

		private Workout currentWorkout;

		private int segmentIndex = 0;
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			Window.Current.CoreWindow.KeyDown += CoreWindow_KeyPressEvent;

			currentWorkout = e.Parameter as Workout;
			titleBlock.Text = currentWorkout.Name;

			changeSegment(segmentIndex);

			startButton.Visibility = Visibility.Visible;

			segmentTitleBlock.Visibility = Visibility.Collapsed;
			repInfoBlock.Visibility = Visibility.Collapsed;
			progressBar.Visibility = Visibility.Collapsed;
			pauseButton.Visibility = Visibility.Collapsed;

			startButton.Focus(FocusState.Programmatic);
			startButton.FocusDisengaged += (sender,handler) => {
				if(startButton.IsEnabled) {
					startButton.Focus(FocusState.Programmatic);
				}
			};
			startButton.Tapped += (sender,handler) => {
				startButton.IsEnabled = false;
				startButton.Visibility = Visibility.Collapsed;
				handler.Handled = true;
				StartWorkout();
			};


			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
			currentView.BackRequested += CurrentView_BackRequested;
		}


		private void StartWorkout() {
			segmentTitleBlock.Visibility = Visibility.Visible;
			repInfoBlock.Visibility = Visibility.Visible;
			progressBar.Visibility = Visibility.Visible;
			pauseButton.Visibility = Visibility.Visible;
			if(totalSeconds != 0) {
				timer.Start();
			}
		}

		private void leftButton_Tapped(object sender,TappedRoutedEventArgs e) {
			changeSegment(--segmentIndex);
		}

		private void ExitWorkout() {
			//ask the user if they are sure they want to do this? - only if workoutCompleted is false
			((App)Application.Current).AWeirdPlaceForAWorkoutObjectThatIsViolatingCodingPrincipals = currentWorkout;
			Frame.GoBack();
		}

		private void rightButton_Tapped(object sender,TappedRoutedEventArgs e) {
			if(++segmentIndex == currentWorkout.Segments.Count) {
				//congratulate the user in some way?
				ExitWorkout();
			} else {
				changeSegment(segmentIndex);
			}
		}

		//expects a not negative index

		private DispatcherTimer timer;
		private void changeSegment(int index) {

			var segment = currentWorkout.Segments[index];

			if(index == 0) {
				leftImage.Source = null;
				leftButton.IsEnabled = false;
			} else {
				leftImage.Source = currentWorkout.Segments[index-1].PreviewImage;
				leftButton.IsEnabled = true;
			}

			middleImage.Source = segment.PreviewImage;

			if(currentWorkout.Segments.Count > index + 1) {
				rightImage.Source = currentWorkout.Segments[index + 1].PreviewImage;
			} else {
				//set finish line image to rightImage
			}
			repInfoBlock.Text = segment.GetRepDescription();
			segmentTitleBlock.Text = segment.Name;
			progressBar.Value = 0;
			if(segment.Seconds == 0) {
				timer.Stop();
				progressBar.IsIndeterminate = true;
				remainingSeconds = -1;
				totalSeconds = 0;
				rightButton.IsEnabled = true;
				addCompletionTimeIfApplicable();
			} else {
				progressBar.IsIndeterminate = false;
				remainingSeconds = segment.Seconds;
				totalSeconds = remainingSeconds;
				progressBar.Maximum = totalSeconds;
				progressBar.Value = 0;
				rightButton.IsEnabled = false;
				if(!startButton.IsEnabled && !isTimerPasued) {
					timer.Start();
				}
			}
			UpdateSecondsLabel();
		}

		private void addCompletionTimeIfApplicable() {
			if(!completedWorkout && segmentIndex + 1 == currentWorkout.Segments.Count) {
				currentWorkout.AddDate(DateTime.Now);
				completedWorkout = true;
			}
		}

		private void Timer_Tick(object sender,object e) {
			remainingSeconds--;
			progressBar.Value = totalSeconds - remainingSeconds;
			if(remainingSeconds == 0) {
				timerFinished();
			}
			UpdateSecondsLabel();
		}


		private void timerFinished() {
			rightButton.IsEnabled = true;
			timer.Stop();
			addCompletionTimeIfApplicable();
		}

		private void pauseButton_Tapped(object sender,TappedRoutedEventArgs e) {
			isTimerPasued = !isTimerPasued;
			if(totalSeconds != -1) {
				if(isTimerPasued) {
					timer.Stop();
				} else {
					if(remainingSeconds > 0) {
						timer.Start();
					}
				}
			}
			UpdateSecondsLabel();
		}

		private int remainingSeconds = 0;
		private int totalSeconds = -1;
		private bool isTimerPasued = false;
		private void UpdateSecondsLabel() {
			if(isTimerPasued) {
				if(remainingSeconds > 0) {
					pauseButton.Content = "timer pasued";
				} else {
					if(totalSeconds != -1) {
						pauseButton.Content = "timer finished - paused";
					} else {
						pauseButton.Content = "no timer - paused";
					}

				}
			} else {
				if(totalSeconds == -1) {
					pauseButton.Content = "no timer";
				} else {
					if(remainingSeconds > 0) {
						pauseButton.Content = $"{remainingSeconds}s left";
					} else {
						pauseButton.Content = "timer finished";
					}
				}
			}
		}

		private bool completedWorkout = false;
		private void CurrentView_BackRequested(object sender,BackRequestedEventArgs e) {
			ExitWorkout();
		}

		private void CoreWindow_KeyPressEvent(CoreWindow sender,KeyEventArgs args) {
			//Todo - controller and keyboard navigation
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
			base.OnNavigatingFrom(e);
			Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyPressEvent;

			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			currentView.BackRequested -= CurrentView_BackRequested;

		}

		private void Page_LayoutUpdated(object sender,object e) {

		}
	}
}
