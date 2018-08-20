using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//Todo: Make the segment editor - we saved the best for last... woot

namespace KeepWithIt {
	public sealed partial class SegmentEditor:Page {
		private WorkoutSegment segment;
		public SegmentEditor() {
			this.InitializeComponent();
		}

		private bool openingOrProcessingImage = false;
		private async void OpenImagePrompt() {
			if(openingOrProcessingImage) {
				return;
			}
			openingOrProcessingImage = true;
			FileOpenPicker fileOpenPicker = new FileOpenPicker();
			fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
			fileOpenPicker.FileTypeFilter.Add(".jpg");
			fileOpenPicker.FileTypeFilter.Add(".png");
			fileOpenPicker.FileTypeFilter.Add(".jpeg");
			fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
			ElementSoundPlayer.Play(ElementSoundKind.Show);
			var file = await fileOpenPicker.PickSingleFileAsync();
			if(file == null) {
				return;
			}
			var softwareBitmap = await WorkoutManager.GetBitMapFromFile(file);
			if(softwareBitmap != null) {
				segment.SetImage(softwareBitmap);
			} else {
				ElementSoundPlayer.Play(ElementSoundKind.Show);
				MessageDialog messageDialog = new MessageDialog("Error opening image. You know what sucks?") {
					DefaultCommandIndex = 0,
					CancelCommandIndex = 0
				};
				messageDialog.Commands.Add(new UICommand("Well, that sucks"));
				await messageDialog.ShowAsync();
			}
			ElementSoundPlayer.Play(ElementSoundKind.Hide);
			openingOrProcessingImage = false;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			Window.Current.CoreWindow.KeyDown += CoreWindow_KeyPressEvent;

			segment = e.Parameter as WorkoutSegment;

			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
			currentView.BackRequested += CurrentView_BackRequested;
		}

		private void GoBack() {
			//Todo: Push changes onto the segment object
			Frame.GoBack();
		}

		private void CurrentView_BackRequested(object sender,BackRequestedEventArgs e) {
			GoBack();
		}

		private void CoreWindow_KeyPressEvent(CoreWindow sender,KeyEventArgs args) {
			//Todo: Handle key presses
		}
		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
			base.OnNavigatingFrom(e);
			Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyPressEvent;
			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			currentView.BackRequested -= CurrentView_BackRequested;
		}
	}
}
