﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace KeepWithIt {
	internal sealed class Workout {

		internal sealed class Segment {

			internal BitmapImage PreviewImage {
				get;set;
			}

			private int seconds = -1;
			private int repetitions = -1;

		}

		internal string Name {
			get;set;
		}

		internal readonly List<Segment> Segments = new List<Segment>();

		internal readonly List<DateTime> Dates = new List<DateTime>();

		private Grid generate81SquareGrid() {
			Grid grid = new Grid();

			for(int i = 0;i < 9;i++) {
				grid.ColumnDefinitions.Add(new ColumnDefinition());
				grid.RowDefinitions.Add(new RowDefinition());
			}

			return grid;
			
		}

		internal Grid GetGrid() {
			var grid = generate81SquareGrid();

			grid.Background = Application.Current.Resources["SystemBaseHighColor"] as SolidColorBrush;

			if(Segments.Count > 0) {
				for(var i = 0;i<9;i++) {

					int row = i / 3; // 0 - 2
					int column = i % 3;// 0 - 2

					Image image = new Image();

					image.Source = Segments[i % Segments.Count].PreviewImage;
					image.Stretch = Stretch.Fill;
					grid.Children.Add(image);

					Grid.SetColumn(image,column * 3); //0, 3, 5
					Grid.SetColumnSpan(image,3);
					Grid.SetRow(image,row * 3); //0, 3, 5
					Grid.SetRowSpan(image,3);


				}
			}

			var titleGrid = new Grid();
			Grid.SetColumnSpan(titleGrid,9);
			Grid.SetRowSpan(titleGrid,9);
			titleGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
			titleGrid.VerticalAlignment = VerticalAlignment.Top;
			titleGrid.Background = new AcrylicBrush() {
				BackgroundSource = AcrylicBackgroundSource.Backdrop,
				TintColor = Colors.Black,
				FallbackColor = Color.FromArgb(180,0,0,0),
				TintOpacity = 0.25
			};

			TextBlock titleBlock = new TextBlock();
			titleBlock.VerticalAlignment = VerticalAlignment.Top;
			titleBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
			titleBlock.TextAlignment = TextAlignment.Center;
			titleBlock.TextWrapping = TextWrapping.Wrap;

			titleBlock.Padding = new Thickness(2.5,2.5,2.5,2.5);
			titleBlock.FontSize = 18f;
			titleBlock.Text = Name;
			titleBlock.Foreground = new SolidColorBrush(Colors.White);

			titleGrid.Children.Add(titleBlock);
			grid.Children.Add(titleGrid);

			return grid;

		}

		internal Grid GetPresentationGrid() {
			//Todo: Not piggy back off of the other preview grid like a little bitch
			return GetGrid();
		}



	}
}
