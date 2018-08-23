using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.Graphics.Imaging;

namespace KeepWithIt {
	internal class WorkoutSegment:INotifyPropertyChanged {

		private SoftwareBitmap image;

		internal async void SetImage(SoftwareBitmap image) {
			this.image = image;
			var source = new SoftwareBitmapSource();
			await source.SetBitmapAsync(image);
			usableImage = source;
			OnPropertyChanged("UsableImage");
		}
		internal SoftwareBitmap GetImage() {
			return image;
		}
		private ImageSource usableImage;
		internal ImageSource UsableImage {
			get {
				return usableImage;
			}
		}

		private string name;
		internal string Name {
			get {
				return name;
			}
			set {
				name = WorkoutManager.ProcessSegmentName(value);
				OnPropertyChanged("Name");
			}
		}

		private int reps = -1;
		private int seconds = -1;

		internal bool DoubleSided { get; set; } = false;

		internal int Reps {
			get {
				return reps;
			}
			set {
				reps = value;
				OnPropertyChanged("ModeDescription");
			}
		}

		internal int Seconds {
			get {
				return seconds;
			}
			set {
				seconds = value;
				OnPropertyChanged("ModeDescription");
			}
		}

		private string secondsString() {
			return $"{seconds} second{((seconds != 1 ? "s" : ""))}";
		}
		private string repsString() {
			return $"{reps} rep{(reps != 1 ? "s" : "")}";
		}

		internal string GetRepDescription() {
			if(reps < 1) {
				return $"no rep count{(DoubleSided ? $", both sides" : string.Empty)}";
			} else {
				return $"{repsString()}{(DoubleSided ? $", both sides" : string.Empty)}";
			}
		}

		internal string ModeDescription {
			get {
				var useReps = reps > 0;
				var useSeconds = seconds > 0;
				if(!useReps && !useSeconds) {
					return "just do it";
				}
				if(useReps && useSeconds) {
					return $"{secondsString()}\n{repsString()}";
				} else if(useReps) {
					return repsString();
				} else {
					return secondsString();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string name) {
			PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(name));
		}
	}
	internal sealed class Workout {
		private string name;
		internal string Name {
			get {
				return name;
			}
			set {
				name = WorkoutManager.ProcessWorkoutName(value);
			}
		}
		internal readonly ObservableCollection<WorkoutSegment> Segments =
			new ObservableCollection<WorkoutSegment>();
		internal readonly List<DateTime> Dates =
			new List<DateTime>();

		internal string FileName = null;

		internal void AddDate(DateTime dateTime) {
			if(Dates.Count < 1) {
				Dates.Add(dateTime);
				return;
			}
			var day = dateTime.Day;
			var month = dateTime.Month;
			var year = dateTime.Year;
			var pastDate = Dates.Last();
			if(year > pastDate.Year) {
				Dates.Add(dateTime);
				return;
			}
			if(month > pastDate.Month) {
				Dates.Add(dateTime);
				return;
			}
			if(day > dateTime.Day) {
				Dates.Add(dateTime);
				return;
			}
		}
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
			var checker2Color = new SolidColorBrush(Color.FromArgb(180,165,165,165));
			var checker1Color = new SolidColorBrush(Color.FromArgb(180,76,76,76));

			for(var i = 0;i<9;i++) {

				int row = i / 3; // 0 - 2
				int column = i % 3;// 0 - 2

				Grid imageGrid = new Grid();

				if(Segments.Count > 0) {
					Image image = new Image();
					var segment = Segments[i % Segments.Count];

					void segmentPropertyUpdater(object sender,PropertyChangedEventArgs e) {
						if(e.PropertyName == "UsableImage") {
							if(image.Source == null) {
								image.Source = segment.UsableImage;
							}
							segment.PropertyChanged -= segmentPropertyUpdater;
						}
					}

					segment.PropertyChanged += segmentPropertyUpdater;

					image.Source = segment.UsableImage;
					image.Stretch = Stretch.UniformToFill;

					image.HorizontalAlignment = HorizontalAlignment.Center;
					image.VerticalAlignment = VerticalAlignment.Center;

					imageGrid.Children.Add(image);
				}

				if(i % 2 == 0) {
					imageGrid.Background = checker1Color;
				} else {
					imageGrid.Background = checker2Color;
				}
				grid.Children.Add(imageGrid);

				Grid.SetColumn(imageGrid,column * 3); //0, 3, 5
				Grid.SetColumnSpan(imageGrid,3);
				Grid.SetRow(imageGrid,row * 3); //0, 3, 5
				Grid.SetRowSpan(imageGrid,3);


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

			titleBlock.FontSize = 18f;
			titleBlock.Text = Name;
			titleBlock.Foreground = new SolidColorBrush(Colors.White);

			titleGrid.Children.Add(titleBlock);
			grid.Children.Add(titleGrid);

			return grid;

		}

		internal Grid GetPresentationGrid() {
			return GetGrid();
		}
	}
}
