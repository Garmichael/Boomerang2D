using Boomerang2DFramework.Framework.EditorHelpers;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.StartHere {
	public class GeneralTips : EditorWindow {
		private static GeneralTips _window;
		private static float _windowHeight;

		private float _repaintTime;
		private bool _repaintNext;
		private float _totalTime;

		[MenuItem("Tools/Boomerang2D/General Tips", false, 2)]
		public static void ShowWindow() {
			_window = (GeneralTips) GetWindow(typeof(GeneralTips));
			_window.Show();
			_window.autoRepaintOnSceneChange = true;
			
			_window.titleContent = new GUIContent(
				"General Tips",
				AssetDatabase.LoadAssetAtPath<Texture>("Assets/Boomerang2DFramework/Framework/EditorAssets/editorWindowIconTips.png")
			);
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
			SuperForms.Region.MainArea(new Rect(0, 0, position.width, position.height), () => {
				SuperForms.Title("General Tips");

				SuperForms.Region.Scroll("GeneralTipsMainScroll", () => {
					SuperForms.Region.VerticalBox(() => {
						
						SuperForms.Space();
						
						SuperForms.BoxHeader("No Cameras Rendering Message");
						SuperForms.ParagraphLabel(
							"     If you look at the Game tab, you may get a message telling you \"No cameras rendering\". To clear " +
							"this message, right click the tab on your Game Window and uncheck the option \"Warn if No Cameras Rendering\"");

						SuperForms.Space();
						
						SuperForms.BoxHeader("Unexplained Dramatic Slowdown");
						SuperForms.ParagraphLabel("     Sometimes your game might run very slowly. This is usually because you have something selected in the " +
						                          "Hierarchy or Project tabs. Try unselecting everything.");

						SuperForms.Space();
						SuperForms.BoxHeader("Weird Issues in the various Editors");
						SuperForms.ParagraphLabel("     While in a Boomerang2D Editor, if data is missing, your console log is filling up with errors, or functionality " +
						                          "just isn't working, Rebuild the Database then close and re-open the editor window that's giving you problems.");

						SuperForms.Space();
						SuperForms.Space();
						SuperForms.Space();
						SuperForms.Space();
					});
				});
			});
		}
	}
}