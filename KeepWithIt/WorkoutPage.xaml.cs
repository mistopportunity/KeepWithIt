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
using Windows.UI.Composition;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using Windows.UI.Popups;
//Todo: audio on this page because I had to write it without headphones because my computer is across the fucking room

namespace KeepWithIt {
	public sealed partial class WorkoutPage:Page {
		public WorkoutPage() {
			this.InitializeComponent();
		}

		private static readonly BitmapImage missingPictureIcon = new BitmapImage(new Uri("ms-appx:///Assets/Default.png",UriKind.RelativeOrAbsolute));
		private static readonly BitmapImage checkerPattern = new BitmapImage(new Uri("ms-appx:///Assets/CheckerPattern.png",UriKind.RelativeOrAbsolute));

		private Workout currentWorkout;

		private int segmentIndex = 0;
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			Window.Current.CoreWindow.KeyDown += CoreWindow_KeyPressEvent;

			currentWorkout = e.Parameter as Workout;

			startButton.Visibility = Visibility.Visible;


			segmentTitleBlock.Visibility = Visibility.Collapsed;
			repInfoBlock.Visibility = Visibility.Collapsed;
			progressBar.Visibility = Visibility.Collapsed;
			pauseButton.Visibility = Visibility.Collapsed;

			startButton.Click += (sender,handler) => {
				startButtonClicked();
			};


			timer = new DispatcherTimer();
			timer.Tick += Timer_Tick;
			timer.Interval = new TimeSpan(0,0,1);

