using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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

namespace KeepWithIt {
	public sealed partial class MainPage:Page {

		private const double focusSizeDivider = 50;
		private double iconSizeMultiplier = 2;

		public MainPage() {
			this.InitializeComponent();
		}

		private void ReloadSquares() {

			if(selectedIndex != -1) {
				removeSelectionAttributes(selectedGrid);
				selectedGrid = null;
				selectedIndex = -1;
			}
			lastSelectedIndex = 0;

			squaresGrid.Children.Clear();

			AddInterfaceSquares();

			foreach(var workout in WorkoutManager.Workouts) {
				var workoutGrid = workout.GetGrid();
				AddSquare(workoutGrid);
			}

		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);

			ReloadSquares();

			Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

		}
		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {

			base.OnNavigatingFrom(e);

			Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;

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
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Center,
				FontSize = 20,
				TextAlignment = TextAlignment.Center,
				//What the fuck is the difference????
				HorizontalTextAlignment = TextAlignment.Center
			};

			textBlock.Foreground = new SolidColorBrush(Colors.Black);

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

		public void ClearPresentSquare() {
			if(presentedSquare != null) {
				TopGrid.Children.Remove(presentedSquare);
			}
			presentedSquare = null;
			presentedSquareIndex = -1;
			UnsubscribeBackButton();

			setButtonsEnabled(false);

			CentralizeSquares();

			ElementSoundPlayer.Play(ElementSoundKind.MovePrevious);
		}

		private int presentedSquareIndex = -1;

		public void PresentSquare(Grid square) {
			if(presentedSquare != null) {
				TopGrid.Children.Remove(presentedSquare);
			}

			setButtonsEnabled(true);

			presentedSquare = square;
			PushSquaresLeft();
			TopGrid.Children.Add(square);

			ElementSoundPlayer.Play(ElementSoundKind.MoveNext);
		}

