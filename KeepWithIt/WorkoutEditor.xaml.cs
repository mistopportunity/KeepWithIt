using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		private string workoutDefaultName;

		private bool loaded = false;

		internal static WorkoutSegment PendingDeletion = null;

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(!loaded) {
				if(e.Parameter != null && e.Parameter is Workout) {
					workout = e.Parameter as Workout;
					nameBox.Text = workout.Name;
				} else {
					workout = new Workout();
					WorkoutManager.Workouts.Add(workout);
					WorkoutManager.SaveWorkout(workout);
					((App)Application.Current).WasThatComplicatedNavigationalMessFromANewWorkout = true;
				}

				workoutDefaultName = $"Workout {WorkoutManager.Workouts.IndexOf(workout)+1}";
				listView.ItemsSource = workout.Segments;

				if(workout.Name == null) {
					nameBox.Text = workoutDefaultName;
					workout.Name = workoutDefaultName;
				}
				nameBox.PlaceholderText = workoutDefaultName;

				nameBox.SelectionStart = nameBox.MaxLength-1;
				nameBox.SelectionLength = 0;
				loaded = true;
			} else if(PendingDeletion != null) {

				workout.Segments.Remove(PendingDeletion);

				PendingDeletion = null;

			}
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
		private int listViewIndex = 1;
		private void focusDown() {
			var focusedItem = FocusManager.GetFocusedElement();
			if(focusedItem == nameBox) {
				addButton.Focus(FocusState.Programmatic);
				ElementSoundPlayer.Play(ElementSoundKind.Focus);
			} else if(focusedItem == addButton) {
				if(listView.Items.Count == 0) {
					nameBox.Focus(FocusState.Programmatic);
					ElementSoundPlayer.Play(ElementSoundKind.Focus);
				} else {
					FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
					ElementSoundPlayer.Play(ElementSoundKind.Focus);
				}
			} else if(focusedItem == null || focusedItem is ScrollViewer) {
				nameBox.Focus(FocusState.Programmatic);
				ElementSoundPlayer.Play(ElementSoundKind.Focus);
			} else {
				listViewIndex++;
				if(listViewIndex > listView.Items.Count) {
					FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
					ElementSoundPlayer.Play(ElementSoundKind.Focus);
					listViewIndex = 1;
				}
			}
		}

		private void focusUp() {
			var focusedItem = FocusManager.GetFocusedElement();
			if(focusedItem == nameBox) {
				if(listView.Items.Count != 0) {
					listViewIndex = listView.Items.Count;
					FocusManager.TryMoveFocus(FocusNavigationDirection.Previous);
					ElementSoundPlayer.Play(ElementSoundKind.Focus);
				} else {
					addButton.Focus(FocusState.Programmatic);
					ElementSoundPlayer.Play(ElementSoundKind.Focus);
				}
			} else if(focusedItem == addButton) {
				nameBox.Focus(FocusState.Programmatic);
				ElementSoundPlayer.Play(ElementSoundKind.Focus);
			} else if(focusedItem == null || focusedItem is ScrollViewer) {
				nameBox.Focus(FocusState.Programmatic);
				ElementSoundPlayer.Play(ElementSoundKind.Focus);
			} else {
				listViewIndex--;
				if(listViewIndex < 1) {
					addButton.Focus(FocusState.Programmatic);
					ElementSoundPlayer.Play(ElementSoundKind.Focus);
					listViewIndex = 1;
				}
			}
		}

		private void CoreWindow_KeyPressEvent(CoreWindow sender,KeyEventArgs args) {
			switch(args.VirtualKey) {
				case VirtualKey.Escape:
				case VirtualKey.GamepadB:
				case VirtualKey.NavigationCancel:
					GoBackAndNibbaRigSomeShit();
					break;
				case VirtualKey.GamepadA:
				case VirtualKey.Enter:
				case VirtualKey.NavigationAccept:
					if(FocusManager.GetFocusedElement() == null) {
						nameBox.Focus(FocusState.Programmatic);
					}
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
					if(FocusManager.GetFocusedElement() != nameBox) {
						focusUp();
					}
					break;
				case VirtualKey.Right:
				case VirtualKey.GamepadDPadRight:
				case VirtualKey.NavigationRight:
					if(FocusManager.GetFocusedElement() != nameBox) {
						focusDown();
					}
					break;
			}
		}

		private void GoBackAndNibbaRigSomeShit() {
			((App)Application.Current).AWeirdPlaceForAWorkoutObjectThatIsViolatingCodingPrincipals = workout;
			loaded = false;
			Frame.GoBack();
			if(workout.Name == null) {
				if(string.IsNullOrEmpty(processedNameBox)) {
					workout.Name = workoutDefaultName;
				} else {
					workout.Name = processedNameBox;
				}
			} else {
				if(processedNameBox == string.Empty) {
					workout.Name = workoutDefaultName;
				} else if(processedNameBox != null) {
					workout.Name = processedNameBox;
				}
			}
			WorkoutManager.SaveWorkout(workout);
		}
		private void CurrentView_BackRequested(object sender,BackRequestedEventArgs e) {
			e.Handled = true;
			GoBackAndNibbaRigSomeShit();
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

		private string processedNameBox = null;
		private void nameBox_TextChanged(object sender,TextChangedEventArgs e) {
			if(string.IsNullOrWhiteSpace(nameBox.Text)) {
				processedNameBox = string.Empty;
				nameBox.Text = string.Empty;
			} else {
				processedNameBox = nameBox.Text;
			}
		}

		private void listView_ItemClick(object sender,ItemClickEventArgs e) {
			var item = e.ClickedItem as WorkoutSegment;
			Frame.Navigate(typeof(SegmentEditor),item);
		}

		private void addButton_Click(object sender,RoutedEventArgs e) {
			workout.Segments.Add(new WorkoutSegment() {
				Name = $"Segment {workout.Segments.Count+1}",
				Reps = 0,
				DoubleSided = false,
				Seconds = 0,
			});
			Frame.Navigate(typeof(SegmentEditor),workout.Segments.Last());
		}
	}
}