			changeSegment(segmentIndex);

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
				ElementSoundPlayer.Play(ElementSoundKind.MoveNext);
			}
		}

		private void startButtonClicked() {
			startButton.IsEnabled = false;
			startButton.Visibility = Visibility.Collapsed;
			StartWorkout();
		}

		private void leftButton_Click(object sender,RoutedEventArgs e) {
			changeSegment(--segmentIndex);
		}

		private void ExitWorkout() {
			//ask the user if they are sure they want to do this? - only if workoutCompleted is false
			((App)Application.Current).AWeirdPlaceForAWorkoutObjectThatIsViolatingCodingPrincipals = currentWorkout;
			Frame.GoBack();
		}

		private async void rightButton_Click(object sender,RoutedEventArgs e) {
			if(++segmentIndex == currentWorkout.Segments.Count) {
				ExitWorkout();
				MessageDialog messageDialog = new MessageDialog(
					"Workout completed! Congratulations. Day added to the workout calendar."
				) {
					DefaultCommandIndex = 0,
					CancelCommandIndex = 0
				};
				messageDialog.Commands.Add(new UICommand("Yay! I share this enthusiasm!"));
				await messageDialog.ShowAsync();
				ElementSoundPlayer.Play(ElementSoundKind.Hide);
			} else {
				changeSegment(segmentIndex);
			}
		}

		//expects a not negative index

		private DispatcherTimer timer;
		private void changeSegment(int index) {

			titleBlock.Text = currentWorkout.Name;

			var segment = currentWorkout.Segments[index];

			if(index == 0) {
				leftImage.Source = checkerPattern;
				leftOverlay.Visibility = Visibility.Collapsed;
				leftButton.IsEnabled = false;
			} else {
				leftOverlay.Visibility = Visibility.Visible;
				var leftSource = currentWorkout.Segments[index-1].UsableImage;
				if(leftSource != null) {
					leftImage.Source = leftSource;
				} else {
					leftImage.Source = missingPictureIcon;
				}

				leftButton.IsEnabled = true;
			}

			var middleSource = segment.UsableImage;
			if(middleSource != null) {
				middleImage.Source = middleSource;
			} else {
				middleImage.Source = missingPictureIcon;
			}


			if(currentWorkout.Segments.Count > index + 1) {
				var rightSource = currentWorkout.Segments[index + 1].UsableImage;
				if(rightSource != null) {
					rightImage.Source = rightSource;
				} else {
					rightImage.Source = missingPictureIcon;
				}
				rightOverlay.Visibility = Visibility.Visible;
			} else {
				rightOverlay.Visibility = Visibility.Collapsed;
				rightImage.Source = checkerPattern;
			}
			repInfoBlock.Text = segment.GetRepDescription();
			segmentTitleBlock.Text = $"{index+1}/{currentWorkout.Segments.Count} - {segment.Name}";
			progressBar.Value = 0;
			if(segment.Seconds == 0) {
				timer.Stop();
				progressBar.IsIndeterminate = true;
				remainingSeconds = -1;
				totalSeconds = 0;
				rightButton.IsEnabled = true;
				if(FocusManager.GetFocusedElement() == null) {
					rightButton.Focus(FocusState.Programmatic);
				}
				addCompletionTimeIfApplicable();
			} else {
				if(segment.DoubleSided) {
					runTimerTwice = true;
				}
				progressBar.IsIndeterminate = false;
				remainingSeconds = segment.Seconds;
				totalSeconds = remainingSeconds;
				progressBar.Maximum = totalSeconds;
				progressBar.Value = 0;
				rightButton.IsEnabled = false;
				if(!startButton.IsEnabled) {
					if(isTimerPasued) {
						if(FocusManager.GetFocusedElement() == null) {
							pauseButton.Focus(FocusState.Programmatic);
						}
					} else {
						timer.Start();
						ElementSoundPlayer.Play(ElementSoundKind.MoveNext);
					}
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
				UpdateSecondsLabel();
				return;
			}
			ElementSoundPlayer.Play(ElementSoundKind.Hide);
			UpdateSecondsLabel();
		}

		private bool runTimerTwice = false;

		private void timerFinished() {
			ElementSoundPlayer.Play(ElementSoundKind.Show);
			if(runTimerTwice) {
				runTimerTwice = false;
				remainingSeconds = totalSeconds;
				progressBar.Value = 0;
				return;
			}

			rightButton.IsEnabled = true;
			rightButton.Focus(FocusState.Programmatic);
			timer.Stop();
			addCompletionTimeIfApplicable();
		}

		private void pauseButton_Click(object sender,RoutedEventArgs e) {
			isTimerPasued = !isTimerPasued;
			if(totalSeconds != -1) {
				if(isTimerPasued) {
					timer.Stop();
				} else {
					if(remainingSeconds > 0) {
						timer.Start();
						ElementSoundPlayer.Play(ElementSoundKind.MoveNext);
					}
				}
			}
			UpdateSecondsLabel();
		}

		private int remainingSeconds = 0;
		private int totalSeconds = 0;
		private bool isTimerPasued = false;
		private void UpdateSecondsLabel() {
			if(isTimerPasued) {
				if(remainingSeconds > 0) {
					pauseButton.Content = "timer pasued";
				} else {
					if(totalSeconds != 0) {
						pauseButton.Content = "timer finished - paused";
					} else {
						pauseButton.Content = "no timer - paused";
					}

				}
			} else {
				if(totalSeconds == 0) {
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

		private void FocusTappedOrClickedOrWhateverTheHell() {
			if(!startButton.IsEnabled) {
				if(FocusManager.GetFocusedElement() == null) {
					if(rightButton.IsEnabled) {
						rightButton.Focus(FocusState.Programmatic);
					} else {
						pauseButton.Focus(FocusState.Programmatic);
					}
				}
			} else {
				startButton.Focus(FocusState.Programmatic);
			}
		}

		private void defaultFocus() {
			if(startButton.IsEnabled) {
				startButton.Focus(FocusState.Programmatic);
			} else {
				if(rightButton.IsEnabled) {
					rightButton.Focus(FocusState.Programmatic);
				} else {
					pauseButton.Focus(FocusState.Programmatic);
				}
			}
		}

		private void focusUp() {
			var focusedElement = FocusManager.GetFocusedElement();
			if(focusedElement == null || focusedElement == startButton) {
				defaultFocus();
			}
			if(focusedElement == pauseButton) {
				if(leftButton.IsEnabled) {
					leftButton.Focus(FocusState.Programmatic);
				}
			} else if(focusedElement == rightButton) {
				pauseButton.Focus(FocusState.Programmatic);
			}

		}
		private void focusDown() {
			var focusedElement = FocusManager.GetFocusedElement();
			if(focusedElement == null || focusedElement == startButton) {
				defaultFocus();
			}
			if(focusedElement == pauseButton) {
				if(rightButton.IsEnabled) {
					rightButton.Focus(FocusState.Programmatic);
				}
			} else if(focusedElement == leftButton) {
				pauseButton.Focus(FocusState.Keyboard);
			}
		}

		private void CoreWindow_KeyPressEvent(CoreWindow sender,KeyEventArgs args) {
			switch(args.VirtualKey) {
				case VirtualKey.Escape:
				case VirtualKey.GamepadB:
				case VirtualKey.NavigationCancel:
					ExitWorkout();
					break;
				case VirtualKey.GamepadA:
				case VirtualKey.Enter:
				case VirtualKey.NavigationAccept:
					FocusTappedOrClickedOrWhateverTheHell();
					break;
				case VirtualKey.Up:
				case VirtualKey.GamepadDPadUp:
				case VirtualKey.NavigationUp:
					focusUp();
					break;
				case VirtualKey.Down:
				case VirtualKey.GamepadDPadDown:
				case VirtualKey.NavigationDown:
					focusDown();
					break;
				case VirtualKey.Left:
				case VirtualKey.GamepadDPadLeft:
				case VirtualKey.NavigationLeft:
					focusUp();
					break;
				case VirtualKey.Right:
				case VirtualKey.GamepadDPadRight:
				case VirtualKey.NavigationRight:
					focusDown();
					break;
			}
		}

		protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
			base.OnNavigatingFrom(e);
			Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyPressEvent;

			timer.Stop();
			timer = null;

			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			currentView.BackRequested -= CurrentView_BackRequested;

			if(completedWorkout) {

				await WorkoutManager.SaveWorkouts();

			}
		}
	}
}