		private void SquareTapped(Grid grid) {

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

				foreach(var date in WorkoutManager.Workouts[presentedSquareIndex].Dates) {
					//Todo: populate and depopulate calendar
				}


				PresentSquare(presentationSquare);

				var currentView = SystemNavigationManager.GetForCurrentView();
				currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
				currentView.BackRequested +=CurrentView_BackRequested;
			}


		}

		private void UnsubscribeBackButton() {
			var currentView = SystemNavigationManager.GetForCurrentView();
			currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			currentView.BackRequested -= CurrentView_BackRequested;
		}

		private void CurrentView_BackRequested(object sender,BackRequestedEventArgs e) {
			ClearPresentSquare();
			e.Handled = true;
		}
		public void AddSquare(Grid grid) {
			if(squaresGrid.Children.Count % 2 == 0) {
				grid.HorizontalAlignment = HorizontalAlignment.Right;
				Grid.SetColumn(grid,1);
			} else {
				grid.HorizontalAlignment = HorizontalAlignment.Left;
				Grid.SetColumn(grid,3);
			}

			grid.Tapped += Grid_Tapped;

			grid.PointerPressed += pointerBasedGridSelect;
			grid.PointerEntered += pointerBasedGridSelect;

			grid.PointerExited += Grid_PointerExited;

			squaresGrid.Children.Add(grid);
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
					selectedGrid.StartBringIntoView();
					lastSelectedIndex = selectedIndex;
					ElementSoundPlayer.Play(ElementSoundKind.Focus);
				}
			}
		}

		private void removeSelectionAttributes(Grid grid) {

			grid.BorderThickness = new Thickness(0,0,0,0);

		}

		private void updateSelectedGrid() {
			if(selectedGrid != null) {
				addSelectionAttributes(selectedGrid);
			}
		}

		private void addSelectionAttributes(Grid grid) {
			var thiccness = -(grid.ActualWidth / focusSizeDivider);
			grid.BorderThickness = new Thickness(thiccness); //Oh God, the irony.

		}

		private void CoreWindow_KeyDown(CoreWindow sender,KeyEventArgs args) {
			if(!squaresCentered) {
				switch(args.VirtualKey) {
					case VirtualKey.Back:
					case VirtualKey.GoBack:
					case VirtualKey.Escape:
					case VirtualKey.GamepadB:
						ClearPresentSquare();
						break;
					case VirtualKey.GamepadDPadDown:
					case VirtualKey.GamepadDPadRight:
					case VirtualKey.Right:
					case VirtualKey.Down:
						FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
						ElementSoundPlayer.Play(ElementSoundKind.Focus);
						break;
					case VirtualKey.GamepadDPadLeft:
					case VirtualKey.GamepadDPadUp:
					case VirtualKey.Left:
					case VirtualKey.Up:
						FocusManager.TryMoveFocus(FocusNavigationDirection.Previous);
						ElementSoundPlayer.Play(ElementSoundKind.Focus);
						break;

				}
			} else {
				switch(args.VirtualKey) {
					case VirtualKey.GamepadA:
					case VirtualKey.Enter:
						if(selectedGrid != null) {
							SquareTapped(selectedGrid);
						}
						break;
					case VirtualKey.Up:
					case VirtualKey.GamepadDPadUp:
						updateSelection(0,-1);
						break;
					case VirtualKey.Down:
					case VirtualKey.GamepadDPadDown:
						updateSelection(0,1);
						break;
					case VirtualKey.Left:
					case VirtualKey.GamepadDPadLeft:
						updateSelection(-1,0);
						break;
					case VirtualKey.Right:
					case VirtualKey.GamepadDPadRight:
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


		private void GotoExport() {
			if(!squaresCentered) {
				//use presentedSquareIndex
				//use a dialog
				ElementSoundPlayer.Play(ElementSoundKind.Show);
			}
		}

		private void GotoEditor() {
			if(!squaresCentered) {
				//Todo: Exporting page
				//use presentedSquareIndex
				//app navigation
				ElementSoundPlayer.Play(ElementSoundKind.MoveNext);
			}
		}

		private void GotoActualWorkout() {
			if(!squaresCentered) {
				//Todo: Workout page
				//use presentedSquareIndex
				//app navigation
				ElementSoundPlayer.Play(ElementSoundKind.MoveNext);
			}
		}

		private void GotoCreation()  {
			if(squaresCentered) {
				//Todo: Creation page
				//app navigation
				ElementSoundPlayer.Play(ElementSoundKind.MoveNext);

			}
		}

		private void GotoAbout() {
			if(squaresCentered) {
				//Todo: About page
				//app navigation
				ElementSoundPlayer.Play(ElementSoundKind.MoveNext);
			}
		}

		private void GotoImport() {
			if(squaresCentered) {
				//Todo: Import page
				//app navigation
				ElementSoundPlayer.Play(ElementSoundKind.MoveNext);
			}
		}

		private bool deleting = false;
		private async void DeletionPrompt() {
			if(deleting)
				return;
			if(!squaresCentered) {
				deleting = true;
				setButtonsEnabled(false);
				MessageDialog messageDialog = new MessageDialog(
					"Are you sure you want to delete this? There's an old adage that goes \"once you do this you can't undo it\"",
					"THIS IS SO SAD") {
					DefaultCommandIndex = 0,
					CancelCommandIndex = 1,
				};
				messageDialog.Commands.Add(new UICommand("Yes. Leave me alone!",(command) => {
					WorkoutManager.DeleteWorkout(presentedSquareIndex);
					ReloadSquares();
					ClearPresentSquare();
					deleting = false;
				}));
				messageDialog.Commands.Add(new UICommand("No! Please, Daddy don't delete it!",(command) => {
					deleting = false;
					setButtonsEnabled(true);
					FocusManager.TryMoveFocus(FocusNavigationDirection.Previous);
				}));
				ElementSoundPlayer.Play(ElementSoundKind.Show);
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
