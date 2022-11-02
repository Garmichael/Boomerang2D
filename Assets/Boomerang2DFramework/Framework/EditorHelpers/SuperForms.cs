using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.EditorHelpers {
	public static class SuperForms {
#if UNITY_EDITOR
		static SuperForms() { }

		public static readonly AreasOpeners Begin = new AreasOpeners();
		public static readonly AreasClosers End = new AreasClosers();
		public static readonly Regions Region = new Regions();

		public static readonly Dictionary<string, Vector2> ScrollPositions = new Dictionary<string, Vector2>();

		public static void LoseFocus() {
			GUIUtility.keyboardControl = 0;
		}

		public static void ScrollRegionToTop(string id) {
			if (ScrollPositions.ContainsKey(id)) {
				Vector2 scrollPosition = ScrollPositions[id];
				scrollPosition.y = 0;
				ScrollPositions[id] = scrollPosition;
			}
		}

		public static void ScrollRegionToBottom(string id) {
			if (ScrollPositions.ContainsKey(id)) {
				Vector2 scrollPosition = ScrollPositions[id];
				scrollPosition.y += 999999;
				ScrollPositions[id] = scrollPosition;
			}
		}

		public static void Space() {
			GUILayout.Space(SuperFormsStyles.Spacing);
		}

		public static void Space(int spacing) {
			GUILayout.Space(spacing);
		}

		public static void FlexibleSpace() {
			GUILayout.FlexibleSpace();
		}

		public static void BlockedArea(params GUILayoutOption[] options) {
			GUILayout.Label("", SuperFormsStyles.Blank, options);
		}

		public static void BlockedArea(int width, int height) {
			GUILayout.Label("", SuperFormsStyles.Blank, GUILayout.Width(width), GUILayout.Height(height));
		}

		public static void Label(Texture tex) {
			GUILayout.Label(tex);
		}

		public static void Label(Texture tex, GUIStyle style) {
			GUILayout.Label(tex, style);
		}

		public static void Label(Texture tex, params GUILayoutOption[] options) {
			GUILayout.Label(tex, options);
		}

		public static void Label(Texture tex, GUIStyle style, params GUILayoutOption[] options) {
			GUILayout.Label(tex, style, options);
		}

		public static void Label(string text) {
			GUILayout.Label(text, SuperFormsStyles.BoxedLabel);
		}

		public static void Label(string text, GUIStyle style) {
			GUILayout.Label(text, style);
		}

		public static void Label(string label, params GUILayoutOption[] options) {
			GUILayout.Label(label, SuperFormsStyles.BoxedLabel, options);
		}

		public static void Label(string text, GUIStyle style, params GUILayoutOption[] options) {
			GUILayout.Label(text, style, options);
		}

		public static void Label(string label, string value) {
			EditorGUILayout.LabelField(label, value);
		}

		public static void FullBoxLabel(string label) {
			Label(label, SuperFormsStyles.BoxedLabelFull, GUILayout.ExpandWidth(true));
		}

		public static void ParagraphLabel(string label) {
			Label(label, SuperFormsStyles.ParagraphLabel, GUILayout.ExpandWidth(true));
		}

		public static void TinyButton(string label, Action action, params GUILayoutOption[] options) {
			if (GUILayout.Button(label, SuperFormsStyles.TinyButton, options)) {
				LoseFocus();
				action();
			}
		}

		public static void TinyButtonSelected(string label, Action action, params GUILayoutOption[] options) {
			if (GUILayout.Button(label, SuperFormsStyles.TinyButtonSelected, options)) {
				LoseFocus();
				action();
			}
		}

		public static void Button(string label, Action action, params GUILayoutOption[] options) {
			if (Button(label, options)) {
				LoseFocus();
				action();
			}
		}

		public static bool Button(string label, bool isSelected, params GUILayoutOption[] options) {
			return isSelected
				? ButtonSelected(label, options)
				: Button(label, options);
		}

		public static void Button(string label, bool isSelected, Action action, params GUILayoutOption[] options) {
			if (isSelected) {
				ButtonSelected(label, action, options);
			} else {
				if (Button(label, options)) {
					LoseFocus();
					action();
				}
			}
		}

		public static void ButtonSelected(string label, Action action, params GUILayoutOption[] options) {
			if (Button(label, SuperFormsStyles.ButtonSelected, options)) {
				LoseFocus();
				action();
			}
		}

		public static bool ButtonSelected(string label, params GUILayoutOption[] options) {
			return Button(label, SuperFormsStyles.ButtonSelected, options);
		}

		public static void Button(string label, Action action, GUIStyle style, params GUILayoutOption[] options) {
			if (Button(label, style, options)) {
				LoseFocus();
				action();
			}
		}

		public static bool Button(string label) {
			return GUILayout.Button(label, SuperFormsStyles.Button);
		}

		public static bool Button(GUIStyle style) {
			return GUILayout.Button("", style);
		}

		public static bool Button(string label, GUIStyle style) {
			return GUILayout.Button(label, style);
		}

		public static bool Button(string label, params GUILayoutOption[] options) {
			return GUILayout.Button(label, SuperFormsStyles.Button, options);
		}

		public static bool Button(string label, GUIStyle style, params GUILayoutOption[] options) {
			return GUILayout.Button(label, style, options);
		}

		public static bool ImageButton(Texture2D texture) {
			return GUILayout.Button(texture);
		}

		public static bool ImageButton(Texture2D texture, GUIStyle style) {
			return GUILayout.Button(texture, style);
		}

		public enum IconButtons {
			ButtonAdd,
			ButtonAddLarge,
			ButtonSaveLarge,
			ButtonSaveAlertLarge,
			ButtonDelete,
			ButtonDeleteLarge,
			ButtonRenameLarge,
			ButtonRename,
			ButtonEdit,
			ButtonEye,
			ButtonEyeLarge,
			ButtonEyeClosed,
			ButtonEyeClosedLarge,
			ButtonRefreshLarge,
			ButtonClone,
			ButtonCloneLarge,
			ButtonArrowUp,
			ButtonArrowDown,
			ButtonLock,
			ButtonUnlock,
			MapEditorDimensions,
			MapEditorDimensionsSelected,
			MapEditorTiles,
			MapEditorTilesSelected,
			MapEditorActors,
			MapEditorActorsSelected,
			MapEditorPrefabs,
			MapEditorPrefabsSelected,
			MapEditorViews,
			MapEditorViewsSelected,
			MapEditorRegions,
			MapEditorRegionsSelected
		}

		public static bool IconButton(IconButtons icon) {
			switch (icon) {
				case IconButtons.ButtonAdd:
					return Button(SuperFormsStyles.ButtonAdd);
				case IconButtons.ButtonAddLarge:
					return Button(SuperFormsStyles.ButtonAddLarge);
				case IconButtons.ButtonSaveLarge:
					return Button(SuperFormsStyles.ButtonSaveLarge);
				case IconButtons.ButtonSaveAlertLarge:
					return Button(SuperFormsStyles.ButtonSaveAlertLarge);
				case IconButtons.ButtonDelete:
					return Button(SuperFormsStyles.ButtonDelete);
				case IconButtons.ButtonDeleteLarge:
					return Button(SuperFormsStyles.ButtonDeleteLarge);
				case IconButtons.ButtonRenameLarge:
					return Button(SuperFormsStyles.ButtonRenameLarge);
				case IconButtons.ButtonRename:
					return Button(SuperFormsStyles.ButtonRename);
				case IconButtons.ButtonEdit:
					return Button(SuperFormsStyles.ButtonEdit);
				case IconButtons.ButtonEye:
					return Button(SuperFormsStyles.ButtonEye);
				case IconButtons.ButtonEyeLarge:
					return Button(SuperFormsStyles.ButtonEyeLarge);
				case IconButtons.ButtonEyeClosed:
					return Button(SuperFormsStyles.ButtonEyeClosed);
				case IconButtons.ButtonEyeClosedLarge:
					return Button(SuperFormsStyles.ButtonEyeClosedLarge);
				case IconButtons.ButtonRefreshLarge:
					return Button(SuperFormsStyles.ButtonRefreshLarge);
				case IconButtons.ButtonClone:
					return Button(SuperFormsStyles.ButtonClone);
				case IconButtons.ButtonCloneLarge:
					return Button(SuperFormsStyles.ButtonCloneLarge);
				case IconButtons.ButtonArrowUp:
					return Button(SuperFormsStyles.ButtonArrowUp);
				case IconButtons.ButtonArrowDown:
					return Button(SuperFormsStyles.ButtonArrowDown);
				case IconButtons.ButtonLock:
					return Button(SuperFormsStyles.ButtonLock);
				case IconButtons.ButtonUnlock:
					return Button(SuperFormsStyles.ButtonUnlock);
				case IconButtons.MapEditorDimensions:
					return Button(SuperFormsStyles.MapEditorDimensions);
				case IconButtons.MapEditorDimensionsSelected:
					return Button(SuperFormsStyles.MapEditorDimensionsSelected);
				case IconButtons.MapEditorTiles:
					return Button(SuperFormsStyles.MapEditorTiles);
				case IconButtons.MapEditorTilesSelected:
					return Button(SuperFormsStyles.MapEditorTilesSelected);
				case IconButtons.MapEditorActors:
					return Button(SuperFormsStyles.MapEditorActors);
				case IconButtons.MapEditorActorsSelected:
					return Button(SuperFormsStyles.MapEditorActorsSelected);
				case IconButtons.MapEditorPrefabs:
					return Button(SuperFormsStyles.MapEditorPrefabs);
				case IconButtons.MapEditorPrefabsSelected:
					return Button(SuperFormsStyles.MapEditorPrefabsSelected);
				case IconButtons.MapEditorViews:
					return Button(SuperFormsStyles.MapEditorViews);
				case IconButtons.MapEditorViewsSelected:
					return Button(SuperFormsStyles.MapEditorViewsSelected);
				case IconButtons.MapEditorRegions:
					return Button(SuperFormsStyles.MapEditorRegions);
				case IconButtons.MapEditorRegionsSelected:
					return Button(SuperFormsStyles.MapEditorRegionsSelected);
				default:
					return Button("???");
			}
		}

		public static void IconButton(IconButtons icon, Action action) {
			if (IconButton(icon)) {
				LoseFocus();
				action();
			}
		}

		public static float FloatField(float value) {
			return EditorGUILayout.FloatField(value, SuperFormsStyles.InputStyle);
		}

		public static float FloatField(string label, float value) {
			Begin.Horizontal();
			Label(label, SuperFormsStyles.BoxedLabel);
			float newValue = EditorGUILayout.FloatField(value, SuperFormsStyles.InputStyle);
			End.Horizontal();

			return newValue;
		}

		public static float FloatField(float value, params GUILayoutOption[] options) {
			Begin.Horizontal();
			float newValue = EditorGUILayout.FloatField(value, SuperFormsStyles.InputStyle, options);
			End.Horizontal();

			return newValue;
		}

		public static float FloatField(string label, float value, params GUILayoutOption[] options) {
			Begin.Horizontal();
			Label(label, SuperFormsStyles.BoxedLabel);
			float newValue = EditorGUILayout.FloatField(value, options);
			End.Horizontal();

			return newValue;
		}

		public static float FloatField(string label, float value, GUIStyle style) {
			Begin.Horizontal();
			Label(label, SuperFormsStyles.BoxedLabel);
			float newValue = EditorGUILayout.FloatField(value, style);
			End.Horizontal();

			return newValue;
		}

		public static int IntField(int value) {
			return EditorGUILayout.IntField(value, SuperFormsStyles.InputStyle);
		}

		public static int IntField(string label, int value) {
			Begin.Horizontal();
			Label(label, SuperFormsStyles.BoxedLabel);
			int newValue = EditorGUILayout.IntField(value, SuperFormsStyles.InputStyle);
			End.Horizontal();

			return newValue;
		}

		public static int IntField(int value, params GUILayoutOption[] options) {
			Begin.Horizontal(options);
			int newValue = EditorGUILayout.IntField(value, SuperFormsStyles.InputStyle, options);
			End.Horizontal();

			return newValue;
		}

		public static int IntField(string label, int value, params GUILayoutOption[] options) {
			Begin.Horizontal();
			Label(label, SuperFormsStyles.BoxedLabel);
			int newValue = EditorGUILayout.IntField(value, SuperFormsStyles.InputStyle, options);
			End.Horizontal();

			return newValue;
		}

		public static string StringField(string label, string value, params GUILayoutOption[] options) {
			Region.Horizontal(() => {
				Label(label, SuperFormsStyles.BoxedLabel);
				value = StringField(value, SuperFormsStyles.InputStyle, options);
			});

			return value;
		}

		public static string StringField(string value, params GUILayoutOption[] options) {
			string newValue = EditorGUILayout.TextField(value, SuperFormsStyles.InputStyle, options);
			return newValue;
		}

		public static string StringField(string text, GUIStyle style, params GUILayoutOption[] options) {
			string newValue = EditorGUILayout.TextField(text, style, options);
			return newValue;
		}

		public static string StringField(string label, string value, GUIStyle style, params GUILayoutOption[] options) {
			Begin.Horizontal();
			string newValue = EditorGUILayout.TextField(label, value, style, options);
			End.Horizontal();
			return newValue;
		}

		public static string TextArea(string label, string value, string id, params GUILayoutOption[] options) {
			Begin.Horizontal();
			Label(label, SuperFormsStyles.BoxedLabel);
			string newValue = EditorGUILayout.TextArea(value, GUILayout.Height(48));
			End.Horizontal();
			return newValue;
		}

		public static string TextArea(string value, params GUILayoutOption[] options) {
			return  EditorGUILayout.TextArea(value, SuperFormsStyles.TextBoxStyle, options);
		}

		public static int DropDown(int index, string[] options) {
			return EditorGUILayout.Popup(index, options, SuperFormsStyles.Dropdown);
		}

		public static int DropDown(int index, float[] options) {
			List<string> stringOptions = new List<string>();
			foreach (float option in options) {
				stringOptions.Add(option.ToString(CultureInfo.InvariantCulture));
			}

			return EditorGUILayout.Popup(index, stringOptions.ToArray(), SuperFormsStyles.Dropdown);
		}

		public static int DropDown(string label, int index, string[] options) {
			Begin.Horizontal();
			Label(label);
			int returnIndex = EditorGUILayout.Popup(index, options, SuperFormsStyles.Dropdown);
			End.Horizontal();
			return returnIndex;
		}

		public static int DropDown(int index, string[] selectOptions, params GUILayoutOption[] options) {
			return EditorGUILayout.Popup(index, selectOptions, SuperFormsStyles.Dropdown, options);
		}

		public static int DropDown(int index, string[] selectOptions, GUIStyle style, params GUILayoutOption[] options) {
			return EditorGUI.Popup(EditorGUILayout.GetControlRect(false, style.fixedHeight, style, options),
				index, selectOptions, style);
		}

		public static int DropDown(int index, string[] selectOptions, GUIStyle style) {
			return EditorGUI.Popup(EditorGUILayout.GetControlRect(false, style.fixedHeight, style),
				index, selectOptions, style);
		}


		public static int DropDown(string label, int index, string[] selectOptions, GUIStyle style) {
			return EditorGUI.Popup(EditorGUILayout.GetControlRect(false, style.fixedHeight, style), label,
				index, selectOptions, style);
		}

		public static int DropDownLarge(int index, string[] selectOptions, params GUILayoutOption[] options) {
			return DropDown(
				index,
				selectOptions,
				SuperFormsStyles.DropdownLarge,
				options
			);
		}

		public static object EnumDropdown(string label, object value, params GUILayoutOption[] options) {
			Begin.Horizontal();
			Label(label, SuperFormsStyles.BoxedLabel);
			Enum dropdownValue = EditorGUILayout.EnumPopup((Enum) value, SuperFormsStyles.Dropdown, options);
			End.Horizontal();
			return dropdownValue;
		}

		public static object EnumDropdown(object value, params GUILayoutOption[] options) {
			Enum dropdownValue = EditorGUILayout.EnumPopup((Enum) value, SuperFormsStyles.Dropdown, options);
			return dropdownValue;
		}

		public static string DictionaryDropDown(Dictionary<string, string> dictionary, string value) {
			return DictionaryDropDown("", dictionary, value);
		}

		public static string DictionaryDropDown(string label, Dictionary<string, string> dictionary, string value) {
			return DictionaryDropDown(label, dictionary, value, SuperFormsStyles.Dropdown);
		}

		public static string DictionaryDropDown(string label, Dictionary<string, string> dictionary, string value,
			GUIStyle style) {
			List<string> keys = dictionary.Select(className => className.Key).ToList();

			string selectedBaseClassKey = dictionary.FirstOrDefault(x => x.Value == value).Key;

			int selectedIndex = keys.IndexOf(selectedBaseClassKey);

			if (selectedIndex < 0) {
				selectedIndex = 0;
			}

			Begin.Horizontal();
			if (label != "") {
				Label(label, SuperFormsStyles.BoxedLabel);
			}

			selectedIndex = DropDown(selectedIndex, keys.ToArray(), style);
			End.Horizontal();

			return keys.Count == 0
				? ""
				: dictionary[keys[selectedIndex]];
		}

		public static int DropDown(string label, int index, string[] selectOptions, params GUILayoutOption[] options) {
			return EditorGUILayout.Popup(label, index, selectOptions, options);
		}

		public static bool Checkbox(bool value, params GUILayoutOption[] options) {
			Begin.Vertical(SuperFormsStyles.Checkbox);
			value = EditorGUILayout.Toggle(value, options);
			End.Vertical();
			return value;
		}

		public static bool Checkbox(string label, bool value, params GUILayoutOption[] options) {
			Begin.Horizontal();
			Label(label, SuperFormsStyles.BoxedLabel);
			Begin.Vertical(SuperFormsStyles.Checkbox);
			value = EditorGUILayout.Toggle(value, options);
			End.Vertical();
			End.Horizontal();
			return value;
		}

		public static void Texture(Rect space, Texture2D texture) {
			GUI.DrawTexture(space, texture);
		}

		public static void Texture(Rect space, Texture2D texture, bool flipX, bool flipY) {
			if (flipX) {
				space.x = space.x + space.width;
				space.width = -space.width;
			}

			if (flipY) {
				space.y = space.y + space.height;
				space.height = -space.height;
			}

			GUI.DrawTexture(space, texture);
		}

		public static void SpriteImage(Rect position, Sprite sprite) {
			Rect spriteRect = new Rect(
				sprite.rect.x / sprite.texture.width,
				sprite.rect.y / sprite.texture.height,
				sprite.rect.width / sprite.texture.width,
				sprite.rect.height / sprite.texture.height
			);

			GUI.DrawTextureWithTexCoords(position, sprite.texture, spriteRect);
		}

		public static void Texture(Rect position, Sprite sprite) {
			Rect spriteRect = new Rect(
				sprite.rect.x / sprite.texture.width,
				sprite.rect.y / sprite.texture.height,
				sprite.rect.width / sprite.texture.width,
				sprite.rect.height / sprite.texture.height
			);

			Graphics.DrawTexture(position, sprite.texture, spriteRect, 0, 0, 0, 0);
		}

		public static float HorizontalSlider(float value, float min, float max, params GUILayoutOption[] options) {
			return GUILayout.HorizontalSlider(value, min, max, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb,
				options);
		}

		public static float HorizontalSlider(float value, float min, float max, GUIStyle slider, GUIStyle sliderThumb) {
			return GUILayout.HorizontalSlider(value, min, max, slider, sliderThumb);
		}

		public static void Title(string label) {
			Label(label, SuperFormsStyles.TitleStyle);
		}

		public static void BoxHeader(string label, params GUILayoutOption[] options) {
			Label(label, SuperFormsStyles.BoxHeader, options);
		}

		public static void BoxSubHeader(string label, params GUILayoutOption[] options) {
			Label(label, SuperFormsStyles.BoxSubHeader, options);
		}

		public static List<string> ListField(string label, List<string> value) {
			Begin.Horizontal();

			if (label != "") {
				Label(label);
			}

			Begin.Vertical();
			List<string> copy = new List<string>(value);

			for (int i = 0; i < copy.Count; i++) {
				Begin.Horizontal();
				copy[i] = StringField(copy[i]);
				if (IconButton(IconButtons.ButtonDelete)) {
					copy.RemoveAt(i);
				}

				End.Horizontal();
			}

			if (IconButton(IconButtons.ButtonAdd)) {
				copy.Add("");
			}

			End.Vertical();
			End.Horizontal();
			return copy;
		}

		public static List<int> ListField(string label, List<int> value) {
			Begin.Horizontal();

			if (label != "") {
				Label(label);
			}

			Begin.Vertical();
			List<int> copy = new List<int>(value);

			for (int i = 0; i < copy.Count; i++) {
				Begin.Horizontal();
				copy[i] = IntField(copy[i]);
				if (IconButton(IconButtons.ButtonDelete)) {
					copy.RemoveAt(i);
				}

				End.Horizontal();
			}

			if (IconButton(IconButtons.ButtonAdd)) {
				copy.Add(0);
			}

			End.Vertical();
			End.Horizontal();
			return copy;
		}

		public static List<float> ListField(string label, List<float> value) {
			Begin.Horizontal();

			if (label != "") {
				Label(label);
			}

			Begin.Vertical();
			List<float> copy = new List<float>(value);

			for (int i = 0; i < copy.Count; i++) {
				Begin.Horizontal();
				copy[i] = FloatField(copy[i]);
				if (IconButton(IconButtons.ButtonDelete)) {
					copy.RemoveAt(i);
				}

				End.Horizontal();
			}

			if (IconButton(IconButtons.ButtonAdd)) {
				copy.Add(0);
			}

			End.Vertical();
			End.Horizontal();
			return copy;
		}

		public static Vector2 Vector2Field(string label, Vector2 value) {
			Label(label, GUILayout.ExpandWidth(true));
			Begin.Horizontal();
			FlexibleSpace();
			Label("X", GUILayout.Width(20));
			value.x = FloatField(value.x, GUILayout.Width(40));
			Label("Y", GUILayout.Width(20));
			value.y = FloatField(value.y, GUILayout.Width(40));
			End.Horizontal();
			return value;
		}

		public static Vector2 Vector2Field(Vector2 value) {
			Begin.Horizontal();
			FlexibleSpace();
			Label("X", GUILayout.Width(20));
			value.x = FloatField(value.x, GUILayout.Width(40));
			Label("Y", GUILayout.Width(20));
			value.y = FloatField(value.y, GUILayout.Width(40));
			End.Horizontal();
			return value;
		}

		public static Vector2Int Vector2Field(string label, Vector2Int value) {
			Label(label, GUILayout.ExpandWidth(true));
			Begin.Horizontal();
			FlexibleSpace();
			Label("X", GUILayout.Width(20));
			value.x = IntField(value.x, GUILayout.Width(40));
			Label("Y", GUILayout.Width(20));
			value.y = IntField(value.y, GUILayout.Width(40));
			End.Horizontal();
			return value;
		}

		public static Vector2Int Vector2Field(Vector2Int value) {
			Begin.Horizontal();
			FlexibleSpace();
			Label("X", GUILayout.Width(20));
			value.x = IntField(value.x, GUILayout.Width(40));
			Label("Y", GUILayout.Width(20));
			value.y = IntField(value.y, GUILayout.Width(40));
			End.Horizontal();
			return value;
		}

		public static Vector2 Vector2FieldSingleLine(Vector2 value, int width = 25) {
			Begin.Horizontal();
			Label("X", GUILayout.Width(20));
			value.x = FloatField(value.x, GUILayout.Width(width));
			Label("Y", GUILayout.Width(20));
			value.y = FloatField(value.y, GUILayout.Width(width));
			End.Horizontal();
			return value;
		}

		public static Vector2Int Vector2FieldSingleLine(Vector2Int value, int width = 25) {
			Begin.Horizontal();
			Label("X", GUILayout.Width(20));
			value.x = IntField(value.x, GUILayout.Width(width));
			Label("Y", GUILayout.Width(20));
			value.y = IntField(value.y, GUILayout.Width(width));
			End.Horizontal();
			return value;
		}
		
		public static Vector2 Vector2FieldSingleLine(string label, Vector2 value, int width = 25) {
			Begin.Horizontal();
			Label(label, GUILayout.ExpandWidth(true));
			FlexibleSpace();
			Label("X", GUILayout.Width(20));
			value.x = FloatField(value.x, GUILayout.Width(width));
			Label("Y", GUILayout.Width(20));
			value.y = FloatField(value.y, GUILayout.Width(width));
			End.Horizontal();
			return value;
		}

		public static Vector2Int Vector2FieldSingleLine(string label, Vector2Int value, int width = 25) {
			Begin.Horizontal();
			Label(label, GUILayout.ExpandWidth(true));
			FlexibleSpace();
			Label("X", GUILayout.Width(20));
			value.x = IntField(value.x, GUILayout.Width(width));
			Label("Y", GUILayout.Width(20));
			value.y = IntField(value.y, GUILayout.Width(width));
			End.Horizontal();
			return value;
		}

		public static Vector2 Vector3Field(string label, Vector3 value) {
			Label(label, GUILayout.ExpandWidth(true));
			Begin.Horizontal();
			FlexibleSpace();
			Label("X", GUILayout.Width(20));
			value.x = FloatField(value.x, GUILayout.Width(40));
			Label("Y", GUILayout.Width(20));
			value.y = FloatField(value.y, GUILayout.Width(40));
			Label("Z", GUILayout.Width(20));
			value.z = FloatField(value.z, GUILayout.Width(40));
			End.Horizontal();
			return value;
		}

		public static Vector2 Vector3Field(Vector3 value) {
			Begin.Horizontal();
			FlexibleSpace();
			Label("X", GUILayout.Width(20));
			value.x = FloatField(value.x, GUILayout.Width(40));
			Label("Y", GUILayout.Width(20));
			value.y = FloatField(value.y, GUILayout.Width(40));
			Label("Z", GUILayout.Width(20));
			value.z = FloatField(value.z, GUILayout.Width(40));
			End.Horizontal();
			return value;
		}

		public static Vector2 Vector4Field(string label, Vector4 value) {
			Label(label, GUILayout.ExpandWidth(true));
			Begin.Horizontal();
			FlexibleSpace();
			Label("W", GUILayout.Width(20));
			value.w = FloatField(value.w, GUILayout.Width(40));
			Label("X", GUILayout.Width(20));
			value.x = FloatField(value.x, GUILayout.Width(40));
			Label("Y", GUILayout.Width(20));
			value.y = FloatField(value.y, GUILayout.Width(40));
			Label("Z", GUILayout.Width(20));
			value.z = FloatField(value.z, GUILayout.Width(40));
			End.Horizontal();
			return value;
		}

		public static Vector2 Vector4Field(Vector4 value) {
			Begin.Horizontal();
			FlexibleSpace();
			Label("W", GUILayout.Width(20));
			value.w = FloatField(value.w, GUILayout.Width(40));
			Label("X", GUILayout.Width(20));
			value.x = FloatField(value.x, GUILayout.Width(40));
			Label("Y", GUILayout.Width(20));
			value.y = FloatField(value.y, GUILayout.Width(40));
			Label("Z", GUILayout.Width(20));
			value.z = FloatField(value.z, GUILayout.Width(40));
			End.Horizontal();
			return value;
		}

		public static Color ColorPicker(Color color) {
			return EditorGUILayout.ColorField(color);
		}

		public static Color ColorPicker(string label, Color color) {
			Begin.Horizontal();
			Label(label);
			color = EditorGUILayout.ColorField(color);
			End.Horizontal();
			return color;
		}
	}

	public class AreasOpeners {
		public void Area(Rect space) {
			GUILayout.BeginArea(space);
		}

		public void Area(Rect space, GUIStyle style) {
			GUILayout.BeginArea(space, style);
		}

		public void MainArea(Rect space) {
			Area(space, SuperFormsStyles.MainAreaStyle);
		}

		public void PaddedArea(Rect space) {
			Area(space, SuperFormsStyles.PaddedArea);
		}

		public void MainOptionBarInline() {
			Horizontal(SuperFormsStyles.MainOptionBar);
		}

		public void Horizontal() {
			GUILayout.BeginHorizontal();
		}

		public void HorizontalHighlighted() {
			GUILayout.BeginHorizontal(SuperFormsStyles.Button);
		}

		public void HorizontalBox(params GUILayoutOption[] options) {
			GUILayout.BeginHorizontal(SuperFormsStyles.BoxStyle, options);
		}

		public void Horizontal(params GUILayoutOption[] options) {
			GUILayout.BeginHorizontal(options);
		}

		public void Horizontal(GUIStyle style, params GUILayoutOption[] options) {
			GUILayout.BeginHorizontal(style, options);
		}

		public void Vertical() {
			GUILayout.BeginVertical();
		}

		public void Vertical(params GUILayoutOption[] options) {
			GUILayout.BeginVertical(options);
		}

		public void VerticalBox() {
			Vertical(SuperFormsStyles.BoxStyle);
		}

		public void VerticalBox(params GUILayoutOption[] options) {
			GUILayout.BeginVertical(SuperFormsStyles.BoxStyle, options);
		}

		public void PlateBox(params GUILayoutOption[] options) {
			GUILayout.BeginVertical(SuperFormsStyles.PlateBoxStyle, options);
		}

		public void PlateBoxHighlighted(params GUILayoutOption[] options) {
			GUILayout.BeginVertical(SuperFormsStyles.PlateBoxHighlightedStyle, options);
		}

		public void PlateBoxSelected(params GUILayoutOption[] options) {
			GUILayout.BeginVertical(SuperFormsStyles.PlateBoxSelectedStyle, options);
		}

		public void VerticalPaddedBox(params GUILayoutOption[] options) {
			GUILayout.BeginVertical(SuperFormsStyles.PaddedBoxStyle, options);
		}

		public void VerticalSubBox(params GUILayoutOption[] options) {
			GUILayout.BeginVertical(SuperFormsStyles.SubBoxStyle, options);
		}

		public void VerticalIndentedBox(params GUILayoutOption[] options) {
			GUILayout.BeginVertical(SuperFormsStyles.IndentedBox, options);
		}

		public void Vertical(GUIStyle style) {
			GUILayout.BeginVertical(style);
		}

		public Vector2 Scroll(Vector2 scrollPosition, params GUILayoutOption[] options) {
			return GUILayout.BeginScrollView(scrollPosition, options);
		}

		public Vector2 Scroll(Vector2 scrollPosition, GUIStyle style) {
			return GUILayout.BeginScrollView(scrollPosition, style);
		}

		public Vector2 Scroll(Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options) {
			return GUILayout.BeginScrollView(scrollPosition, style, options);
		}
	}

	public class AreasClosers {
		public void Area() {
			GUILayout.EndArea();
		}

		public void Horizontal() {
			GUILayout.EndHorizontal();
		}

		public void MainOptionBarInline() {
			GUILayout.EndHorizontal();
		}

		public void Vertical() {
			GUILayout.EndHorizontal();
		}

		public void Scroll() {
			GUILayout.EndScrollView();
		}
	}

	public class Regions {
		public void MainOptionBarInline(Action action) {
			GUILayout.BeginHorizontal(SuperFormsStyles.MainOptionBar);
			action();
			GUILayout.EndHorizontal();
		}

		public void MainArea(Rect space, Action action) {
			GUILayout.BeginArea(space, SuperFormsStyles.MainAreaStyle);
			action();
			GUILayout.EndArea();
		}

		public void Area(Rect space, Action action, params GUILayoutOption[] options) {
			GUILayout.BeginArea(space);
			action();
			GUILayout.EndArea();
		}

		public Vector2 Scroll(string scrollId, Action action, params GUILayoutOption[] options) {
			Vector2 position = Vector2.zero;

			if (SuperForms.ScrollPositions.ContainsKey(scrollId)) {
				position = SuperForms.ScrollPositions[scrollId];
			} else {
				SuperForms.ScrollPositions.Add(scrollId, Vector2.zero);
			}

			position = GUILayout.BeginScrollView(position, options);
			action();
			GUILayout.EndScrollView();
			SuperForms.ScrollPositions[scrollId] = position;
			return position;
		}

		public Vector2 Scroll(string scrollId, Action action, GUIStyle style, params GUILayoutOption[] options) {
			Vector2 position = Vector2.zero;

			if (SuperForms.ScrollPositions.ContainsKey(scrollId)) {
				position = SuperForms.ScrollPositions[scrollId];
			} else {
				SuperForms.ScrollPositions.Add(scrollId, Vector2.zero);
			}

			position = GUILayout.BeginScrollView(position, style, options);
			action();
			GUILayout.EndScrollView();
			SuperForms.ScrollPositions[scrollId] = position;
			return position;
		}

		public Vector2 ScrollDisabledWheel(Vector2 position, Action action, params GUILayoutOption[] options) {
			position = GUILayout.BeginScrollView(position, options);
			action();
			GUILayout.EndVertical();
			GUI.EndScrollView(false);
			return position;
		}

		public void Horizontal(Action action, params GUILayoutOption[] options) {
			GUILayout.BeginHorizontal(options);
			action();
			GUILayout.EndHorizontal();
		}

		public void Horizontal(Action action, GUIStyle style, params GUILayoutOption[] options) {
			GUILayout.BeginHorizontal(style, options);
			action();
			GUILayout.EndHorizontal();
		}

		public void Vertical(Action action, params GUILayoutOption[] options) {
			GUILayout.BeginVertical(options);
			action();
			GUILayout.EndVertical();
		}

		public void Vertical(Action action, GUIStyle style, params GUILayoutOption[] options) {
			GUILayout.BeginVertical(style, options);
			action();
			GUILayout.EndVertical();
		}


		public void HorizontalBox(Action action, params GUILayoutOption[] options) {
			GUILayout.BeginHorizontal(SuperFormsStyles.BoxStyle, options);
			action();
			GUILayout.EndHorizontal();
		}

		public void VerticalBox(Action action, params GUILayoutOption[] options) {
			GUILayout.BeginVertical(SuperFormsStyles.BoxStyle, options);
			action();
			GUILayout.EndVertical();
		}
		

		public void VerticalBox(Action action, GUIStyle style, params GUILayoutOption[] options) {
			GUILayout.BeginVertical(new GUIStyle(style), options);
			action();
			GUILayout.EndVertical();
		}
		
		public void VerticalSubBox(Action action, params GUILayoutOption[] options) {
			GUILayout.BeginVertical(SuperFormsStyles.SubBoxStyle, options);
			action();
			GUILayout.EndVertical();
		}

		private static readonly Dictionary<string, Rect> AreaRects = new Dictionary<string, Rect>();
#endif
	}
}