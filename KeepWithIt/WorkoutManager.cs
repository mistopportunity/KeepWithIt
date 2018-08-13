using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using System.Threading.Tasks;

namespace KeepWithIt {
	internal static class WorkoutManager {


		internal static List<Workout> Workouts;

		internal static void DeleteWorkout(int workoutIndex) {
			Workouts.RemoveAt(workoutIndex);
		}

		internal static void AddWorkout(Workout workout) {
			Workouts.Add(workout);
		}

		internal static bool WorkoutsLoaded {
			get {
				return Workouts != null;
			}
		}
		internal static void LoadWorkouts() {

			Workouts = new List<Workout>();

#if DEBUG
			LoadPrototypeWorkouts();
			return;
#endif

			//Populate list from wherever the Hell

		}

		private static void LoadPrototypeWorkouts() {

			var path = "ms-appx:///Assets/Kitten.png";
			Uri uri = new Uri(path,UriKind.RelativeOrAbsolute);
			var debugImage = new BitmapImage(uri);

			var workout1 = new Workout() {
				Name = "Debug workout 1"
			};
			workout1.Segments.Add(new Workout.Segment() {
				PreviewImage = debugImage
			});

			AddWorkout(workout1);

		}

	}
}
