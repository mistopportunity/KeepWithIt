﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using Windows.UI.Popups;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace KeepWithIt {
	public sealed partial class MainPage:Page {

		private const double focusSizeDivider = 50;
		private double iconSizeMultiplier = 2;

		public MainPage() {
			this.InitializeComponent();
		}

		private bool loadedSquaresForTheFirstDamnTime = false;
		private void LoadSquaresForTheFirstDamnTime() {
			loadedSquaresForTheFirstDamnTime = true;
			AddInterfaceSquares();
			foreach(var workout in WorkoutManager.Workouts) {
				var workoutGrid = workout.GetGrid();
				AddSquare(workoutGrid);
			}
		}

		private void UpdateSquaresSinceNewOnesWereAdded() {
			//compare the current number of squaresGrid children against WorkoutEditor's workouts list count
			var originalCount = squaresGrid.Children.Count - interfaceSquaresCount;
			var newCount = WorkoutManager.Workouts.Count;

			var difference = newCount - originalCount;

			for(int i = 0;i<difference;i++) {

				AddSquare(WorkoutManager.Workouts[originalCount + i].GetGrid());

			}

		}

		private void AddedANewDamnWorkoutToTheMix(Workout workout) {
			AddSquare(workout.GetGrid());
		}

		private void DeleteAWorkoutRestInPeace(int workoutIndex) {
			var squaresGridIndex = workoutIndex + interfaceSquaresCount;
			if(selectedIndex == squaresGridIndex) {
				selectedIndex = squaresGridIndex - 1;
				selectedGrid = null;
				lastSelectedIndex = 0;
			}
			squaresGrid.Children.RemoveAt(squaresGridIndex);

			for(int i = squaresGridIndex;i<squaresGrid.Children.Count;i++) {
				UpdateSquareLayout(squaresGrid.Children[i] as Grid,i);
			}


		}

		private void UpdateASpecificSquare(int workoutIndex) {
			var workout = WorkoutManager.Workouts[workoutIndex];
			var squaresGridIndex = workoutIndex + interfaceSquaresCount;

			squaresGrid.Children.RemoveAt(squaresGridIndex);

			AddSquare(workout.GetGrid(),squaresGridIndex);

		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {

			ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

			var app = ((App)Application.Current);
			if(app.AWeirdPlaceForAWorkoutObjectThatIsViolatingCodingPrincipals != null) {
				if(app.WasThatComplicatedNavigationalMessFromANewWorkout) {
					AddedANewDamnWorkoutToTheMix(app.AWeirdPlaceForAWorkoutObjectThatIsViolatingCodingPrincipals);
				}
				PostImportSelect(app.AWeirdPlaceForAWorkoutObjectThatIsViolatingCodingPrincipals,false);
				UpdateASpecificSquare(presentedSquareIndex);
				app.AWeirdPlaceForAWorkoutObjectThatIsViolatingCodingPrincipals = null;
				app.WasThatComplicatedNavigationalMessFromANewWorkout = false;
			} else {
				if(!loadedSquaresForTheFirstDamnTime) {
					LoadSquaresForTheFirstDamnTime();
				} else {
					UpdateSquaresSinceNewOnesWereAdded();
				}
				if(e.Parameter is Workout) {
					PostImportSelect(e.Parameter as Workout,true);
				} else if(e.Parameter is UselessPotato) {
					var workout = WorkoutManager.Workouts.Last();
					PostImportSelect(workout,true);
				}
			}

			userIsAwayFromThisPlace = false;
			Window.Current.CoreWindow.KeyDown += CoreWindow_KeyPressEvent;

		}
		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {

			Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyPressEvent;

			UnsubscribeBackButton();

		}


		private void Page_Loaded(object sender,RoutedEventArgs e) {
			UpdateScroll();
		}

		private void CreateInterfaceSquare(string text,string imagePath) {
			Uri uri = new Uri(imagePath,UriKind.RelativeOrAbsolute);
			SvgImageSource imageSource = new SvgImageSource(uri);

			Grid grid = new Grid();

			grid.ColumnDefinitions.Add(new ColumnDefinition() {
				Width = new GridLength(1,GridUnitType.Star)
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition() {
				Width = new GridLength(iconSizeMultiplier,GridUnitType.Star)
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition() {
				Width = new GridLength(1,GridUnitType.Star)
			});

			grid.RowDefinitions.Add(new RowDefinition() {
				Height = new GridLength(4,GridUnitType.Star)
			});
			grid.RowDefinitions.Add(new RowDefinition() {
				Height = new GridLength(1,GridUnitType.Star)
			});


			var image = new Image() {
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Source = imageSource,
				Stretch = Stretch.Uniform,
			};

			var textBlock = new TextBlock() {
				Text = text,
				Foreground = new SolidColorBrush(Colors.Black),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Center,
				FontSize = 20,
				TextAlignment = TextAlignment.Center
			};

			grid.Children.Add(image);
			grid.Children.Add(textBlock);

			Grid.SetRow(textBlock,2);
			Grid.SetColumnSpan(textBlock,3);
			Grid.SetColumn(image,1);

			grid.Background = new SolidColorBrush(Colors.White);

			AddSquare(grid);
		}
		private int interfaceSquaresCount = 0;
		private void AddInterfaceSquares() {

			CreateInterfaceSquare("Create","ms-appx:///Assets/plus.svg");

			CreateInterfaceSquare("Import","ms-appx:///Assets/download.svg");

			CreateInterfaceSquare("About","ms-appx:///Assets/question.svg");


			interfaceSquaresCount = 3;
		}

		private Grid GetPresentationSquare() {
			return WorkoutManager.Workouts[presentedSquareIndex].GetPresentationGrid();
		}

		private Grid presentedSquare = null;

		private void setButtonsEnabled(bool enabled) {

			StartButton.IsEnabled = enabled;
			EditButton.IsEnabled = enabled;
			DeleteButton.IsEnabled = enabled;
			ExportButton.IsEnabled = enabled;

		}

		public void ClearPresentSquare(bool playSound) {
			if(presentedSquare != null) {
				TopGrid.Children.Remove(presentedSquare);
			}
			presentedSquare = null;
			presentedSquareIndex = -1;
			UnsubscribeBackButton();

			setButtonsEnabled(false);

			CentralizeSquares();

			if(playSound) {
				ElementSoundPlayer.Play(ElementSoundKind.MovePrevious);
			}

		}

		private int presentedSquareIndex = -1;

		public void PresentSquare(Grid square,bool playSound) {
			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
			currentView.BackRequested += CurrentView_BackRequested;

			calendar.SelectedDates.Clear();

			foreach(var date in WorkoutManager.Workouts[presentedSquareIndex].Dates) {
				calendar.SelectedDates.Add(date);
			}

			calendar.SetDisplayDate(DateTime.Today - TimeSpan.FromDays(14));

			if(presentedSquare != null) {
				TopGrid.Children.Remove(presentedSquare);
			}

			setButtonsEnabled(true);

			if(WorkoutManager.Workouts[presentedSquareIndex].Segments.Count < 1) {
				StartButton.IsEnabled = false;
				ExportButton.IsEnabled = false;
			}

			presentedSquare = square;
			PushSquaresLeft();
			TopGrid.Children.Add(square);

			if(playSound) {
				ElementSoundPlayer.Play(ElementSoundKind.MoveNext);
			}
		}

		private void SquareTapped(Grid grid) {

			if(importing)
				return;

			var gridIndex = squaresGrid.Children.IndexOf(grid);

			if(gridIndex < interfaceSquaresCount) {
				switch(gridIndex) {
					case 0:
						GotoCreation();
						break;
					case 1:
						GotoImport();
						break;
					case 2:
						GotoAbout();
						break;
				}
			} else {
				presentedSquareIndex = gridIndex - interfaceSquaresCount;

				var presentationSquare = GetPresentationSquare();

				PresentSquare(presentationSquare,true);

			}
		}

		private void UnsubscribeBackButton() {
			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			currentView.BackRequested -= CurrentView_BackRequested;
		}

		private void CurrentView_BackRequested(object sender,BackRequestedEventArgs e) {
			e.Handled = true;
			if(exporting) {
				return;
			}
			ClearPresentSquare(true);
		}

		private void UpdateSquareLayout(Grid grid,int? index = null) {
			int whereAreWeDoingThisShitAt;
			if(index == null) {
				whereAreWeDoingThisShitAt = squaresGrid.Children.Count;
			} else {
				whereAreWeDoingThisShitAt = index.Value;
			}
			if(whereAreWeDoingThisShitAt % 2 == 0) {
				grid.HorizontalAlignment = HorizontalAlignment.Right;
				Grid.SetColumn(grid,1);
			} else {
				grid.HorizontalAlignment = HorizontalAlignment.Left;
				Grid.SetColumn(grid,3);
			}
		}
		private void AddSquare(Grid grid,int? index = null) {
			UpdateSquareLayout(grid,index);

			grid.Tapped += Grid_Tapped;

			grid.PointerPressed += pointerBasedGridSelect;
			grid.PointerEntered += pointerBasedGridSelect;

			grid.PointerExited += Grid_PointerExited;

			if(index == null) {
				squaresGrid.Children.Add(grid);
			} else {
				squaresGrid.Children.Insert(index.Value,grid);
			}
		}

		private void Grid_Tapped(object sender,TappedRoutedEventArgs e) {
			SquareTapped(sender as Grid);
			e.Handled = true;
		}

		private bool squaresCentered = true;
		private void PushSquaresLeft() {
			Column1.Width = new GridLength(1,GridUnitType.Star);
			Column2.Width = new GridLength(0,GridUnitType.Star);
			Column3.Width = new GridLength(1,GridUnitType.Star);
			squaresCentered = false;
		}
		private void CentralizeSquares() {
			Column1.Width = new GridLength(0,GridUnitType.Star);
			Column2.Width = new GridLength(1,GridUnitType.Star);
			Column3.Width = new GridLength(0,GridUnitType.Star);
			squaresCentered = true;
		}
		private void layoutSquares() {

			var squareCount = squaresGrid.Children.Count;

			if(--squareCount == -1) {
				return;
			}

			var marginSize = CentralSubColumn.ActualWidth;


			var leftSize = ContentColumn1.ActualWidth;
			var rightSize = ContentColumn2.ActualWidth;

			void layoutSquare(int index,bool last) {
				var square = squaresGrid.Children[index] as Grid;

				double width;
				if(index % 2 == 0) {
					width = leftSize;
				} else {
					width = rightSize;
				}

				square.Width = width;
				square.Height = width;

				square.VerticalAlignment = VerticalAlignment.Top;

				square.Margin = new Thickness(

					0, marginSize + ((marginSize + width) * (index/2)),
					
					0, (!last ? 0 : marginSize)

				);

			}

			for(var i = 0;i<squareCount;i++) {

				layoutSquare(i,false);

			}

			layoutSquare(squareCount,true);
			

		}

		private void resetGridMode() {
			if(presentedSquare != null) {
				Grid.SetRow(presentedSquare,0);
				Grid.SetRowSpan(presentedSquare,2);
				Grid.SetColumn(presentedSquare,0);
				Grid.SetColumnSpan(presentedSquare,1);
			}
			Grid.SetRow(focusRightSide,0);
			Grid.SetRowSpan(focusRightSide,2);
			Grid.SetColumn(focusRightSide,2);
			Grid.SetColumnSpan(focusRightSide,1);
			calendar.Margin = new Thickness(0,100,15,15);
			focusRightSide.Margin = new Thickness(0,0,0,0);
		}
		private void mobilePresentView() {

			Grid.SetRow(focusRightSide,0);
			Grid.SetRowSpan(focusRightSide,1);
			Grid.SetColumn(focusRightSide,0);
			Grid.SetColumnSpan(focusRightSide,3);

			Grid.SetRow(presentedSquare,1);
			Grid.SetRowSpan(presentedSquare,1);
			Grid.SetColumn(presentedSquare,0);
			Grid.SetColumnSpan(presentedSquare,3);
			calendar.Margin = new Thickness(0,100,15,0);
			focusRightSide.Margin = new Thickness(15,0,0,0);
		}
		private bool gridAlignmentDefault = true;
		private void Page_LayoutUpdated(object sender,object e) {

			if(userIsAwayFromThisPlace) {
				return;
			}

			var usingMobileishMode = ActualWidth < ActualHeight;

			if(squaresCentered) {
				GridLength columnSize;
				if(usingMobileishMode) {
					columnSize = new GridLength(0.75,GridUnitType.Star);
				} else {
					columnSize = new GridLength(12,GridUnitType.Star);
				}
				SubColumnFirst.Width = columnSize;
				SubColumnLast.Width = columnSize;
				layoutSquares();
				if(!gridAlignmentDefault) {
					resetGridMode();
					gridAlignmentDefault = true;
				}
				updateSelectedGrid();

			} else {

				double leftSize;
				double rightSize;

				if(usingMobileishMode) {
					mobilePresentView();
					gridAlignmentDefault = false;

					leftSize = TopGrid.ActualHeight / 2;
					rightSize = TopGrid.ActualWidth;

				} else {
					resetGridMode();
					gridAlignmentDefault = true;

					leftSize = Column1.ActualWidth;
					rightSize = TopGrid.ActualHeight;

				}

				double actualActualWidth;
				if(leftSize > rightSize) {
					actualActualWidth = rightSize - (leftSize * 0.05);
				} else {
					actualActualWidth = leftSize * 0.95;
				}
				presentedSquare.Width = actualActualWidth;
				presentedSquare.Height = actualActualWidth;

			}
		
		}

		private int selectedIndex = -1;
		private int lastSelectedIndex = 0;
		private Grid selectedGrid = null;

		private void updateSelection(int xDelta,int yDelta) {
			if(selectedIndex == -1) {
				selectedIndex = lastSelectedIndex;
				selectedGrid = squaresGrid.Children[selectedIndex] as Grid;
				addSelectionAttributes(selectedGrid);
				ElementSoundPlayer.Play(ElementSoundKind.Focus);
			} else {

				var startIndex = selectedIndex;

				var squaresCount = squaresGrid.Children.Count;
				if(xDelta > 0) {
					if(selectedIndex % 2 == 0) {
						if(selectedIndex + 1 < squaresCount) {
							selectedIndex++;
						}
					}
				} else if(xDelta < 0) {
					if(selectedIndex % 2 == 1) {
						selectedIndex--;
					}
				}

				if(yDelta > 0) {
					if(selectedIndex >= squaresCount - 3) {

						if(selectedIndex % 2 == 0) {
							if(selectedIndex + 2 < squaresCount) {
								selectedIndex+=2;
							}
						} else {
							if(squaresCount % 2 != 1 && selectedIndex + 2 < squaresCount) {
								selectedIndex+=2;
							}
						}
					} else {
						selectedIndex += 2;
					}
				} else if(yDelta < 0) {
					if(selectedIndex >= 2) {
						selectedIndex -= 2;
					}

				}
				if(selectedIndex != startIndex) {
					removeSelectionAttributes(selectedGrid);
					selectedGrid = squaresGrid.Children[selectedIndex] as Grid;
					addSelectionAttributes(selectedGrid);
					UpdateScroll();
					lastSelectedIndex = selectedIndex;
					ElementSoundPlayer.Play(ElementSoundKind.Focus);
				}
			}
		}

		private void UpdateScroll() {
			if(selectedGrid != null) {
				squaresScroller.ChangeView(null,selectedGrid.ActualWidth * (selectedIndex / 2),null);
			}
		}

		private void removeSelectionAttributes(Grid grid) {

			if(grid != null) {
				grid.BorderThickness = new Thickness(0,0,0,0);
			}

		}

		private void updateSelectedGrid() {
			if(selectedGrid != null) {
				addSelectionAttributes(selectedGrid);
			}
		}

		private void addSelectionAttributes(Grid grid) {
			var thiccness = Math.Floor(-(grid.ActualWidth / focusSizeDivider));
			grid.BorderThickness = new Thickness(thiccness); //Oh God, the irony.

		}

		private void CoreWindow_KeyPressEvent(CoreWindow sender,KeyEventArgs args) {
			if(!squaresCentered) {
				switch(args.VirtualKey) {
					case VirtualKey.Escape:
						ClearPresentSquare(true);
						break;
					case VirtualKey.Right:
					case VirtualKey.Down:
						FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
						break;
					case VirtualKey.Left:
					case VirtualKey.Up:
						FocusManager.TryMoveFocus(FocusNavigationDirection.Previous);
						break;

				}
			} else {
				switch(args.VirtualKey) {
					case VirtualKey.GamepadA:
					case VirtualKey.Enter:
					case VirtualKey.NavigationAccept:
						if(selectedGrid != null) {
							SquareTapped(selectedGrid);
						}
						break;
					case VirtualKey.Up:
					case VirtualKey.GamepadDPadUp:
					case VirtualKey.NavigationUp:
					case VirtualKey.GamepadLeftThumbstickUp:
						updateSelection(0,-1);
						break;
					case VirtualKey.Down:
					case VirtualKey.GamepadDPadDown:
					case VirtualKey.NavigationDown:
					case VirtualKey.GamepadLeftThumbstickDown:
						updateSelection(0,1);
						break;
					case VirtualKey.Left:
					case VirtualKey.GamepadDPadLeft:
					case VirtualKey.NavigationLeft:
					case VirtualKey.GamepadLeftThumbstickLeft:
						updateSelection(-1,0);
						break;
					case VirtualKey.Right:
					case VirtualKey.GamepadDPadRight:
					case VirtualKey.NavigationRight:
					case VirtualKey.GamepadLeftThumbstickRight:
						updateSelection(1,0);
						break;
				}
			}
		}

		private void Grid_PointerExited(object sender,PointerRoutedEventArgs e) {
			if(selectedIndex != -1) {
				removeSelectionAttributes(selectedGrid);
				selectedGrid = null;
				selectedIndex = -1;
			}
		}

		private void pointerBasedGridSelect(object sender,PointerRoutedEventArgs e) {
			var senderGrid = sender as Grid;
			if(senderGrid == selectedGrid) {
				return;
			}
			if(selectedIndex != -1) {
				removeSelectionAttributes(selectedGrid);
			}
			selectedGrid = senderGrid;
			selectedIndex = squaresGrid.Children.IndexOf(selectedGrid);
			lastSelectedIndex = selectedIndex;
			addSelectionAttributes(selectedGrid);
			ElementSoundPlayer.Play(ElementSoundKind.Focus);
		}

		private bool exporting = false;
		private async void GotoExport() {
			if(!squaresCentered && !exporting) {
				exporting = true;
				var exportWorkout = WorkoutManager.Workouts[presentedSquareIndex];
				setButtonsEnabled(false);
				var savePicker = new FileSavePicker();
				savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
				savePicker.FileTypeChoices.Add("Keep with it Workout File",new List<string>() {".kwiw"});
				savePicker.SuggestedFileName = WorkoutManager.Workouts[presentedSquareIndex].Name;

				StorageFile file = await savePicker.PickSaveFileAsync();
				if(file != null) {
					CachedFileManager.DeferUpdates(file);
					var exportPassed = await WorkoutManager.ExportWorkout(file,exportWorkout,false);
					var status = FileUpdateStatus.Incomplete;
					if(exportPassed) {
						status = await CachedFileManager.CompleteUpdatesAsync(file);
					}
					if(status != FileUpdateStatus.Complete) {

						MessageDialog messageDialog = new MessageDialog("Workout couldn't be exported! SAD SAD SAD SAADDDDD SAD") {
							DefaultCommandIndex = 0,
							CancelCommandIndex = 0
						};
						messageDialog.Commands.Add(new UICommand("Okay :("));
						await messageDialog.ShowAsync();
					}
				} else {
					ElementSoundPlayer.Play(ElementSoundKind.Hide);
				}

				setButtonsEnabled(true);
				exporting = false;
			}
		}

		private bool userIsAwayFromThisPlace = false;

		private void GotoEditor() {
			if(!squaresCentered && !importing) {
				var currentSquare = presentedSquareIndex;
				userIsAwayFromThisPlace = true;
				Frame.Navigate(typeof(WorkoutEditor),WorkoutManager.Workouts[currentSquare]);

			}
		}
		private void GotoActualWorkout() {
			if(!squaresCentered && !importing) {
				var currentSquare = presentedSquareIndex;
				userIsAwayFromThisPlace = true;
				Frame.Navigate(typeof(WorkoutPage),WorkoutManager.Workouts[currentSquare]);

			}
		}
		private void GotoCreation()  {
			if(squaresCentered && !importing) {
				ElementSoundPlayer.Play(ElementSoundKind.Invoke);
				var currentSquare = presentedSquareIndex;
				userIsAwayFromThisPlace = true;
				Frame.Navigate(typeof(WorkoutEditor));


			}
		}
		private void GotoAbout() {
			if(squaresCentered && !importing) {
				ElementSoundPlayer.Play(ElementSoundKind.Invoke);
				Frame.Navigate(typeof(AboutPage));

			}
		}
		private bool importing = false;
		private async void GotoImport() {
			if(squaresCentered && !importing) {
				importing = true;
				ElementSoundPlayer.Play(ElementSoundKind.Invoke);
				var picker = new FileOpenPicker();
				picker.ViewMode = PickerViewMode.Thumbnail;
				picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
				picker.FileTypeFilter.Add(".kwiw");
				var file = await picker.PickSingleFileAsync();
				if(file != null) {
					var fileResult = await WorkoutManager.AddWorkout(file);
					if(fileResult == false) {
						MessageDialog messageDialog = new MessageDialog("Workout couldn't be imported! SAD SAD SAD SAADDDDD SAD") {
							DefaultCommandIndex = 0,
							CancelCommandIndex = 0
						};
						messageDialog.Commands.Add(new UICommand("Okay :("));
						await messageDialog.ShowAsync();
					} else {
						var newWorkout = WorkoutManager.Workouts.Last();
						AddedANewDamnWorkoutToTheMix(newWorkout);
						PostImportSelect(newWorkout,true);
					}
				} else {
					ElementSoundPlayer.Play(ElementSoundKind.Hide);
				}

				importing = false;
			}
		}

		private void PostImportSelect(Workout workout,bool playSound) {
			presentedSquareIndex = WorkoutManager.Workouts.IndexOf(workout);
			selectedIndex = presentedSquareIndex + interfaceSquaresCount;
			selectedGrid = squaresGrid.Children[selectedIndex] as Grid;
			UpdateScroll();
			PresentSquare(workout.GetPresentationGrid(),playSound);
		}

		private bool deleting = false;
		private async void DeletionPrompt() {
			if(deleting)
				return;
			if(!squaresCentered) {
				deleting = true;
				setButtonsEnabled(false);
				MessageDialog messageDialog = new MessageDialog(
					"Are you sure you want to delete this? There's an old adage that goes \"once you do this you can't undo it\"") {
					DefaultCommandIndex = 0,
					CancelCommandIndex = 1,
				};
				messageDialog.Commands.Add(new UICommand("Yes. Leave me alone!",async (command) => {
					await WorkoutManager.DeleteWorkout(presentedSquareIndex);
					DeleteAWorkoutRestInPeace(presentedSquareIndex);
					ClearPresentSquare(playSound: false);
					deleting = false;
				}));
				messageDialog.Commands.Add(new UICommand("No! Please, Daddy don't delete it!",(command) => {
					deleting = false;
					setButtonsEnabled(true);
					FocusManager.TryMoveFocus(FocusNavigationDirection.Previous);

				}));

				await messageDialog.ShowAsync();
			}
		}
		private void ExportButton_Click(object sender,RoutedEventArgs e) {
			GotoExport();
		}
		private void EditButton_Click(object sender,RoutedEventArgs e) {
			GotoEditor();
		}
		private void StartButton_Click(object sender,RoutedEventArgs e) {
			GotoActualWorkout();
		}
		private void DeleteButton_Click(object sender,RoutedEventArgs e) {
			DeletionPrompt();
		}
	}
}
