using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace KeepWithIt {
	public sealed partial class MainPage:Page {

		public MainPage() {
			this.InitializeComponent();
			AddSquare();
			AddSquare();
			AddSquare();
			AddSquare();
			AddSquare();
			AddSquare();



#if DEBUG
			var bigSquare = GetPresentationSquare(
				squareElements[0]
			);

			PresentSquare(bigSquare);

			Thread.Sleep(1000);

			ClearPresentSquare();

#endif


		}

		private List<Grid> squareElements = new List<Grid>();


		private Grid GetPresentationSquare(Grid square) {

			//todo once we aren't using grids - grids are for prototyping
			var grid = new Grid() {
				Background = new SolidColorBrush(Colors.DarkSlateGray),
			};

			return grid;

		}

		private Grid presentedSquare = null;

		public void ClearPresentSquare() {
			if(presentedSquare != null) {
				TopGrid.Children.Remove(presentedSquare);
			}
			presentedSquare = null;
			CentralizeSquares();
		}

		public void PresentSquare(Grid square) {
			if(presentedSquare != null) {
				TopGrid.Children.Remove(presentedSquare);
			}
			presentedSquare = square;
			PushSquaresLeft();
			TopGrid.Children.Add(square);
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

			if(squareElements.Count % 2 == 0) {
				grid.HorizontalAlignment = HorizontalAlignment.Right;
				grid.SetValue(Grid.ColumnProperty,1);
			} else {
				grid.HorizontalAlignment = HorizontalAlignment.Left;
				grid.SetValue(Grid.ColumnProperty,3);
			}

			squareElements.Add(grid);
			squaresGrid.Children.Add(grid);

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

			var squareCount = squareElements.Count;

			if(--squareCount == -1) {
				return;
			}

			var marginSize = CentralSubColumn.ActualWidth;


			var leftSize = ContentColumn1.ActualWidth;
			var rightSize = ContentColumn2.ActualWidth;

			void layoutSquare(int index,bool last) {
				var square = squareElements[index];

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

		private void Page_LayoutUpdated(object sender,object e) {

			if(squaresCentered) {
				GridLength columnSize;
				if(ActualWidth < ActualHeight) {
					columnSize = new GridLength(0.75,GridUnitType.Star);
				} else {
					columnSize = new GridLength(12,GridUnitType.Star);
				}
				SubColumnFirst.Width = columnSize;
				SubColumnLast.Width = columnSize;
				layoutSquares();
			} else {
				double actualActualWidth;
				if(Column1.ActualWidth > TopGrid.ActualHeight) {
					actualActualWidth = TopGrid.ActualHeight - (Column1.ActualWidth * 0.05);
				} else {
					actualActualWidth = Column1.ActualWidth * 0.95;
				}
				presentedSquare.Width = actualActualWidth;
				presentedSquare.Height = actualActualWidth;
			}
		
		}
	}
}
