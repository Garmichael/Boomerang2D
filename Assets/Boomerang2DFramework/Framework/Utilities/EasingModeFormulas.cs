using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Utilities {
	public static class EasingModeFormulas {
		private static readonly Dictionary<string, Func<float, float>> EasingFormulas = new Dictionary<string, Func<float, float>>() {
			{"Linear", Linear},

			{"QuadraticEaseIn", QuadraticEaseIn},
			{"QuadraticEaseOut", QuadraticEaseOut},
			{"QuadraticEaseInOut", QuadraticEaseInOut},

			{"CubicEaseIn", CubicEaseIn},
			{"CubicEaseOut", CubicEaseOut},
			{"CubicEaseInOut", CubicEaseInOut},

			{"QuarticEaseIn", QuarticEaseIn},
			{"QuarticEaseOut", QuarticEaseOut},
			{"QuarticEaseInOut", QuarticEaseInOut},

			{"QuinticEaseIn", QuinticEaseIn},
			{"QuinticEaseOut", QuinticEaseOut},
			{"QuinticEaseInOut", QuinticEaseInOut},

			{"SineEaseIn", SineEaseIn},
			{"SineEaseOut", SineEaseOut},
			{"SineEaseInOut", SineEaseInOut},

			{"CircularEaseIn", CircularEaseIn},
			{"CircularEaseOut", CircularEaseOut},
			{"CircularEaseInOut", CircularEaseInOut},

			{"ExponentialEaseIn", ExponentialEaseIn},
			{"ExponentialEaseOut", ExponentialEaseOut},
			{"ExponentialEaseInOut", ExponentialEaseInOut},

			{"ElasticEaseIn", ElasticEaseIn},
			{"ElasticEaseOut", ElasticEaseOut},
			{"ElasticEaseInOut", ElasticEaseInOut},

			{"BackEaseIn", BackEaseIn},
			{"BackEaseOut", BackEaseOut},
			{"BackEaseInOut", BackEaseInOut},

			{"BounceEaseIn", BounceEaseIn},
			{"BounceEaseOut", BounceEaseOut},
			{"BounceEaseInOut", BounceEaseInOut}
		};
		
		public enum EasingModes {
			Linear,
			QuadraticEaseIn,
			QuadraticEaseOut,
			QuadraticEaseInOut,
			CubicEaseIn,
			CubicEaseOut,
			CubicEaseInOut,
			QuarticEaseIn,
			QuarticEaseOut,
			QuarticEaseInOut,
			QuinticEaseIn,
			QuinticEaseOut,
			QuinticEaseInOut,
			SineEaseIn,
			SineEaseOut,
			SineEaseInOut,
			CircularEaseIn,
			CircularEaseOut,
			CircularEaseInOut,
			ExponentialEaseIn,
			ExponentialEaseOut,
			ExponentialEaseInOut,
			ElasticEaseIn,
			ElasticEaseOut,
			ElasticEaseInOut,
			BackEaseIn,
			BackEaseOut,
			BackEaseInOut,
			BounceEaseIn,
			BounceEaseOut,
			BounceEaseInOut
		}

		private static readonly Dictionary<EasingModes, Func<float, float>> EasingFormulasEnums = new Dictionary<EasingModes, Func<float, float>>() {
			{EasingModes.Linear, Linear},
			{EasingModes.QuadraticEaseIn, QuadraticEaseIn},
			{EasingModes.QuadraticEaseOut, QuadraticEaseOut},
			{EasingModes.QuadraticEaseInOut, QuadraticEaseInOut},

			{EasingModes.CubicEaseIn, CubicEaseIn},
			{EasingModes.CubicEaseOut, CubicEaseOut},
			{EasingModes.CubicEaseInOut, CubicEaseInOut},

			{EasingModes.QuarticEaseIn, QuarticEaseIn},
			{EasingModes.QuarticEaseOut, QuarticEaseOut},
			{EasingModes.QuarticEaseInOut, QuarticEaseInOut},

			{EasingModes.QuinticEaseIn, QuinticEaseIn},
			{EasingModes.QuinticEaseOut, QuinticEaseOut},
			{EasingModes.QuinticEaseInOut, QuinticEaseInOut},

			{EasingModes.SineEaseIn, SineEaseIn},
			{EasingModes.SineEaseOut, SineEaseOut},
			{EasingModes.SineEaseInOut, SineEaseInOut},

			{EasingModes.CircularEaseIn, CircularEaseIn},
			{EasingModes.CircularEaseOut, CircularEaseOut},
			{EasingModes.CircularEaseInOut, CircularEaseInOut},

			{EasingModes.ExponentialEaseIn, ExponentialEaseIn},
			{EasingModes.ExponentialEaseOut, ExponentialEaseOut},
			{EasingModes.ExponentialEaseInOut, ExponentialEaseInOut},

			{EasingModes.ElasticEaseIn, ElasticEaseIn},
			{EasingModes.ElasticEaseOut, ElasticEaseOut},
			{EasingModes.ElasticEaseInOut, ElasticEaseInOut},

			{EasingModes.BackEaseIn, BackEaseIn},
			{EasingModes.BackEaseOut, BackEaseOut},
			{EasingModes.BackEaseInOut, BackEaseInOut},

			{EasingModes.BounceEaseIn, BounceEaseIn},
			{EasingModes.BounceEaseOut, BounceEaseOut},
			{EasingModes.BounceEaseInOut, BounceEaseInOut}
		};

		public static float Ease(string easeMode, float lerpVal) {
			if (easeMode == null) {
				easeMode = "Linear";
			}

			return EasingFormulas.ContainsKey(easeMode)
				? EasingFormulas[easeMode](lerpVal)
				: EasingFormulas["Linear"](lerpVal);
		}
		
		public static float Ease(EasingModes easeMode, float lerpVal) {
			return EasingFormulasEnums.ContainsKey(easeMode)
				? EasingFormulasEnums[easeMode](lerpVal)
				: EasingFormulasEnums[EasingModes.Linear](lerpVal);
		}

		public static List<string> GetListOfEasingMethods() {
			return EasingFormulas.Select(x => x.Key).ToList();
		}

		private static float Linear(float lerpVal) {
			return lerpVal;
		}

		private static float QuadraticEaseIn(float lerpVal) {
			return lerpVal * lerpVal;
		}

		private static float QuadraticEaseOut(float lerpVal) {
			return -(lerpVal * (lerpVal - 2));
		}

		private static float QuadraticEaseInOut(float lerpVal) {
			return lerpVal < 0.5f
				? 2 * lerpVal * lerpVal
				: (-2 * lerpVal * lerpVal) + (4 * lerpVal) - 1;
		}

		private static float CubicEaseIn(float lerpVal) {
			return lerpVal * lerpVal * lerpVal;
		}

		private static float CubicEaseOut(float lerpVal) {
			float i = (lerpVal - 1);
			return i * i * i + 1;
		}

		private static float CubicEaseInOut(float lerpVal) {
			if (lerpVal < 0.5f) {
				return 4 * lerpVal * lerpVal * lerpVal;
			}

			float i = ((2 * lerpVal) - 2);
			return 0.5f * i * i * i + 1;
		}

		private static float QuarticEaseIn(float lerpVal) {
			return lerpVal * lerpVal * lerpVal * lerpVal;
		}

		private static float QuarticEaseOut(float lerpVal) {
			float i = (lerpVal - 1);
			return i * i * i * (1 - lerpVal) + 1;
		}

		private static float QuarticEaseInOut(float lerpVal) {
			if (lerpVal < 0.5f) {
				return 8 * lerpVal * lerpVal * lerpVal * lerpVal;
			}

			float i = (lerpVal - 1);
			return -8 * i * i * i * i + 1;
		}

		private static float QuinticEaseIn(float lerpVal) {
			return lerpVal * lerpVal * lerpVal * lerpVal * lerpVal;
		}

		private static float QuinticEaseOut(float lerpVal) {
			float i = (lerpVal - 1);
			return i * i * i * i * i + 1;
		}

		private static float QuinticEaseInOut(float lerpVal) {
			if (lerpVal < 0.5f) {
				return 16 * lerpVal * lerpVal * lerpVal * lerpVal * lerpVal;
			}

			float i = ((2 * lerpVal) - 2);
			return 0.5f * i * i * i * i * i + 1;
		}

		private static float SineEaseIn(float lerpVal) {
			return Mathf.Sin((lerpVal - 1) * Mathf.PI * 0.5f) + 1;
		}

		private static float SineEaseOut(float lerpVal) {
			return Mathf.Sin(lerpVal * Mathf.PI * 0.5f);
		}

		private static float SineEaseInOut(float lerpVal) {
			return 0.5f * (1 - Mathf.Cos(lerpVal * Mathf.PI));
		}

		private static float CircularEaseIn(float lerpVal) {
			return 1 - Mathf.Sqrt(1 - (lerpVal * lerpVal));
		}

		private static float CircularEaseOut(float lerpVal) {
			return Mathf.Sqrt((2 - lerpVal) * lerpVal);
		}

		private static float CircularEaseInOut(float lerpVal) {
			return lerpVal < 0.5f
				? 0.5f * (1 - Mathf.Sqrt(1 - 4 * (lerpVal * lerpVal)))
				: 0.5f * (Mathf.Sqrt(-((2 * lerpVal) - 3) * ((2 * lerpVal) - 1)) + 1);
		}

		private static float ExponentialEaseIn(float lerpVal) {
			return (Math.Abs(lerpVal) < 0.001)
				? lerpVal
				: Mathf.Pow(2, 10 * (lerpVal - 1));
		}

		private static float ExponentialEaseOut(float lerpVal) {
			return (Math.Abs(lerpVal - 1.0f) < 0.001)
				? lerpVal
				: 1 - Mathf.Pow(2, -10 * lerpVal);
		}

		private static float ExponentialEaseInOut(float lerpVal) {
			if (Math.Abs(lerpVal) < 0.001 || Math.Abs(lerpVal - 1.0) < 0.001) {
				return lerpVal;
			}

			return lerpVal < 0.5f
				? 0.5f * Mathf.Pow(2, (20 * lerpVal) - 10)
				: -0.5f * Mathf.Pow(2, (-20 * lerpVal) + 10) + 1;
		}

		private static float ElasticEaseIn(float lerpVal) {
			return Mathf.Sin(13 * Mathf.PI * 0.5f * lerpVal) * Mathf.Pow(2, 10 * (lerpVal - 1));
		}

		private static float ElasticEaseOut(float lerpVal) {
			return Mathf.Sin(-13 * Mathf.PI * 0.5f * (lerpVal + 1)) * Mathf.Pow(2, -10 * lerpVal) + 1;
		}

		private static float ElasticEaseInOut(float lerpVal) {
			return lerpVal < 0.5f
				? 0.5f * Mathf.Sin(13 * Mathf.PI * 0.5f * (2 * lerpVal)) * Mathf.Pow(2, 10 * ((2 * lerpVal) - 1))
				: 0.5f * (Mathf.Sin(-13 * Mathf.PI * 0.5f * ((2 * lerpVal - 1) + 1)) * Mathf.Pow(2, -10 * (2 * lerpVal - 1)) + 2);
		}

		private static float BackEaseIn(float lerpVal) {
			return lerpVal * lerpVal * lerpVal - lerpVal * Mathf.Sin(lerpVal * Mathf.PI);
		}

		private static float BackEaseOut(float lerpVal) {
			float i = (1 - lerpVal);
			return 1 - (i * i * i - i * Mathf.Sin(i * Mathf.PI));
		}

		private static float BackEaseInOut(float lerpVal) {
			if (lerpVal < 0.5f) {
				float i = 2 * lerpVal;
				return 0.5f * (i * i * i - i * Mathf.Sin(i * Mathf.PI));
			} else {
				float i = (1 - (2 * lerpVal - 1));
				return 0.5f * (1 - (i * i * i - i * Mathf.Sin(i * Mathf.PI))) + 0.5f;
			}
		}

		private static float BounceEaseIn(float lerpVal) {
			return 1 - BounceEaseOut(1 - lerpVal);
		}

		private static float BounceEaseOut(float lerpVal) {
			if (lerpVal < 4 / 11.0f) {
				return (121 * lerpVal * lerpVal) / 16.0f;
			}

			if (lerpVal < 8 / 11.0f) {
				return (363 / 40.0f * lerpVal * lerpVal) - (99 / 10.0f * lerpVal) + 17 / 5.0f;
			}

			if (lerpVal < 9 / 10.0f) {
				return (4356 / 361.0f * lerpVal * lerpVal) - (35442 / 1805.0f * lerpVal) + 16061 / 1805.0f;
			}

			return (54 / 5.0f * lerpVal * lerpVal) - (513 / 25.0f * lerpVal) + 268 / 25.0f;
		}

		private static float BounceEaseInOut(float lerpVal) {
			return lerpVal < 0.5f
				? 0.5f * BounceEaseIn(lerpVal * 2)
				: 0.5f * BounceEaseOut(lerpVal * 2 - 1) + 0.5f;
		}
	}
}