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
using Windows.UI.Xaml.Navigation;
using Windows.System;
using Windows.UI.Popups;

namespace KeepWithIt {
	public sealed partial class MainPage:Page {

		public MainPage() {
			this.InitializeComponent();
		}

		private void ReloadSquares() {
			squaresGrid.Children.Clear();

			AddInterfaceSquares();

			//Populate squaresGrid with WorkoutManager.Workouts

			AddPrototypeSquare("First square (index 0)");
			AddPrototypeSquare("Second square (index 1)");
			AddPrototypeSquare("Third square (index 2)");
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

		private void AddPrototypeSquare(string text) {
			AddSquare(
				new List<UIElement>() {
						new TextBlock() {
							Text = text,
							Foreground = new SolidColorBrush(Colors.White),
							FontSize = 18f
						}
					}
			);
		}
		private int interfaceSquaresCount = 0;
		private void AddInterfaceSquares() {
			AddPrototypeSquare("Create");
			AddPrototypeSquare("Import");
			AddPrototypeSquare("About");
			interfaceSquaresCount = 3;
		}

		private Grid PresentationSquare {
			get {
				var grid = new Grid() {
					Background = new SolidColorBrush(Colors.Black)
				};

				//use presented square index to access a more root data class

				return grid;
			}
		}

		private Grid presentedSquare = null;

		public void ClearPresentSquare() {
			if(presentedSquare != null) {
				TopGrid.Children.Remove(presentedSquare);
			}
			presentedSquare = null;
			presentedSquareIndex = -1;
			UnsubscribeBackButton();
			CentralizeSquares();
		}

		private int presentedSquareIndex = -1;

		public void PresentSquare(Grid square) {
			if(presentedSquare != null) {
				TopGrid.Children.Remove(presentedSquare);
			}
			presentedSquare = square;
			PushSquaresLeft();
			TopGrid.Children.Add(square);
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
				var presentationSquare = PresentationSquare;

				//use presentedSquareIndex to populate and depopulate calendar

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

			grid.Tapped += (sender,e) => {
				SquareTapped(sender as Grid);
			};

			squaresGrid.Children.Add(grid);
		}

		public void AddSquare(IEnumerable<UIElement> content = null) {

			var grid = new Grid() {
				Background = new SolidColorBrush(Colors.Black)
			};

			var gridChildren = grid.Children;

			if(content != null) {
				foreach(var element in content) {
					gridChildren.Add(element);
				}
			}

			AddSquare(grid);


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
			calendar.Margin = new Thickness(0,145,15,15);
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
			calendar.Margin = new Thickness(0,145,15,0);
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
		private Grid selectedGrid = null;
		private void updateSelection(int xDelta,int yDelta) {
			if(selectedIndex == -1) {

				selectedIndex = 0;
				selectedGrid = squaresGrid.Children[selectedIndex] as Grid;
				addSelectionAttributes(selectedGrid);

			} else {

				var squaresCount = squaresGrid.Children.Count;

				if(xDelta != 0) {
					if(selectedIndex % 2 == 0) {
						if(selectedIndex + 1 < squaresCount) {
							selectedIndex++;
						}
					} else {
						selectedIndex--;
					}
				}

				if(yDelta > 0) {
					if(selectedIndex <= squaresCount - 3) {
						if(squaresCount % 2 != 0) {
							selectedIndex = squaresCount - 1;
						} else {
							selectedIndex += 2;
						}
					}
				} else if(yDelta < 0) {
					if(selectedIndex >= 2) {
						selectedIndex -= 2;
					}

				}

				removeSelectionAttributes(selectedGrid);
				selectedGrid = squaresGrid.Children[selectedIndex] as Grid;
				addSelectionAttributes(selectedGrid);

				selectedGrid.StartBringIntoView();
				//Offset scroller to include margins, eh?

			}
		}

		private void removeSelectionAttributes(Grid grid) {
			grid.Background = new SolidColorBrush(Colors.Black);
		}

		private void addSelectionAttributes(Grid grid) {
			grid.Background = new SolidColorBrush(Colors.Gray);
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
						break;
					case VirtualKey.GamepadDPadLeft:
					case VirtualKey.GamepadDPadUp:
					case VirtualKey.Left:
					case VirtualKey.Up:
						FocusManager.TryMoveFocus(FocusNavigationDirection.Previous);
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
					case VirtualKey.GamepadLeftThumbstickUp:
						updateSelection(0,-1);
						break;
					case VirtualKey.Down:
					case VirtualKey.GamepadDPadDown:
					case VirtualKey.GamepadLeftThumbstickDown:
						updateSelection(0,1);
						break;
					case VirtualKey.Left:
					case VirtualKey.GamepadDPadLeft:
					case VirtualKey.GamepadLeftThumbstickLeft:
						updateSelection(-1,0);
						break;
					case VirtualKey.Right:
					case VirtualKey.GamepadDPadRight:
					case VirtualKey.GamepadLeftThumbstickRight:
						updateSelection(1,0);
						break;
				}
			}
		}

		private void GotoExport() {
			if(!squaresCentered) {
				//use presentedSquareIndex
				//app navigation
			}
		}

		private void GotoEditor() {
			if(!squaresCentered) {
				//todo
				//use presentedSquareIndex
				//app navigation
			}
		}

		private void GotoActualWorkout() {
			if(!squaresCentered) {
				//use presentedSquareIndex
				//app navigation
			}
		}

		private void GotoCreation() {
			if(squaresCentered) {
				//app navigation
			}
		}

		private void GotoAbout() {
			if(squaresCentered) {
				//app navigation
			}
		}

		private void GotoImport() {
			if(squaresCentered) {
				//app navigation
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
	}
}
