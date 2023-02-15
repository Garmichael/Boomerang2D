using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Boomerang2DFramework.Framework.Actors;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.EditorHelpers {
	public static class FieldBasedForms {
#if UNITY_EDITOR

		[Serializable]
		public struct CompareFloat {
			public ValueComparison Comparison;
			public float Value;
		}

		public static object DrawFormField(FieldInfo field, string label, object value) {
			object[] spacerAttributes = field.GetCustomAttributes(typeof(SpaceAttribute), false);

			if (spacerAttributes.Length > 0) {
				foreach (SpaceAttribute unused in spacerAttributes) {
					SuperForms.Space(20);
				}
			}

			object[] headerAttributes = field.GetCustomAttributes(typeof(HeaderAttribute), false);

			if (headerAttributes.Length > 0) {
				foreach (HeaderAttribute attribute in headerAttributes) {
					SuperForms.Space(20);
					SuperForms.BoxSubHeader(attribute.header);
				}
			}

			return DrawFormField(label, value);
		}

		public static object DrawFormField(object value) {
			return DrawFormField("", value);
		}

		public static object DrawFormField(string label, [CanBeNull] object value, params GUILayoutOption[] options) {
			label = Regex.Replace(label, "(\\B[A-Z])", " $1");

			if (options == null) {
				options = new GUILayoutOption[0];
			}

			if (value is int integer) {
				return label.Trim() == ""
					? SuperForms.IntField(integer, options)
					: SuperForms.IntField(label, integer, options);
			}

			if (value is long longInteger) {
				return label.Trim() == ""
					? (long) EditorGUILayout.IntField((int) longInteger, options)
					: (long) EditorGUILayout.IntField(label, (int) longInteger, options);
			}

			if (value is float f) {
				return label.Trim() == ""
					? SuperForms.FloatField(f, options)
					: SuperForms.FloatField(label, f, options);
			}

			if (value is double d) {
				return label.Trim() == ""
					? (double) SuperForms.FloatField((float) d, options)
					: (double) SuperForms.FloatField(label, (float) d, options);
			}

			if (value is bool b) {
				return label.Trim() == ""
					? SuperForms.Checkbox(b, options)
					: SuperForms.Checkbox(label, b, options);
			}

			if (value is string s) {
				return label.Trim() == ""
					? SuperForms.StringField(s)
					: SuperForms.StringField(label, s);
			}

			if (value is Vector2 vector2) {
				return label.Trim() == ""
					? SuperForms.Vector2Field(vector2)
					: SuperForms.Vector2Field(label, vector2);
			}

			if (value is Vector2Int vector2Int) {
				return label.Trim() == ""
					? SuperForms.Vector2FieldSingleLine("", vector2Int)
					: SuperForms.Vector2FieldSingleLine(label, vector2Int);
			}

			if (value is Vector3 vector3) {
				return label.Trim() == ""
					? SuperForms.Vector3Field(vector3)
					: SuperForms.Vector3Field(label, vector3);
			}

			if (value is Vector4 vector4) {
				return label.Trim() == ""
					? SuperForms.Vector4Field(vector4)
					: SuperForms.Vector4Field(label, vector4);
			}

			if (value != null && value.GetType().IsEnum) {
				return label.Trim() == ""
					? SuperForms.EnumDropdown((Enum) value)
					: SuperForms.EnumDropdown(label, (Enum) value);
			}

			if (value is LayerMask mask) {
				return label.Trim() == ""
					? (LayerMask) EditorGUILayout.LayerField(mask, options)
					: (LayerMask) EditorGUILayout.LayerField(label, mask, options);
			}

			if (value is List<string> stringList) {
				SuperForms.Begin.Horizontal();
				if (label.Trim() != "") {
					SuperForms.Label(label);
				}

				SuperForms.Begin.Vertical();
				List<string> copy = new List<string>(stringList);

				for (int i = 0; i < copy.Count; i++) {
					SuperForms.Begin.Horizontal();
					copy[i] = SuperForms.StringField(copy[i]);
					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						copy.RemoveAt(i);
					}

					SuperForms.End.Horizontal();
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					copy.Add("");
				}

				SuperForms.End.Vertical();
				SuperForms.End.Horizontal();
				return copy;
			}

			if (value is List<int> intList) {
				SuperForms.Begin.Horizontal();
				if (label.Trim() != "") {
					SuperForms.Label(label);
				}

				SuperForms.Begin.Vertical();
				List<int> copy = new List<int>(intList);

				for (int i = 0; i < copy.Count; i++) {
					SuperForms.Begin.Horizontal();
					copy[i] = SuperForms.IntField(copy[i]);
					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						copy.RemoveAt(i);
					}

					SuperForms.End.Horizontal();
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					copy.Add(0);
				}

				SuperForms.End.Vertical();
				SuperForms.End.Horizontal();
				return copy;
			}

			if (value is List<float> floatList) {
				SuperForms.Begin.Horizontal();
				if (label.Trim() != "") {
					SuperForms.Label(label);
				}

				SuperForms.Begin.Vertical();
				List<float> copy = new List<float>(floatList);

				for (int i = 0; i < copy.Count; i++) {
					SuperForms.Begin.Horizontal();
					copy[i] = SuperForms.FloatField(copy[i]);
					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						copy.RemoveAt(i);
					}

					SuperForms.End.Horizontal();
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					copy.Add(0);
				}

				SuperForms.End.Vertical();
				SuperForms.End.Horizontal();
				return copy;
			}

			if (value is List<Directions> directionList) {
				SuperForms.Begin.Horizontal();
				if (label.Trim() != "") {
					SuperForms.Label(label);
				}

				SuperForms.Begin.Vertical();
				List<Directions> copy = new List<Directions>(directionList);

				for (int i = 0; i < copy.Count; i++) {
					SuperForms.Begin.Horizontal();
					copy[i] = (Directions) SuperForms.EnumDropdown(copy[i]);

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						copy.RemoveAt(i);
					}

					SuperForms.End.Horizontal();
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					copy.Add(Directions.Up);
				}

				SuperForms.End.Vertical();
				SuperForms.End.Horizontal();
				return copy;
			}

			if (value is List<CompareFloat> compareFloatList) {
				if (label.Trim() != "") {
					SuperForms.Label(label);
				}

				SuperForms.Begin.Horizontal();

				SuperForms.Begin.Vertical();
				List<CompareFloat> copy = new List<CompareFloat>(compareFloatList);

				for (int i = 0; i < copy.Count; i++) {
					SuperForms.Begin.Horizontal();
					CompareFloat blah = copy[i];
					blah.Comparison = (ValueComparison) SuperForms.EnumDropdown(blah.Comparison);
					blah.Value = SuperForms.FloatField(blah.Value);

					copy[i] = new CompareFloat {
						Comparison = blah.Comparison,
						Value = blah.Value
					};

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						copy.RemoveAt(i);
					}

					SuperForms.End.Horizontal();
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					copy.Add(new CompareFloat());
				}

				SuperForms.End.Vertical();
				SuperForms.End.Horizontal();
				return copy;
			}

			if (value is SerializableStringDictionary serializableStringDictionary) {
				if (serializableStringDictionary.Keys == null || serializableStringDictionary.Values == null) {
					serializableStringDictionary.Keys = new List<string>();
					serializableStringDictionary.Values = new List<string>();
				}

				SuperForms.Begin.Horizontal();
				if (label.Trim() != "") {
					SuperForms.Label(label);
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					serializableStringDictionary.Keys.Add("");
					serializableStringDictionary.Values.Add("");
				}

				SuperForms.End.Horizontal();

				SuperForms.Begin.Horizontal();
				SuperForms.Begin.VerticalIndentedBox();


				if (serializableStringDictionary.Keys.Count != serializableStringDictionary.Values.Count) {
					return new SerializableStringDictionary();
				}

				int deleteIndex = -1;

				for (int i = 0; i < serializableStringDictionary.Keys.Count; i++) {
					SuperForms.Begin.Horizontal();
					serializableStringDictionary.Keys[i] = SuperForms.StringField(serializableStringDictionary.Keys[i]);
					serializableStringDictionary.Values[i] =
						SuperForms.StringField(serializableStringDictionary.Values[i]);

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						deleteIndex = i;
					}

					SuperForms.End.Horizontal();
				}

				if (deleteIndex > -1) {
					serializableStringDictionary.Keys.RemoveAt(deleteIndex);
					serializableStringDictionary.Values.RemoveAt(deleteIndex);
				}

				SuperForms.End.Vertical();
				SuperForms.End.Horizontal();

				return serializableStringDictionary;
			}

			if (value is SerializableFloatDictionary serializableFloatDictionary) {
				if (serializableFloatDictionary.Keys == null || serializableFloatDictionary.Values == null) {
					serializableFloatDictionary.Keys = new List<string>();
					serializableFloatDictionary.Values = new List<float>();
				}

				SuperForms.Begin.Horizontal();
				if (label.Trim() != "") {
					SuperForms.Label(label);
				}


				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					serializableFloatDictionary.Keys.Add("");
					serializableFloatDictionary.Values.Add(0f);
				}

				SuperForms.End.Horizontal();

				SuperForms.Begin.Horizontal();
				SuperForms.Begin.VerticalIndentedBox();

				if (serializableFloatDictionary.Keys.Count != serializableFloatDictionary.Values.Count) {
					return new SerializableStringDictionary();
				}

				int deleteIndex = -1;

				for (int i = 0; i < serializableFloatDictionary.Keys.Count; i++) {
					SuperForms.Begin.Horizontal();
					serializableFloatDictionary.Keys[i] = SuperForms.StringField(serializableFloatDictionary.Keys[i]);
					serializableFloatDictionary.Values[i] =
						SuperForms.FloatField(serializableFloatDictionary.Values[i]);

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						deleteIndex = i;
					}

					SuperForms.End.Horizontal();
				}

				if (deleteIndex > -1) {
					serializableFloatDictionary.Keys.RemoveAt(deleteIndex);
					serializableFloatDictionary.Values.RemoveAt(deleteIndex);
				}

				SuperForms.End.Vertical();
				SuperForms.End.Horizontal();

				return serializableFloatDictionary;
			}

			if (value is SerializableBoolDictionary serializableBoolDictionary) {
				if (serializableBoolDictionary.Keys == null || serializableBoolDictionary.Values == null) {
					serializableBoolDictionary.Keys = new List<string>();
					serializableBoolDictionary.Values = new List<bool>();
				}

				SuperForms.Begin.Horizontal();
				if (label.Trim() != "") {
					SuperForms.Label(label);
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					serializableBoolDictionary.Keys.Add("");
					serializableBoolDictionary.Values.Add(false);
				}

				SuperForms.End.Horizontal();

				SuperForms.Begin.Horizontal();
				SuperForms.Begin.VerticalIndentedBox();

				if (serializableBoolDictionary.Keys.Count != serializableBoolDictionary.Values.Count) {
					return new SerializableStringDictionary();
				}

				int deleteIndex = -1;

				for (int i = 0; i < serializableBoolDictionary.Keys.Count; i++) {
					SuperForms.Begin.Horizontal();
					serializableBoolDictionary.Keys[i] = SuperForms.StringField(serializableBoolDictionary.Keys[i]);
					serializableBoolDictionary.Values[i] = SuperForms.Checkbox(serializableBoolDictionary.Values[i]);

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						deleteIndex = i;
					}

					SuperForms.End.Horizontal();
				}

				if (deleteIndex > -1) {
					serializableBoolDictionary.Keys.RemoveAt(deleteIndex);
					serializableBoolDictionary.Values.RemoveAt(deleteIndex);
				}

				SuperForms.End.Vertical();
				SuperForms.End.Horizontal();

				return serializableBoolDictionary;
			}

			if (value is SerializableIntDictionary serializableIntDictionary) {
				if (serializableIntDictionary.Keys == null || serializableIntDictionary.Values == null) {
					serializableIntDictionary.Keys = new List<string>();
					serializableIntDictionary.Values = new List<int>();
				}

				SuperForms.Begin.Horizontal();
				if (label.Trim() != "") {
					SuperForms.Label(label);
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					serializableIntDictionary.Keys.Add("");
					serializableIntDictionary.Values.Add(0);
				}

				SuperForms.End.Horizontal();

				SuperForms.Begin.Horizontal();
				SuperForms.Begin.VerticalIndentedBox();


				if (serializableIntDictionary.Keys.Count != serializableIntDictionary.Values.Count) {
					return new SerializableStringDictionary();
				}

				int deleteIndex = -1;

				for (int i = 0; i < serializableIntDictionary.Keys.Count; i++) {
					SuperForms.Begin.Horizontal();
					serializableIntDictionary.Keys[i] = SuperForms.StringField(serializableIntDictionary.Keys[i]);
					serializableIntDictionary.Values[i] = SuperForms.IntField(serializableIntDictionary.Values[i]);

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						deleteIndex = i;
					}

					SuperForms.End.Horizontal();
				}

				if (deleteIndex > -1) {
					serializableIntDictionary.Keys.RemoveAt(deleteIndex);
					serializableIntDictionary.Values.RemoveAt(deleteIndex);
				}

				SuperForms.End.Vertical();
				SuperForms.End.Horizontal();

				return serializableIntDictionary;
			}

			if (value is SerializableColorDictionary serializableColorDictionary) {
				if (serializableColorDictionary.Keys == null || serializableColorDictionary.Values == null) {
					serializableColorDictionary.Keys = new List<string>();
					serializableColorDictionary.Values = new List<Color>();
				}

				SuperForms.Begin.Horizontal();
				if (label.Trim() != "") {
					SuperForms.Label(label);
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					serializableColorDictionary.Keys.Add("");
					serializableColorDictionary.Values.Add(Color.white);
				}

				SuperForms.End.Horizontal();

				SuperForms.Begin.Horizontal();
				SuperForms.Begin.VerticalIndentedBox();


				if (serializableColorDictionary.Keys.Count != serializableColorDictionary.Values.Count) {
					return new SerializableStringDictionary();
				}

				int deleteIndex = -1;

				for (int i = 0; i < serializableColorDictionary.Keys.Count; i++) {
					SuperForms.Begin.Horizontal();
					serializableColorDictionary.Keys[i] = SuperForms.StringField(serializableColorDictionary.Keys[i]);
					serializableColorDictionary.Values[i] =
						SuperForms.ColorPicker(serializableColorDictionary.Values[i]);

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						deleteIndex = i;
					}

					SuperForms.End.Horizontal();
				}

				if (deleteIndex > -1) {
					serializableColorDictionary.Keys.RemoveAt(deleteIndex);
					serializableColorDictionary.Values.RemoveAt(deleteIndex);
				}

				SuperForms.End.Vertical();
				SuperForms.End.Horizontal();

				return serializableColorDictionary;
			}

			if (value is ActorFloatValueConstructor outputValue) {
				FloatValueConstructorValue[] valuesThatUseFloatSubValue = {FloatValueConstructorValue.Float};

				FloatValueConstructorValue[] valuesThatUseStringSubValue = {
					FloatValueConstructorValue.StatValue,
					FloatValueConstructorValue.PlayerStatValue,
					FloatValueConstructorValue.GameFlagValue,
				};
				FloatValueConstructorValue[] valuesThatUseMinMax = {
					FloatValueConstructorValue.RandomValue,
					FloatValueConstructorValue.RandomInt,
				};

				if (outputValue.Entries == null) {
					outputValue.Entries = new List<FloatValueConstructorEntry>();
				}

				SuperForms.Region.Horizontal(() => {
					outputValue.StartValue = SuperForms.FloatField(label, outputValue.StartValue);
					SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd,
						() => { outputValue.Entries.Add(new FloatValueConstructorEntry()); });
				});

				if (outputValue.Entries.Count > 0) {
					SuperForms.Begin.VerticalIndentedBox();

					FloatValueConstructorEntry toDelete = null;

					foreach (FloatValueConstructorEntry entryGroup in outputValue.Entries) {
						SuperForms.Region.Horizontal(() => {
							entryGroup.Operation =
								(FloatValueConstructorOperation) SuperForms.EnumDropdown(entryGroup.Operation,
									GUILayout.Width(60));
							entryGroup.Value = (FloatValueConstructorValue) SuperForms.EnumDropdown(entryGroup.Value);
							entryGroup.UsesSubValueFloat =
								Array.IndexOf(valuesThatUseFloatSubValue, entryGroup.Value) > -1;
							entryGroup.UsesSubValueString =
								Array.IndexOf(valuesThatUseStringSubValue, entryGroup.Value) > -1;
							entryGroup.UsesSubValueMinMax =
								Array.IndexOf(valuesThatUseMinMax, entryGroup.Value) > -1;

							if (entryGroup.UsesSubValueFloat) {
								entryGroup.SubValueFloat = SuperForms.FloatField(entryGroup.SubValueFloat);
							}

							if (entryGroup.UsesSubValueString) {
								entryGroup.SubValueString = SuperForms.StringField(entryGroup.SubValueString);
							}

							if (entryGroup.UsesSubValueMinMax) {
								entryGroup.SubValueMin = SuperForms.FloatField(entryGroup.SubValueMin);
								entryGroup.SubValueMax = SuperForms.FloatField(entryGroup.SubValueMax);
							}

							SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete,
								() => { toDelete = entryGroup; });
						});
					}

					if (toDelete != null) {
						outputValue.Entries.Remove(toDelete);
					}

					if (outputValue.Entries.Any()) {
						SuperForms.Begin.Horizontal();
						SuperForms.Label("Clamp", GUILayout.Width(80));
						outputValue.ClampMin = SuperForms.Checkbox(outputValue.ClampMin);
						SuperForms.Label("Min", GUILayout.Width(40));
						outputValue.ClampMax = SuperForms.Checkbox(outputValue.ClampMax);
						SuperForms.Label("Max", GUILayout.Width(40));
						SuperForms.End.Horizontal();

						if (outputValue.ClampMin || outputValue.ClampMax) {
							SuperForms.Begin.Horizontal();
							SuperForms.Label("Clamp Values", GUILayout.Width(80));

							if (outputValue.ClampMin) {
								outputValue.ClampMinValue = SuperForms.FloatField(outputValue.ClampMinValue);
							}

							if (outputValue.ClampMax) {
								outputValue.ClampMaxValue = SuperForms.FloatField(outputValue.ClampMaxValue);
							}

							SuperForms.End.Horizontal();
						}

						SuperForms.Space();
					}

					SuperForms.End.Vertical();
				}

				return outputValue;
			}

			return value;
		}
#endif
	}
}