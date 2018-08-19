using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
namespace KeepWithIt {

	//Todo: Remove prototype workouts

	internal static class WorkoutManager {
		internal static readonly List<Workout> Workouts = new List<Workout>();
		internal static void DeleteWorkout(int workoutIndex) {
			Workouts.RemoveAt(workoutIndex);
			SaveWorkouts();
		}

		internal static void AddWorkout(Workout workout) {
			Workouts.Add(workout);
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

					var buffer = dataReader.ReadBuffer((uint)size);
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

			for(int i = 0;i<Workouts.Count;i++) {

				var workout = Workouts[i];
				var file = await localFolder.CreateFileAsync(i.ToString(),CreationCollisionOption.ReplaceExisting);

				await ExportWorkout(file,workout);

			}

		}

		private static void LoadPrototypeWorkouts() {

			var path = "ms-appx:///Assets/Kitten.png";
			Uri uri = new Uri(path,UriKind.RelativeOrAbsolute);
			var debugImage = new BitmapImage(uri);

			var workout1 = new Workout() {
				Name = "Debug workout 1",
			};
			workout1.Dates.Add(DateTime.Today - TimeSpan.FromDays(54));

			workout1.Segments.Add(new WorkoutSegment() {
				PreviewImage = debugImage,
				Name = "Segment 1 - reps, secs",
				Reps = 5,
				Seconds = 2,
			});
			workout1.Segments.Add(new WorkoutSegment() {
				PreviewImage = debugImage,
				Name = "Segment 2 - no reps, secs",
				Reps = 0,
				Seconds = 2,
			});
			workout1.Segments.Add(new WorkoutSegment() {
				PreviewImage = debugImage,
				Name = "Segment 3 - reps, no secs",
				Reps = 69,
				Seconds = 0,
			});
			workout1.Segments.Add(new WorkoutSegment() {
				PreviewImage = debugImage,
				Name = "Segment 4 - no reps, no secs",
				Reps = 0,
				Seconds = 0,
			});

			AddWorkout(workout1);

		}

	}
}
