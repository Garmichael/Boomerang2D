using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boomerang2DFramework.Framework.Utilities {
	public static class BoomerangUtils {
		/// <summary>
		/// Checks if the value is between the min and max values.
		/// </summary>
		/// <param name="val">The value to check</param>
		/// <param name="min">The minimum acceptable value</param>
		/// <param name="max">The maximum acceptable value</param>
		/// <returns>True if the value is acceptable</returns>
		public static bool ValueIsBetween(float val, float min, float max) {
			return val >= min && val <= max;
		}

		/// <summary>
		/// Clamps the value between the min and max values. If the value is below the min, it is set to the min. If
		/// the value is above the max, it is set to the max.
		/// </summary>
		/// <param name="value">The value to clamp</param>
		/// <param name="min">The minimum acceptable value</param>
		/// <param name="max">The maximum acceptable value</param>
		/// <returns>The clamped value</returns>
		public static float ClampValue(float value, float min, float max) {
			if (value < min) {
				value = min;
			}

			if (value > max) {
				value = max;
			}

			return value;
		}

		/// <summary>
		/// Clamps the value between the min and max values. If the value is below the min, it is set to the min. If
		/// the value is above the max, it is set to the max.
		/// </summary>
		/// <param name="value">The value to clamp</param>
		/// <param name="min">The minimum acceptable value</param>
		/// <param name="max">The maximum acceptable value</param>
		/// <returns>The clamped value</returns>
		public static int ClampValue(int value, int min, int max) {
			return (int) ClampValue(value, min, (float) max);
		}

		/// <summary>
		/// If the value given is less than the min value, it is set to the min value. 
		/// </summary>
		/// <param name="value">The value to clamp</param>
		/// <param name="min">The minimum acceptable value</param>
		/// <returns>The clamped value</returns>
		public static int MinValue(int value, int min) {
			if (value < min) {
				value = min;
			}

			return value;
		}

		/// <summary>
		/// If the value given is less than the min value, it is set to the min value. 
		/// </summary>
		/// <param name="value">The value to clamp</param>
		/// <param name="min">The minimum acceptable value</param>
		/// <returns>The clamped value</returns>
		public static float MinValue(float value, float min) {
			if (value < min) {
				value = min;
			}

			return value;
		}

		/// <summary>
		/// If the value given is greater than the max value, it is set to the max value. 
		/// </summary>
		/// <param name="value">The value to clamp</param>
		/// <param name="max">The maximum acceptable value</param>
		/// <returns>The clamped value</returns>
		public static int MaxValue(int value, int max) {
			if (value > max) {
				value = max;
			}

			return value;
		}

		/// <summary>
		/// If the value given is greater than the max value, it is set to the max value. 
		/// </summary>
		/// <param name="value">The value to clamp</param>
		/// <param name="max">The maximum acceptable value</param>
		/// <returns>The clamped value</returns>
		public static float MaxValue(float value, float max) {
			if (value > max) {
				value = max;
			}

			return value;
		}

		/// <summary>
		/// Calculates the value given inputs for lerping
		/// </summary>
		/// <param name="currentTime">The moment of the Lerp in Seconds</param>
		/// <param name="lerpLength">The length of the Lerp in Seconds</param>
		/// <param name="startValue">The initial value</param>
		/// <param name="endValue">The final value</param>
		/// <param name="easingMode">The string name of the lerp</param>
		/// <returns>The calculate value at the moment of the lerp</returns>
		public static float EaseLerp(
			float currentTime,
			float lerpLength,
			float startValue,
			float endValue,
			string easingMode
		) {
			if (currentTime > lerpLength) {
				currentTime = lerpLength;
			}

			float percentThrough = currentTime / lerpLength;
			float easeVal = EasingModeFormulas.Ease(easingMode, percentThrough);
			return easeVal * (endValue - startValue) + startValue;
		}

		/// <summary>
		/// Calculates the value given inputs for lerping
		/// </summary>
		/// <param name="currentTime">The moment of the Lerp in Seconds</param>
		/// <param name="lerpLength">The length of the Lerp in Seconds</param>
		/// <param name="startValue">The initial value</param>
		/// <param name="endValue">The final value</param>
		/// <param name="easingMode">The string name of the lerp</param>
		/// <returns>The calculate value at the moment of the lerp</returns>
		public static float EaseLerp(
			float currentTime,
			float lerpLength,
			float startValue,
			float endValue,
			EasingModeFormulas.EasingModes easingMode
		) {
			if (currentTime > lerpLength) {
				currentTime = lerpLength;
			}

			float percentThrough = currentTime / lerpLength;
			float easeVal = EasingModeFormulas.Ease(easingMode, percentThrough);
			return easeVal * (endValue - startValue) + startValue;
		}

		/// <summary>
		/// Swaps two entries in a List
		/// </summary>
		public static void Swap<T>(List<T> list, int index1, int index2) {
			if (IndexInRange(list, index1) && IndexInRange(list, index2)) {
				T temp = list[index1];
				list[index1] = list[index2];
				list[index2] = temp;
			}
		}

		/// <summary>
		/// Checks to see if an input is active
		/// </summary>
		public static bool GetInputResult(InputType inputType, string inputValue) {
			switch (inputType) {
				case InputType.Axis:
					bool axisIsPositive = true;

					if (inputValue.Length > 0 &&
					    inputValue[inputValue.Length - 1] == '-' ||
					    inputValue[inputValue.Length - 1] == '+'
					) {
						axisIsPositive = inputValue[inputValue.Length - 1] == '+';
						inputValue = inputValue.Substring(0, inputValue.Length - 1);
					}

					return axisIsPositive
						? Input.GetAxis(inputValue) > 0f
						: Input.GetAxis(inputValue) < 0f;
				case InputType.Button:
					return Input.GetButton(inputValue);
				case InputType.Key:
					return Input.GetKey(inputValue);
				case InputType.None:
					return false;
				default:
					return false;
			}
		}

		/// <summary>
		/// Gets the value of an input
		/// </summary>
		public static float GetInputResultValue(InputType inputType, string inputValue) {
			switch (inputType) {
				case InputType.Axis:
					bool axisIsPositive = true;

					if (inputValue.Length > 0 &&
					    inputValue[inputValue.Length - 1] == '-' ||
					    inputValue[inputValue.Length - 1] == '+'
					) {
						axisIsPositive = inputValue[inputValue.Length - 1] == '+';
						inputValue = inputValue.Substring(0, inputValue.Length - 1);
					}

					return axisIsPositive
						? Input.GetAxis(inputValue)
						: -Input.GetAxis(inputValue);
				case InputType.Button:
					if (Input.GetButton(inputValue)) {
						return 1;
					}

					return 0;
				case InputType.Key:
					if (Input.GetKey(inputValue)) {
						return 1;
					}

					return 0;
				case InputType.None:
					return 0;
				default:
					return 0;
			}
		}

		/// <summary>
		/// Rounds a value to snap to the nearest Pixel position.
		/// </summary>
		/// <param name="value">The value to round</param>
		/// <returns>The rounded value</returns>
		public static float RoundToPixelPerfection(float value) {
			const double precision = 1 / GameProperties.PixelsPerUnit;

			return (float) Math.Round(value / (float) precision, MidpointRounding.AwayFromZero) *
			       (float) precision;
		}

		/// <summary>
		/// Rounds a value to snap to the nearest Pixel position.
		/// </summary>
		/// <param name="value">The value to round</param>
		/// <returns>The rounded value</returns>
		public static Vector2 RoundToPixelPerfection(Vector2 value) {
			return new Vector2(
				RoundToPixelPerfection(value.x),
				RoundToPixelPerfection(value.y)
			);
		}

		/// <summary>
		/// Rounds a value to snap to the nearest Pixel position.
		/// </summary>
		/// <param name="value">The value to round</param>
		/// <returns>The rounded value</returns>
		public static Vector3 RoundToPixelPerfection(Vector3 value) {
			return new Vector3(
				RoundToPixelPerfection(value.x),
				RoundToPixelPerfection(value.y),
				RoundToPixelPerfection(value.z)
			);
		}

		/// <summary>
		/// Checks if the given Index is found in the collection
		/// </summary>
		public static bool IndexInRange<T>(List<T> collection, int index) {
			return collection != null && index >= 0 && index < collection.Count;
		}

		/// <summary>
		/// Checks if the given Index is found in the collection
		/// </summary>
		public static bool IndexInRange<T>(T[] collection, int index) {
			return collection != null && index >= 0 && index < collection.Length;
		}

		/// <summary>
		/// Generates a random string
		/// </summary>
		/// <param name="length">The length of the string</param>
		/// <returns>The random string</returns>
		public static string GenerateRandomString(int length) {
			const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";
			string myString = "";
			for (int i = 0; i < length; i++) {
				myString += glyphs[Random.Range(0, glyphs.Length)];
			}

			return myString;
		}

		public static List<List<int>> BuildMappedObjectForPlacedStamp(
			TilesetEditorStamp tilesetEditorStamp,
			int width,
			int height
		) {
			tilesetEditorStamp.ParsedTileIds = ParseTilesetEditorStamp(tilesetEditorStamp);
			tilesetEditorStamp.RepeatableRows.Sort();
			tilesetEditorStamp.RepeatableColumns.Sort();

			//Get Repeatable Column Widths
			Dictionary<int, int> repeatableColumnWidths = new Dictionary<int, int>();
			int nonRepeatableColumnCountInStamp = tilesetEditorStamp.Width - tilesetEditorStamp.RepeatableColumns.Count;
			int repeatableColumnsInEditorObject = tilesetEditorStamp.RepeatableColumns.Count > 0
				? MinValue(width - nonRepeatableColumnCountInStamp, 0)
				: 0;

			int k = 0;
			for (int i = 0; i < repeatableColumnsInEditorObject; i++) {
				int indexOfColumnInStamp = tilesetEditorStamp.RepeatableColumns[k];

				if (repeatableColumnWidths.ContainsKey(indexOfColumnInStamp)) {
					repeatableColumnWidths[indexOfColumnInStamp]++;
				} else {
					repeatableColumnWidths.Add(indexOfColumnInStamp, 1);
				}

				k++;
				if (k > tilesetEditorStamp.RepeatableColumns.Count - 1) {
					k = 0;
				}
			}

			//Get Repeatable Row Widths
			Dictionary<int, int> repeatableRowWidths = new Dictionary<int, int>();
			int nonRepeatableRowCountInStamp = tilesetEditorStamp.Height - tilesetEditorStamp.RepeatableRows.Count;
			int repeatableRowsInEditorObject = tilesetEditorStamp.RepeatableRows.Count > 0
				? MinValue(height - nonRepeatableRowCountInStamp, 0)
				: 0;

			k = 0;
			for (int i = 0; i < repeatableRowsInEditorObject; i++) {
				int indexOfRowInStamp = tilesetEditorStamp.RepeatableRows[k];

				if (repeatableRowWidths.ContainsKey(indexOfRowInStamp)) {
					repeatableRowWidths[indexOfRowInStamp]++;
				} else {
					repeatableRowWidths.Add(indexOfRowInStamp, 1);
				}

				k++;
				if (k > tilesetEditorStamp.RepeatableRows.Count - 1) {
					k = 0;
				}
			}

			List<List<int>> mappedStampObject = new List<List<int>>(tilesetEditorStamp.ParsedTileIds);

			if (repeatableColumnWidths.Count == 0) {
				while (width > mappedStampObject[0].Count) {
					foreach (List<int> row in mappedStampObject) {
						row.Add(row[row.Count % tilesetEditorStamp.Width]);
					}
				}

				if (width < mappedStampObject[0].Count) {
					int numberToRemove = mappedStampObject[0].Count - width;
					int numberOfRepeatableColumns = tilesetEditorStamp.RepeatableColumns.Count;

					for (int i = 0; i < numberToRemove; i++) {
						if (i >= numberOfRepeatableColumns) {
							break;
						}

						foreach (List<int> row in mappedStampObject) {
							row.RemoveAt(tilesetEditorStamp.RepeatableColumns[i]);
						}
					}
				}

				while (width < mappedStampObject[0].Count) {
					foreach (List<int> row in mappedStampObject) {
						row.RemoveAt(row.Count - 1);
					}
				}
			} else {
				for (int i = tilesetEditorStamp.RepeatableColumns.Count - 1; i >= 0; i--) {
					int columnToRepeat = tilesetEditorStamp.RepeatableColumns[i];
					if (repeatableColumnWidths.ContainsKey(columnToRepeat)) {
						foreach (List<int> row in mappedStampObject) {
							for (int j = 0; j < repeatableColumnWidths[columnToRepeat] - 1; j++) {
								row.Insert(columnToRepeat, row[columnToRepeat]);
							}
						}
					}
				}
			}

			if (repeatableRowWidths.Count == 0) {
				if (height < mappedStampObject.Count) {
					int numberToRemove = mappedStampObject.Count - height;
					int numberOfRepeatableRows = tilesetEditorStamp.RepeatableRows.Count;

					for (int i = 0; i < numberToRemove; i++) {
						if (i >= numberOfRepeatableRows) {
							break;
						}

						mappedStampObject.RemoveAt(tilesetEditorStamp.RepeatableRows[i]);
					}
				}

				while (height < mappedStampObject.Count) {
					mappedStampObject.RemoveAt(mappedStampObject.Count - 1);
				}

				while (height > mappedStampObject.Count) {
					mappedStampObject.Add(mappedStampObject[mappedStampObject.Count % tilesetEditorStamp.Height]);
				}
			} else {
				for (int i = tilesetEditorStamp.RepeatableRows.Count - 1; i >= 0; i--) {
					int rowToRepeat = tilesetEditorStamp.RepeatableRows[i];

					if (repeatableRowWidths.ContainsKey(rowToRepeat)) {
						for (int j = 0; j < repeatableRowWidths[rowToRepeat] - 1; j++) {
							mappedStampObject.Insert(rowToRepeat, mappedStampObject[rowToRepeat]);
						}
					}
				}
			}

			return mappedStampObject;
		}

		public static List<List<int>> ParseTilesetEditorStamp(TilesetEditorStamp tilesetEditorStamp) {
			List<List<int>> parsedTilesetEditorStamp = new List<List<int>>();

			foreach (string tileRow in tilesetEditorStamp.TileRows) {
				List<string> tiles = tileRow.Split(',').ToList();
				List<int> tileInts = new List<int>();

				foreach (string tile in tiles) {
					int value = int.TryParse(tile, out int parsedInt) ? parsedInt : 0;
					tileInts.Add(value);
				}

				parsedTilesetEditorStamp.Add(tileInts);
			}

			return parsedTilesetEditorStamp;
		}

		/// <summary>
		/// Compares two values with a comparison
		/// </summary>
		public static bool CompareValue(ValueComparison comparison, float targetValue, float value) {
			return comparison == ValueComparison.LessThan && value < targetValue ||
			       comparison == ValueComparison.LessThanOrEqual && value <= targetValue ||
			       comparison == ValueComparison.Equal && Math.Abs(value - targetValue) < 0.1 ||
			       comparison == ValueComparison.GreaterThanOrEqual && value >= targetValue ||
			       comparison == ValueComparison.GreaterThan && value >= targetValue;
		}
		
		public static Vector2 GetPointOnBezier(float time, Vector2 startPoint, Vector2 controlA, Vector2 controlB, Vector2 endPoint) {
			float cx = 3 * (controlA.x - startPoint.x);
			float cy = 3 * (controlA.y - startPoint.y);
			float bx = 3 * (controlB.x - controlA.x) - cx;
			float by = 3 * (controlB.y - controlA.y) - cy;
			float ax = endPoint.x - startPoint.x - cx - bx;
			float ay = endPoint.y - startPoint.y - cy - by;
			float cube = time * time * time;
			float square = time * time;

			float resX = (ax * cube) + (bx * square) + (cx * time) + startPoint.x;
			float resY = (ay * cube) + (by * square) + (cy * time) + startPoint.y;

			return new Vector2(resX, resY);
		}
	}
}