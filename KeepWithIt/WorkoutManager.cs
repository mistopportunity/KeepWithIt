using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace KeepWithIt {

	//Todo: Remove prototype workouts

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
				//error handling needed all over the place
				return softwareBitmap;

			}


		}

		private async static Task<string> GetBase64OfBitmap(SoftwareBitmap sourceImage) {
			byte[] bytes = new byte[0];
			using(var randomAccessStream = new InMemoryRandomAccessStream()) {
				var encoder = await BitmapEncoder.CreateAsync(
					BitmapEncoder.JpegEncoderId,
					randomAccessStream
				);
				encoder.SetSoftwareBitmap(sourceImage);
				try {
					await encoder.FlushAsync();
				} catch {
					return null;
				}

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
				ProcessIncomingBitmap(softwareBitmap);
			}
			return softwareBitmap.ProcessIncomingBitmap();
		}

		private static SoftwareBitmap ProcessIncomingBitmap(this SoftwareBitmap bitmap) {
			if(bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
				bitmap.BitmapAlphaMode == BitmapAlphaMode.Straight) {
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
			return name.Replace(Environment.NewLine," ");
		}

		internal static string ProcessSegmentName(string name) {
			return name.Replace(Environment.NewLine," ");
		}

		private static string GetWorkoutStringData(Workout workout) {
			//todo - convert a workout object to string, returning null if failing - use crytpographic namespace to base64 bitmaps
			return null;
		}

		private static Workout GetWorkoutFromData(string data) {
			//todo - convert a string to workout object, returning null if failing - use crytpographic namespace to decode base64 bitmaps
			return null;
		}

		internal async static Task<bool> AddWorkout(StorageFile file) {

			var stream = await file.OpenAsync(FileAccessMode.Read);
			ulong size = stream.Size;

			if(size == 0) {
				return false;
			}

			//might just cheap out and do an overall try catch atrocity

			using(var inputStream = stream.GetInputStreamAt(0)) {

				using(var dataReader = new DataReader(inputStream)) {

					var length = (uint)size;

					await dataReader.LoadAsync(length);

					var buffer = dataReader.ReadBuffer(length);
					var text = CryptographicBuffer.ConvertBinaryToString(
						BinaryStringEncoding.Utf8,
						buffer
					);

					var workoutFromData = GetWorkoutFromData(text);
					if(workoutFromData == null) {
						return false;
					} else {
						Workouts.Add(workoutFromData);
						return true;
					}

				}

			}

		}

		internal async static Task<bool> ExportWorkout(StorageFile file,Workout workout) {
			var workoutData = GetWorkoutStringData(workout);
			if(workoutData == null) {
				return false;
			}

			var buffer = CryptographicBuffer.ConvertStringToBinary(
				workoutData,
				BinaryStringEncoding.Utf8
			);

			await FileIO.WriteBufferAsync(file,buffer);
			return true;
		}

		internal async static void LoadWorkouts() {

			StorageFolder localFolder = ApplicationData.Current.LocalFolder;

			var filesList = await localFolder.GetFilesAsync();

			//sort the files if out of order or unexpected order becomes an annoyance

			foreach(var file in filesList) {

				await AddWorkout(file);

			}


#if DEBUG
			if(Workouts.Count == 0)
			LoadPrototypeWorkouts();
#endif
		}

		internal async static void SaveWorkouts() {

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

		private static void LoadPrototypeWorkouts() {

			var path = "ms-appx:///Assets/Kitten.png";
			Uri uri = new Uri(path,UriKind.RelativeOrAbsolute);





			var workout1 = new Workout() {
				Name = "Debug workout 1",
			};
			workout1.Dates.Add(DateTime.Today - TimeSpan.FromDays(54));

			workout1.Segments.Add(new WorkoutSegment() {
				Name = "Segment 1 - reps, secs",
				Reps = 5,
				Seconds = 2,
			});


			workout1.Segments.Add(new WorkoutSegment() {

				Name = "Segment 2 - no reps, secs",
				Reps = 0,
				Seconds = 2,
			});
			workout1.Segments.Add(new WorkoutSegment() {

				Name = "Segment 3 - reps, no secs",
				Reps = 69,
				Seconds = 0,
			});
			workout1.Segments.Add(new WorkoutSegment() {

				Name = "Segment 4 - no reps, no secs",
				Reps = 0,
				Seconds = 0,
			});

			AddWorkout(workout1);

		}

	}
}
