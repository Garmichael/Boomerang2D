using Boomerang2DFramework.Framework.EditorHelpers;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.StartHere {
	public class GettingStarted : EditorWindow {
		private static GettingStarted _window;
		private static float _windowHeight;

		private float _repaintTime;
		private bool _repaintNext;
		private float _totalTime;

		[MenuItem("Tools/Boomerang2D/Getting Started", false, 1)]
		public static void ShowWindow() {
			_window = (GettingStarted) GetWindow(typeof(GettingStarted));
			_window.Show();
			_window.autoRepaintOnSceneChange = true;

			_window.titleContent = new GUIContent(
				"Getting Started",
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
				SuperForms.Title("Getting Started");

				SuperForms.Region.Scroll("GettingStartedMainScroll", () => {
					SuperForms.Region.VerticalBox(() => {
						SuperForms.Space();

						SuperForms.BoxHeader("Welcome to Boomerang 2D!");
						SuperForms.ParagraphLabel("     Boomerang2D is a Framework for creating retro style 2D games. B2D takes care of all the " +
						                          "heavy lifting of a 2D game, including collision detection, ray tracing, state management, " +
						                          "map building, camera controls, and more. With B2D, you can focus on what's important: " +
						                          "building your game content. Virtually all of the functionality you need to create a 2D game is included with this " +
						                          "package, yet custom script and behavior can be easily added.");

						SuperForms.Space();
						SuperForms.BoxSubHeader("Get help and more at Boomerang2D.com and the B2D Discord Channel");
						SuperForms.Button("Visit Boomerang2D.com", () => { Application.OpenURL("https://www.boomerang2d.com"); }, GUILayout.Width(200));
						SuperForms.Button("Join Boomerang Discord", () => { Application.OpenURL("https://discord.gg/F4tpUHJsvj"); }, GUILayout.Width(200));

						SuperForms.Space();
						SuperForms.ParagraphLabel("You can find video tutorials for Boomerang 2D on our YouTube Channel");
						SuperForms.Button("View Video Tutorials", () => { Application.OpenURL("https://www.youtube.com/playlist?list=PLh_iA7J_8dx1UtZZvP0H6uSiavYcBCQcV"); }, GUILayout.Width(200));
						
						SuperForms.Space();
						SuperForms.ParagraphLabel("Also, if you're looking for a Game Development Team, you might find one at one of our other project sites, Develteam.com");
						SuperForms.Button("Visit Develteam.com", () => { Application.OpenURL("https://www.develteam.com"); }, GUILayout.Width(200));

						SuperForms.Space();

						SuperForms.BoxHeader("Setting up the required Layers");

						SerializedObject layerManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
						SerializedProperty layers = layerManager.FindProperty("layers");

						bool foundSolid = false;
						bool foundSolidOnTop = false;
						bool foundSolidOnBottom = false;
						bool foundSolidOnLeft = false;
						bool foundSolidOnRight = false;
						bool foundNonSolidTile = false;
						bool foundWeapon = false;
						bool foundActor = false;

						for (int i = 8; i < 32; i++) {
							if (layers.GetArrayElementAtIndex(i).stringValue == "Solid") {
								foundSolid = true;
							}

							if (layers.GetArrayElementAtIndex(i).stringValue == "SolidOnTop") {
								foundSolidOnTop = true;
							}

							if (layers.GetArrayElementAtIndex(i).stringValue == "SolidOnBottom") {
								foundSolidOnBottom = true;
							}

							if (layers.GetArrayElementAtIndex(i).stringValue == "SolidOnLeft") {
								foundSolidOnLeft = true;
							}

							if (layers.GetArrayElementAtIndex(i).stringValue == "SolidOnRight") {
								foundSolidOnRight = true;
							}

							if (layers.GetArrayElementAtIndex(i).stringValue == "NonSolidTile") {
								foundNonSolidTile = true;
							}

							if (layers.GetArrayElementAtIndex(i).stringValue == "Weapon") {
								foundWeapon = true;
							}

							if (layers.GetArrayElementAtIndex(i).stringValue == "Actor") {
								foundActor = true;
							}
						}

						bool displayLayersSetUpRightMessage = foundSolid && foundSolidOnTop && foundSolidOnBottom && foundSolidOnRight &&
						                                      foundSolidOnLeft && foundNonSolidTile && foundWeapon && foundActor;

						if (!displayLayersSetUpRightMessage) {
							SuperForms.BoxSubHeader("Manually Assign the Layers");
							SuperForms.ParagraphLabel("For Boomerang2D to work, you must have the following layers set: ");
							SuperForms.ParagraphLabel("- Solid");
							SuperForms.ParagraphLabel("- SolidOnTop");
							SuperForms.ParagraphLabel("- SolidOnBottom");
							SuperForms.ParagraphLabel("- SolidOnLeft");
							SuperForms.ParagraphLabel("- SolidOnRight");
							SuperForms.ParagraphLabel("- NonSolidTile");
							SuperForms.ParagraphLabel("- Weapon");
							SuperForms.ParagraphLabel("- Actor");
							SuperForms.Space();
							SuperForms.ParagraphLabel("Set them in Edit -> Project Settings -> Tags and Layers");
							SuperForms.ParagraphLabel("They can go on any index.");

							SuperForms.Space();
							SuperForms.BoxSubHeader("Automatically Assign the Layers");
							SuperForms.ParagraphLabel("You can use this button to do it for you.");
							SuperForms.ParagraphLabel("NOTE: This will overwrite layers 8 through 12 if you already have them set to something else");


							SuperForms.Button("Assign Layers", () => {
								layers.GetArrayElementAtIndex(8).stringValue = "Solid";
								layers.GetArrayElementAtIndex(9).stringValue = "SolidOnTop";
								layers.GetArrayElementAtIndex(10).stringValue = "SolidOnBottom";
								layers.GetArrayElementAtIndex(11).stringValue = "SolidOnLeft";
								layers.GetArrayElementAtIndex(12).stringValue = "SolidOnRight";
								layers.GetArrayElementAtIndex(13).stringValue = "NonSolidTile";
								layers.GetArrayElementAtIndex(14).stringValue = "Weapon";
								layers.GetArrayElementAtIndex(15).stringValue = "Actor";
								layerManager.ApplyModifiedProperties();
							}, GUILayout.Width(200));
						} else {
							SuperForms.ParagraphLabel("Congratulations, your layers are set up properly");
						}

						SuperForms.Space();
						
						SuperForms.BoxHeader("Set your Game Properties");
						SuperForms.ParagraphLabel("Open GameProperties.cs to edit your game settings.");

						SuperForms.Space();

						SuperForms.BoxSubHeader("PixelsPerUnit");
						SuperForms.ParagraphLabel("Your base Unit Size. Should be a Power of Two. Changing this setting will force you to " +
						                          "re-import all of your sprite data for Actors, Tilesets, and Bitmap Fonts. " +
						                          "Also, changing it will break the demo content so check that out first.");

						SuperForms.Space();

						SuperForms.BoxSubHeader("AspectWidth / AspectHeight");
						SuperForms.ParagraphLabel("The Aspect Ratio of your project. This aspect ratio is independent of the game's resolution " +
						                          "and the game screen will be fit to fill the space of the game's resolution.");

						SuperForms.Space();


						SuperForms.BoxSubHeader("RenderDisplayMode");
						SuperForms.ParagraphLabel("The options are Fit, PixelPerfect, and Stretch. This determines how the viewport will render the game.");

						SuperForms.Space();

						SuperForms.BoxSubHeader("UnitsWide");
						SuperForms.ParagraphLabel("Defines how many units will fit on the screen horizontally. Multiply UnitsWide by PixelsPerUnit " +
						                          "to determine how many pixels wide your game render.");

						SuperForms.Space();

						SuperForms.BoxSubHeader("Other settings...");
						SuperForms.ParagraphLabel("The remaining settings in this class should not be modified");

						SuperForms.Space();
						
						SuperForms.BoxHeader("The Boomerang2D GameObject");
						SuperForms.ParagraphLabel("     The only object you need attached to your scene is a single GameObject called \"Boomerang2D\". " +
						                          "Not even a camera is required. The GameObject must also have the script \"Boomerang Reference Database\" " +
						                          "attached to it. Look at the included Scene found in the Boomerang2DFramework root directory for an example " +
						                          "of this. ");
						SuperForms.ParagraphLabel(
							"     If the various Boomerang2D Editors give a failed message, the lack of this GameObject with the correct " +
							"script attached is most likely the problem.");

						SuperForms.Space();
						
						SuperForms.BoxHeader("Reference Database");
						SuperForms.ParagraphLabel(
							"     When first setting up your project, you must Build the Reference Database.  It can be found in the menu option " +
							"Boomerang2D -> Build Reference Database. ");
						SuperForms.Space();
						
						SuperForms.ParagraphLabel(
							"     In order to load your content for use in the game, Boomerang2D uses a Reference Database. This stores " +
							"references to your Actors, Weapons, Maps, Tilesets, and all other content you've created. When modifying and saving content " +
							"through the content editors, the Reference Database is automatically updated. However, sometimes, you may need to do this " +
							"manually.You will also need to save your scene " +
							"whenever the database is updated or the next time you open Unity, you will have to rebuild the database.");
						SuperForms.Space();
						
						SuperForms.ParagraphLabel(
							"     If the database is not in sync with the content, you will get run-time errors.");
						SuperForms.Space();

						SuperForms.BoxHeader("Start Game Script");
						SuperForms.ParagraphLabel(
							"     To kick off the framework, have a game object with a StartGame monobehavior attached to it. It requires a call to " +
							"`Boomerang2D.InitializeFramework()` before anything else should happen. It is recommended that you attach your StartGame script " +
							"to the same GameObject that the Database Reference is attached as explained above.");

						SuperForms.ParagraphLabel(
							"     After Initializing the Framework, simply load any map you've built using: MapManager.BuildMap(\"MAPNAME\"); and the " +
							"Map Builder will create all the Actors, Scripts, and other Assets for your scene!");

						SuperForms.ParagraphLabel(
							"     An example of a StartGame script is included as " +
							"StartGame.cs. Note that StartGame.cs will be overwritten with patch updates, so it's recommended that you create your own using " +
							"this as a guideline.");
						
						SuperForms.Space();
						
						SuperForms.BoxHeader("Input");
						SuperForms.ParagraphLabel("     There's an Input Asset in the Framework's Root Directory that is set up for Keyboard and Xbox " +
						                          "Controllers. To play the Demo content, make sure you set this Input Asset to your main Input scheme. Do this" +
						                          " by going Edit -> Project Settings -> Input and clicking the settings icon at the top right. Select " +
						                          "the InputManagerXboxController from the available options. *");
						
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