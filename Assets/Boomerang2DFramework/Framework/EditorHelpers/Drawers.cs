using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEngine;

namespace Boomerang2DFramework.Framework.EditorHelpers {
	public static class Drawers {
#if UNITY_EDITOR
		public static void DrawTileBorder(int borderWidth, Texture texture2D, Rect bounds) {
			GUI.DrawTexture(new Rect(bounds.x, bounds.y, bounds.width, borderWidth), texture2D);
			GUI.DrawTexture(new Rect(bounds.x, bounds.y + bounds.width - borderWidth, bounds.height, borderWidth), texture2D);
			GUI.DrawTexture(new Rect(bounds.x, bounds.y + borderWidth, borderWidth, bounds.height - borderWidth * 2), texture2D);
			GUI.DrawTexture(new Rect(bounds.x + bounds.width - borderWidth, bounds.y + borderWidth, borderWidth, bounds.height - borderWidth * 2), texture2D);
		}

		public static void DrawTileColliders(Rect tileSpace, TileProperties tileProperties, Collider2D[] tileColliders) {
			foreach (Collider2D tileCollider in tileColliders) {
				Vector2[][] paths = new Vector2[0][];

				List<Vector2[]> pathsList = new List<Vector2[]>();

				PolygonCollider2D typesAsPolygonCollider = tileCollider as PolygonCollider2D;
				EdgeCollider2D typesAsEdgeCollider = tileCollider as EdgeCollider2D;
				BoxCollider2D typedAsBoxCollider = tileCollider as BoxCollider2D;

				if (typesAsPolygonCollider != null) {
					pathsList.Clear();

					for (int i = 0; i < typesAsPolygonCollider.pathCount; i++) {
						Vector2[] pathPoints = typesAsPolygonCollider.GetPath(i);

						for (int j = 0; j < pathPoints.Length; j++) {
							pathPoints[j].x += typesAsPolygonCollider.offset.x;
							pathPoints[j].y += typesAsPolygonCollider.offset.y;
						}

						pathsList.Add(pathPoints);
					}

					paths = pathsList.ToArray();
				} else if (typesAsEdgeCollider != null) {
					Vector2[] pathPoints = typesAsEdgeCollider.points;

					for (int i = 0; i < pathPoints.Length; i++) {
						pathPoints[i].x += typesAsEdgeCollider.offset.x;
						pathPoints[i].y += typesAsEdgeCollider.offset.y;
					}

					pathsList.Add(pathPoints);
					paths = pathsList.ToArray();
				} else if (typedAsBoxCollider != null) {
					List<Vector2> colliderPoints = new List<Vector2> {
						new Vector2(-typedAsBoxCollider.size.x / 2 - typedAsBoxCollider.offset.x, typedAsBoxCollider.size.y / 2 + typedAsBoxCollider.offset.y),
						new Vector2(typedAsBoxCollider.size.x / 2 - typedAsBoxCollider.offset.x, typedAsBoxCollider.size.y / 2 + typedAsBoxCollider.offset.y),
						new Vector2(typedAsBoxCollider.size.x / 2 - typedAsBoxCollider.offset.x, -typedAsBoxCollider.size.y / 2 + typedAsBoxCollider.offset.y),
						new Vector2(-typedAsBoxCollider.size.x / 2 - typedAsBoxCollider.offset.x, -typedAsBoxCollider.size.y / 2 + typedAsBoxCollider.offset.y)
					};

					pathsList.Clear();
					pathsList.Add(colliderPoints.ToArray());
					paths = pathsList.ToArray();
				}

				foreach (Vector2[] points in paths) {
					if (points.Length > 0) {
						for (int point = 0; point < points.Length; point++) {
							float angle = Mathf.Deg2Rad * -tileProperties.CollisionRotation;

							float newX = points[point].x * Mathf.Cos(angle) - points[point].y * Mathf.Sin(angle);
							float newY = points[point].x * Mathf.Sin(angle) + points[point].y * Mathf.Cos(angle);

							points[point].x = newX;
							points[point].y = newY;

							points[point].x = (0.5f + points[point].x) * 46;
							points[point].y = (0.5f + points[point].y) * 46;

							if (tileProperties.CollisionFlippedX) {
								points[point].x = 46 - points[point].x;
							}

							if (!tileProperties.CollisionFlippedY) {
								points[point].y = 46 - points[point].y;
							}

							points[point].x += tileProperties.CollisionOffset.x * 46;
							points[point].y += tileProperties.CollisionOffset.y * 46;
						}

						Shader shader = Shader.Find("Hidden/Internal-Colored");
						Material material = new Material(shader);

						GUI.BeginClip(new Rect(tileSpace.x, tileSpace.y, tileSpace.width * 10, tileSpace.height * 10));
						material.SetPass(0);
						GL.Begin(GL.LINE_STRIP);
						GL.Color(new Color(0, 2, 0, 0.8f));

						for (int point = 0; point < points.Length; point++) {
							GL.Vertex3(points[point].x, points[point].y, 0);
						}

						if (!(tileCollider is EdgeCollider2D)) {
							GL.Vertex3(points[0].x, points[0].y, 0);
						}

						GL.End();
						GUI.EndClip();
					}
				}
			}
		}

		public static int GetAnimationFrame(TileProperties tilePropertiesProperties, float totalTime) {
			float totalAnimationTime = tilePropertiesProperties.AnimationFramesSpeeds.Sum();

			float totalCycles = totalTime / totalAnimationTime;
			float progressionIntoCycle = totalCycles - Mathf.Floor(totalCycles);
			float currentCycleFrame = progressionIntoCycle * totalAnimationTime;
			float frameMax = 0;
			int destinationFrame = 0;

			for (int i = 0; i < tilePropertiesProperties.AnimationFramesSpeeds.Count; i++) {
				frameMax += tilePropertiesProperties.AnimationFramesSpeeds[i];

				if (currentCycleFrame > frameMax) {
					continue;
				}

				destinationFrame = i;
				break;
			}

			return destinationFrame;
		}
#endif
	}
}