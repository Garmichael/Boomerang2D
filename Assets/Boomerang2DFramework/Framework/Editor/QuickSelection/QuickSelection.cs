using Boomerang2DFramework.Framework.EditorHelpers;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.QuickSelection {
	public class QuickSelection : EditorWindow {
		private static QuickSelection _window;

		private float _repaintTime;
		private bool _repaintNext;
		private float _totalTime;

		private static float WindowWidth => EditorGUIUtility.currentViewWidth - 4;

		private static float WindowHeight => Screen.height - 22;


		[MenuItem("Tools/Boomerang2D/Selection Helpers", false, 52)]
		public static void ShowWindow() {
			_window = (QuickSelection) GetWindow(typeof(QuickSelection));
			_window.Show();
			_window.autoRepaintOnSceneChange = true;
			_window.titleContent = new GUIContent("Quick Selection");
		}

		private void Update() {
			_totalTime = Time.realtimeSinceStartup;
			UpdateRepaint();
		}

		private void UpdateRepaint() {
			const float fps = 10f;

			if (_repaintTime > _totalTime + 1f) {
				_repaintTime = 0f;
			}

			if (_repaintTime + 1.0f / fps < _totalTime || _repaintNext) {
				Repaint();
				_repaintTime = _totalTime;
				_repaintNext = false;
			}
		}

		private void OnGUI() {
			SuperForms.Region.MainArea(new Rect(0, 0, WindowWidth, WindowHeight), () => {
				SuperForms.BoxHeader("Quick Selections");

				SuperForms.BoxSubHeader("Player Object");

				SuperForms.Button("Select Player", () => {
					if (Boomerang2D.Player != null) {
						Selection.objects = new Object[] {Boomerang2D.Player.GameObject};
					}
				});
				SuperForms.Button("Pan to Player", () => {
					if (Boomerang2D.Player != null) {
						Selection.objects = new Object[] {Boomerang2D.Player.GameObject};
						if (Selection.activeGameObject != null) {
							SceneView.lastActiveSceneView.LookAt(Selection.activeGameObject.transform.position);
						}
					}
				});
			});
		}
	}
}