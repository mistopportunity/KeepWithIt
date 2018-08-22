using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace KeepWithIt {
	internal static class WorkoutManager {
		internal static string ProcessWorkoutName(string name) {
			if(name == null)
				return null;
			return name.Replace("\n"," ");
		}
		internal static string ProcessSegmentName(string name) {
			if(name == null)
				return null;
			return name.Replace("\n"," ");
		}

		internal static readonly List<Workout> Workouts = new List<Workout>();

		private async static Task<SoftwareBitmap> GetBitmapFromBase64(string data) {

				var bytes = Convert.FromBase64String(data);

				using(var randomAccessStream = new InMemoryRandomAccessStream()) {
					await randomAccessStream.WriteAsync(bytes.AsBuffer());
					var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
					var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

					return ProcessIncomingBitmap(softwareBitmap);

				}

		}
		private async static Task<string> GetBase64OfBitmap(SoftwareBitmap sourceImage) {

			byte[] bytes = new byte[0];
			using(var randomAccessStream = new InMemoryRandomAccessStream()) {
				var encoder = await BitmapEncoder.CreateAsync(
					BitmapEncoder.PngEncoderId,
					randomAccessStream
				);
				SoftwareBitmap bitmap = new SoftwareBitmap(
					sourceImage.BitmapPixelFormat,
					sourceImage.PixelWidth,
					sourceImage.PixelHeight,
					sourceImage.BitmapAlphaMode
				);

				sourceImage.CopyTo(bitmap);

				encoder.SetSoftwareBitmap(bitmap);
				await encoder.FlushAsync();
				bytes = new byte[randomAccessStream.Size];
				await randomAccessStream.ReadAsync(
					bytes.AsBuffer(),
					(uint)bytes.Length,
					InputStreamOptions.None
				);
			}
			var base64String = Convert.ToBase64String(bytes);
			return base64String;


		}
		internal async static Task<SoftwareBitmap> GetBitMapFromFile(StorageFile file) {
			SoftwareBitmap softwareBitmap;

				using(var stream = await file.OpenAsync(FileAccessMode.Read)) {
					BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

					softwareBitmap = await decoder.GetSoftwareBitmapAsync();

					softwareBitmap = ProcessIncomingBitmap(softwareBitmap);
				}

				return softwareBitmap;

		}
		private static SoftwareBitmap ProcessIncomingBitmap(SoftwareBitmap bitmap) {
			if(bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || bitmap.BitmapAlphaMode == BitmapAlphaMode.Straight) {
				bitmap = SoftwareBitmap.Convert(
					bitmap,
					BitmapPixelFormat.Bgra8,
					BitmapAlphaMode.Premultiplied
				);
			}
			return bitmap;
		}

		private async static Task<string> GetWorkoutStringData(Workout workout) {
			var lines = new List<string>();

			lines.Add(workout.Name);

			foreach(var date in workout.Dates) {
				lines.Add(date.ToBinary().ToString());
			}

			lines.Add("END DATES");

			foreach(var segment in workout.Segments) {

				lines.Add(segment.Name.ToString());
				lines.Add(segment.Reps.ToString());
				lines.Add(segment.Seconds.ToString());
				lines.Add(segment.DoubleSided.ToString());

				if(segment.GetImage() == null) {
					lines.Add(string.Empty);
					continue;
				}
				var imageString = await GetBase64OfBitmap(segment.GetImage());
				if(imageString == null) {
					lines.Add(string.Empty);
				} else {
					lines.Add(imageString);
				}

			}

			return string.Join("\n",lines);
		}
		private async static Task<Workout> GetWorkoutFromData(string data) {
			var lines = data.Split('\n');

			if(lines.Length < 1) {
				return null;
			}

			Workout workout = new Workout();

			workout.Name = lines[0];


			var finishedDates = false;

			var startIndex = 1;
			while(!finishedDates) {
				if(lines[startIndex] == "END DATES") {
					finishedDates = true;
				} else {
					try {
						var date = long.Parse(lines[startIndex]);
						var dateTime = new DateTime(date);
						workout.AddDate(dateTime);
					} catch { }
				}
				startIndex+=1;
			}

			for(int i = startIndex;i<lines.Length;i+=5) {

				var segment = new WorkoutSegment();
				try {

					segment.Name = lines[i];
					segment.Reps = int.Parse(lines[i+1]);
					segment.Seconds = int.Parse(lines[i+2]);
					segment.DoubleSided = bool.Parse(lines[i+3]);

					var line5 = lines[i+4];

					if(!string.IsNullOrEmpty(line5)) {
						var bitmap = await GetBitmapFromBase64(line5);
						if(bitmap != null) {
							segment.SetImage(bitmap);
						}
					}

				} catch {
					continue;
				}
				workout.Segments.Add(segment);
			}

			return workout;
		}

		internal async static Task<bool> AddWorkout(StorageFile file) {
			string text;
			try {
				text = await FileIO.ReadTextAsync(file);
			} catch {
				return false;
			}
			if(string.IsNullOrEmpty(text)) {
				return false;
			}

			var workoutFromData =
				await GetWorkoutFromData(text);

			if(workoutFromData == null) {
				return false;
			} else {
				Workouts.Add(workoutFromData);

				//re-encode to save
				SaveWorkout(workoutFromData).Start();

				return true;
			}

		}

		internal async static Task DeleteWorkout(int workoutIndex) {
			var workout = Workouts[workoutIndex];
			Workouts.RemoveAt(workoutIndex);
			if(workout.FileName != null) {
				StorageFolder localFolder = ApplicationData.Current.LocalFolder;
				var file = await localFolder.TryGetItemAsync(workout.FileName);
				if(file != null) {
					await file.DeleteAsync();
				}
			}
		}

		internal async static Task SaveWorkout(Workout workout) {
			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			if(workout.FileName == null) {
				var files = await localFolder.GetFilesAsync();
				var highestIndex = 0;
				foreach(var file in files) {
					var nameSplit = file.Name.Split('_');
					if(nameSplit.Length == 2) {
						if(int.TryParse(nameSplit[1],out int result)) {
							highestIndex = result;
						}
					}
				}
				workout.FileName = $"workout_{highestIndex+1}";
			}
			StorageFile saveFile;
			try {
				saveFile = await localFolder.CreateFileAsync(
					workout.FileName,CreationCollisionOption.ReplaceExisting
				);
			} catch {
				return;
			}
			await ExportWorkout(saveFile,workout);
		}
		internal async static Task<bool> ExportWorkout(StorageFile file,Workout workout) {
			var workoutData = await GetWorkoutStringData(workout);
			if(workoutData == null) {
				return false;
			}
			try {
				await FileIO.WriteTextAsync(file,workoutData);
			} catch {
				return false;
			}
			return true;
		}
		internal async static Task LoadWorkouts() {
			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			var files = await localFolder.GetFilesAsync();
			foreach(var file in files) {
				if(!file.Name.StartsWith("workout") || file.Name.Split('.').Length != 1) {
					await file.DeleteAsync();
					return;
				}
				string output = null;
				try {
					output = await FileIO.ReadTextAsync(file);
				} catch {
					continue;
				}
				if(!string.IsNullOrWhiteSpace(output)) {
					var workout = await GetWorkoutFromData(output);
					if(workout != null) {
						workout.FileName = file.Name;
						Workouts.Add(workout);
					}
				}
			}
		}
	}
}
