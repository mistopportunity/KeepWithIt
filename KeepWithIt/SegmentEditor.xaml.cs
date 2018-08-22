using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace KeepWithIt {
	public sealed partial class SegmentEditor:Page {
		private WorkoutSegment segment;
		public SegmentEditor() {
			this.InitializeComponent();
		}
		private bool stillOnThePage = true;
		private bool openingOrProcessingImage = false;
		private async void OpenImagePrompt() {
			if(openingOrProcessingImage || imageLoading) {
				return;
			}
			setPictureLabelText("Selecting image");
			openingOrProcessingImage = true;
			FileOpenPicker fileOpenPicker = new FileOpenPicker();
			fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
			fileOpenPicker.FileTypeFilter.Add(".jpg");
			fileOpenPicker.FileTypeFilter.Add(".png");
			fileOpenPicker.FileTypeFilter.Add(".jpeg");
			fileOpenPicker.FileTypeFilter.Add(".gif");
			fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
			ElementSoundPlayer.Play(ElementSoundKind.Show);
			var file = await fileOpenPicker.PickSingleFileAsync();
			if(file == null) {
				if(stillOnThePage) {
					UpdatePicture();
					openingOrProcessingImage = false;
				}
				return;
			}
			setPictureLabelText("Loading new image");
			SoftwareBitmap softwareBitmap;
			try {
				softwareBitmap = await WorkoutManager.GetBitMapFromFile(file);
			} catch {
				softwareBitmap = null;
			}

			if(softwareBitmap != null) {
				setPictureLabelText("Processing new image");
				segment.PropertyChanged += Segment_PropertyChanged;
				segment.SetImage(softwareBitmap);
				imageLoading = true;
			} else {
				ElementSoundPlayer.Play(ElementSoundKind.Show);
				MessageDialog messageDialog = new MessageDialog("Error opening new image. You know what sucks?") {
					DefaultCommandIndex = 0,
					CancelCommandIndex = 0
				};
				messageDialog.Commands.Add(new UICommand("Well, this sucks?"));
				await messageDialog.ShowAsync();
			}
			ElementSoundPlayer.Play(ElementSoundKind.Hide);
			if(stillOnThePage) {
				openingOrProcessingImage = false;
				UpdatePicture();
			}
		}

		private void setPictureLabelText(string text) {
			pictureLabel.Text = text;
		}

		private bool imageLoading = false;
		private void Segment_PropertyChanged(object sender,System.ComponentModel.PropertyChangedEventArgs e) {
			if(e.PropertyName == "UsableImage") {
				segment.PropertyChanged -= Segment_PropertyChanged;
				imageLoading = false;
				UpdatePicture();
			}
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			Window.Current.CoreWindow.KeyDown += CoreWindow_KeyPressEvent;

			stillOnThePage = true;

			segment = e.Parameter as WorkoutSegment;

			nameBox.PlaceholderText = segment.Name;
			nameBox.Text = segment.Name;

			reps = segment.Reps;
			seconds = segment.Seconds;

			doubleSidedToggle.IsChecked = segment.DoubleSided;

			UpdatePicture();

			UpdateRepsLabel();
			UpdateSecondsLabel();


			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
			currentView.BackRequested += CurrentView_BackRequested;
		}

		private void UpdatePicture() {
			if(openingOrProcessingImage || imageLoading) {
				return;
			}
			if(segment.GetImage() == null) {
				setPictureLabelText("No image selected");
				backImage.Visibility = Visibility.Visible;
			} else {
				setPictureLabelText("Image seleceted");
				backImage.Visibility = Visibility.Collapsed;
				frontImage.Source = segment.UsableImage;
			}
		}

		private void GoBack() {
			if(!string.IsNullOrWhiteSpace(nameBox.Text)) {
				segment.Name = nameBox.Text;
			}

			if(doubleSidedToggle.IsChecked.HasValue) {
				segment.DoubleSided = doubleSidedToggle.IsChecked.Value;
			} else {
				segment.DoubleSided = false;
			}

			segment.Reps = reps;
			segment.Seconds = seconds;

			stillOnThePage = false;
			Frame.GoBack();
		}

		private void CurrentView_BackRequested(object sender,BackRequestedEventArgs e) {
			e.Handled = true;
			GoBack();
		}

		private void CoreWindow_KeyPressEvent(CoreWindow sender,KeyEventArgs args) {
			switch(args.VirtualKey) {
				case VirtualKey.Escape:
				case VirtualKey.GamepadB:
				case VirtualKey.NavigationCancel:
					GoBack();
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
					FocusManager.TryMoveFocus(FocusNavigationDirection.Previous);
					break;
				case VirtualKey.Down:
				case VirtualKey.GamepadDPadDown:
				case VirtualKey.NavigationDown:
					FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
					break;
				case VirtualKey.Left:
				case VirtualKey.GamepadDPadLeft:
				case VirtualKey.NavigationLeft:
					if(FocusManager.GetFocusedElement() != nameBox) {
						FocusManager.TryMoveFocus(FocusNavigationDirection.Previous);
					}
					break;
				case VirtualKey.Right:
				case VirtualKey.GamepadDPadRight:
				case VirtualKey.NavigationRight:
					if(FocusManager.GetFocusedElement() != nameBox) {
						FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
					}
					break;
			}
		}
		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
			base.OnNavigatingFrom(e);
			Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyPressEvent;
			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			currentView.BackRequested -= CurrentView_BackRequested;
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

		private void deleteButton_Click(object sender,RoutedEventArgs e) {
			WorkoutEditor.PendingDeletion = segment;
			stillOnThePage = false;
			Frame.GoBack();
		}

		private int reps;
		private int seconds;

		private const int repsIterationAmount = 5;
		private const int secondsIterationAmount = 5;

		private void UpdateSecondsLabel() {
			if(seconds == 0) {
				secondsLabelBlock.Text = "No timer";
			} else {
				secondsLabelBlock.Text = $"{seconds}s timer";
			}
		}
		private void UpdateRepsLabel() {
			if(reps == 0) {
				repsLabelBlock.Text = "No rep count";
			} else {
				repsLabelBlock.Text = $"{reps} reps";
			}
		}

		private void secondsMinusButton_Click(object sender,RoutedEventArgs e) {
			if(seconds == 0) {
				return;
			}
			seconds -= secondsIterationAmount;
			UpdateSecondsLabel();
		}

		private void secondsPlusButton_Click(object sender,RoutedEventArgs e) {
			if(seconds > int.MaxValue - secondsIterationAmount) {
				return;
			}
			seconds += secondsIterationAmount;
			UpdateSecondsLabel();
		}

		private void repsMinusButton_Click(object sender,RoutedEventArgs e) {
			if(reps == 0) {
				return;
			}
			reps -= repsIterationAmount;
			UpdateRepsLabel();
		}

		private void repsPlusButton_Click(object sender,RoutedEventArgs e) {
			if(reps > int.MaxValue - repsIterationAmount) {
				return;
			}
			reps += repsIterationAmount;
			UpdateRepsLabel();
		}

		private void Button_Click(object sender,RoutedEventArgs e) {
			OpenImagePrompt();
		}
	}
}
