using System.Collections.Generic;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.PropertyClasses {
	/// <summary>
	/// JSON object for an Actor's Bounding Boxes 
	/// </summary>
	[System.Serializable]
	public class BoundingBoxProperties {
		public bool Enabled = true;
		public Vector2 Size = new Vector2(1, 1);
		public Vector2 Offset = new Vector2(0, 0);
		public Vector2 RealSize => BoomerangUtils.RoundToPixelPerfection(Size / GameProperties.PixelsPerUnit);
		public Vector2 RealOffset => BoomerangUtils.RoundToPixelPerfection(Offset / GameProperties.PixelsPerUnit);

		public bool RayCastUp = true;
		public int RayCountUp = 1;
		public float RayLengthUp = 5f;
		public float RayInsetUp;
		public float RayInsetFirstUp;
		public float RayInsetLastUp;

		public bool RayCastRight = true;
		public int RayCountRight = 1;
		public float RayLengthRight = 5f;
		public float RayInsetRight;
		public float RayInsetFirstRight;
		public float RayInsetLastRight;

		public bool RayCastDown = true;
		public int RayCountDown = 1;
		public float RayLengthDown = 5f;
		public float RayInsetDown;
		public float RayInsetFirstDown;
		public float RayInsetLastDown;

		public bool RayCastLeft = true;
		public int RayCountLeft = 1;
		public float RayLengthLeft = 5f;
		public float RayInsetLeft;
		public float RayInsetFirstLeft;
		public float RayInsetLastLeft;

		public List<string> Flags = new List<string>();
	}
}