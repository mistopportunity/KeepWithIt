using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

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
		internal async static Task<bool> AddWorkout(IStorageItem file) {
			//Todo: Process IStorageItem
			Workouts.Add(new Workout() {
				Name = file.Name.Split(".").SkipLast(1).Aggregate((x,y) => $"{x}{y}")
			});
			return true;
		}

		internal async static void ExportWorkout(StorageFile file,Workout workout) {
			//Todo: properly write the file
			await FileIO.WriteTextAsync(file,file.Name);
		}

		internal async static void LoadWorkouts() {

			//Todo load workouts

#if DEBUG
			if(Workouts.Count == 0)
			LoadPrototypeWorkouts();
#endif
		}

		internal async static void SaveWorkouts() {
			//Todo save workouts
			// ' it's "saved" '
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
				Name = "Segment prototyping",
				Reps = 5,
				Seconds = 20,
			});
			workout1.Segments.Add(new WorkoutSegment() {
				PreviewImage = debugImage,
				Name = "Segment prototyping",
				Reps = 5
			});
			workout1.Segments.Add(new WorkoutSegment() {
				PreviewImage = debugImage,
				Name = "Segment prototyping",
				Seconds = 60
			});
			workout1.Segments.Add(new WorkoutSegment() {
				PreviewImage = debugImage,
				Name = "Segment prototyping"
			});

			AddWorkout(workout1);

			AddWorkout(new Workout() {
				Name = "Debug workout 2"
			});

			AddWorkout(new Workout() {
				Name = "Debug workout 3"
			});

			AddWorkout(new Workout() {
				Name = "Debug workout 4"
			});

		}

	}
}
