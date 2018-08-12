using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepWithIt {
	internal static class WorkoutManager {


		internal static List<Workout> Workouts;


		internal static bool WorkoutsLoaded {
			get {
				return Workouts != null;
			}
		}
		internal static void LoadWorkouts() {

			Workouts = new List<Workout>();

			//Populate list from wherever the Hell

		}

	}
}
