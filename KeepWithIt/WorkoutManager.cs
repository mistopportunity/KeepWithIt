using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace KeepWithIt {
	internal static class WorkoutManager {
		internal static readonly List<Workout> Workouts = new List<Workout>();
		internal static void DeleteWorkout(int workoutIndex) {
			Workouts.RemoveAt(workoutIndex);
			SaveWorkouts();
		}

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

		private const int MaxImageDimension = 1024;

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


		internal static void AddWorkout(Workout workout) {
			Workouts.Add(workout);
		}

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

		private static async Task<string> GetWorkoutStringData(Workout workout) {
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
			var lines = data.Split("\n");

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

			var text = await FileIO.ReadTextAsync(file);
			if(string.IsNullOrEmpty(text)) {
				return false;
			}

			var workoutFromData = await GetWorkoutFromData(text);
			if(workoutFromData == null) {
				return false;
			} else {
				Workouts.Add(workoutFromData);
				return true;
			}


		}

		internal async static Task<bool> ExportWorkout(StorageFile file,Workout workout) {
			var workoutData = await GetWorkoutStringData(workout);
			if(workoutData == null) {
				return false;
			}
			var x = file.Path;
			;
			await FileIO.WriteTextAsync(file,workoutData);
			return true;
		}

		internal async static Task LoadWorkouts() {
			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			var filesList = await localFolder.GetFilesAsync();

			//sort the files if out of order or unexpected order becomes an annoyance

			foreach(var file in filesList) {
				await AddWorkout(file);
			}
		}

		internal async static Task SaveWorkouts() {
			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			var filesList = await localFolder.GetFilesAsync();
			//preparing for overflow file deletion
			var filesNamesList = new Dictionary<int,StorageFile>();
			foreach(var file in filesList) {
				filesNamesList.Add(int.Parse(file.Name),file);
			}
			if(Workouts.Count != 0) {
				for(int i = 0;i<Workouts.Count;i++) {
					filesNamesList.Remove(i);
					var workout = Workouts[i];
					var file = await localFolder.CreateFileAsync(i.ToString(),CreationCollisionOption.ReplaceExisting);
					var passed = await ExportWorkout(file,workout);
					//remove this to not override working files that may be deleted because the new components are broken
					if(!passed) {
						await file.DeleteAsync();
					}

				}
			}
			//deleting overflowing files if the new saved data is smaller than the previous
			foreach(var remainingFile in filesNamesList) {
				await remainingFile.Value.DeleteAsync();
			}

		}
	}
}
