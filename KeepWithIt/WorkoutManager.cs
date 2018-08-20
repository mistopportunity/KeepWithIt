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
	internal static class WorkoutManager {
		internal static readonly List<Workout> Workouts = new List<Workout>();
		internal static void DeleteWorkout(int workoutIndex) {
			Workouts.RemoveAt(workoutIndex);
			SaveWorkouts();
		}

		private async static Task<SoftwareBitmap> GetBitmapFromBase64(string data) {
			try {
				var bytes = Convert.FromBase64String(data);

				using(var randomAccessStream = new InMemoryRandomAccessStream()) {
					await randomAccessStream.WriteAsync(bytes.AsBuffer());
					var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
					var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
					return softwareBitmap;

				}
			} catch {
				return null;
			}
		}

		private async static Task<string> GetBase64OfBitmap(SoftwareBitmap sourceImage) {
			try {
				byte[] bytes = new byte[0];
				using(var randomAccessStream = new InMemoryRandomAccessStream()) {
					var encoder = await BitmapEncoder.CreateAsync(
						BitmapEncoder.JpegEncoderId,
						randomAccessStream
					);

					encoder.SetSoftwareBitmap(sourceImage);

					;
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
			} catch {
				return null;
			}

		}
		internal async static Task<SoftwareBitmap> GetBitMapFromFile(StorageFile file) {
			SoftwareBitmap softwareBitmap;
			try {
				using(var stream = await file.OpenAsync(FileAccessMode.Read)) {
					BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

					softwareBitmap = await decoder.GetSoftwareBitmapAsync();

					softwareBitmap = await ProcessIncomingBitmap(softwareBitmap);
				}

				return softwareBitmap;
			} catch {
				return null;
			}
		}

		private const int MaxImageDimension = 1024;

		private async static Task<SoftwareBitmap> ProcessIncomingBitmap(SoftwareBitmap bitmap) {
			if(bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || bitmap.BitmapAlphaMode == BitmapAlphaMode.Straight) {
				bitmap = SoftwareBitmap.Convert(
					bitmap,
					BitmapPixelFormat.Bgra8,
					BitmapAlphaMode.Premultiplied
				);
			}


			uint newWidth;
			uint newHeight;
			if(bitmap.PixelWidth > MaxImageDimension) {
				newWidth = MaxImageDimension;
				newHeight = (uint)Math.Floor(
					(MaxImageDimension / (double)bitmap.PixelWidth) * bitmap.PixelHeight
				);
			} else if(bitmap.PixelHeight > MaxImageDimension) {
				newHeight = MaxImageDimension;
				newWidth = (uint)Math.Floor(
					(MaxImageDimension / (double)bitmap.PixelHeight) * bitmap.PixelWidth
				);
			} else {
				return bitmap;
			}

			var bitmapTransform = new BitmapTransform() {
				ScaledWidth = newWidth,
				ScaledHeight = newHeight,
				InterpolationMode = BitmapInterpolationMode.Cubic
			};


			using(var randomAccessStream = new InMemoryRandomAccessStream()) {
				var encoder = await BitmapEncoder.CreateAsync(
					BitmapEncoder.JpegEncoderId,
					randomAccessStream
				);
				encoder.SetSoftwareBitmap(bitmap);

				try {
					await encoder.FlushAsync();
				} catch {
					return null;
				}


				var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);


				var pixelData = await decoder.GetPixelDataAsync(
					BitmapPixelFormat.Bgra8,
					BitmapAlphaMode.Premultiplied,
					bitmapTransform,ExifOrientationMode.IgnoreExifOrientation,
					ColorManagementMode.ColorManageToSRgb
				);

				var pixels = pixelData.DetachPixelData();

				var writeableBitmap = new WriteableBitmap(
					(int)decoder.PixelWidth,
					(int)decoder.PixelHeight
				);

				await writeableBitmap.SetSourceAsync(randomAccessStream);
				var newBitmap = SoftwareBitmap.CreateCopyFromBuffer(
					writeableBitmap.PixelBuffer,
					BitmapPixelFormat.Bgra8,
					(int)newWidth,
					(int)newHeight,
					BitmapAlphaMode.Premultiplied
				);

				return newBitmap;

			}



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

			foreach(var segment in workout.Segments) {

				lines.Add(segment.Name.ToString());
				lines.Add(segment.Reps.ToString());
				lines.Add(segment.Seconds.ToString());
				lines.Add(segment.DoubleSided.ToString());

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

			if(lines.Length < 6) {
				return null;
			}

			for(int i = 1;i<lines.Length;i+=5) {
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
			try {
				var stream = await file.OpenAsync(FileAccessMode.Read);
				ulong size = stream.Size;

				if(size == 0) {
					return false;
				}

				using(var inputStream = stream.GetInputStreamAt(0)) {

					using(var dataReader = new DataReader(inputStream)) {

						var length = (uint)size;

						await dataReader.LoadAsync(length);

						var buffer = dataReader.ReadBuffer(length);
						var text = CryptographicBuffer.ConvertBinaryToString(
							BinaryStringEncoding.Utf8,
							buffer
						);

						var workoutFromData = await GetWorkoutFromData(text);
						if(workoutFromData == null) {
							return false;
						} else {
							Workouts.Add(workoutFromData);
							return true;
						}

					}

				}
			} catch {
				return false;
			}
		}

		internal async static Task<bool> ExportWorkout(StorageFile file,Workout workout) {
			var workoutData = await GetWorkoutStringData(workout);
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
	}
}
