using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.GameFlagManagement;
using Random = UnityEngine.Random;

namespace Boomerang2DFramework.Framework.Actors.ActorFloatValues {
	/// <summary>
	/// Actor Float Value Constructors are used to generate a value each frame based on its inputs and mathmatical
	/// operations. 
	/// </summary>
	/// <remarks>
	/// The editor shows these as normal number input fields with + signs that let you add rows to them.
	/// </remarks>
	[Serializable]
	public class ActorFloatValueConstructor {
		public float StartValue;
		public List<FloatValueConstructorEntry> Entries;

		public bool ClampMin;
		public float ClampMinValue;
		public bool ClampMax;
		public float ClampMaxValue;

		/// <summary>
		/// Constructs the value from the inputs
		/// </summary>
		/// <param name="actor">The Actor who owns this property</param>
		/// <returns>The built value</returns>
		/// <exception cref="ArgumentOutOfRangeException">On Invalid Mathmatical operation</exception>
		public float BuildValue(Actor actor) {
			float outputValue = StartValue;

			foreach (FloatValueConstructorEntry entry in Entries) {
				float value = 0;
				switch (entry.Value) {
					case FloatValueConstructorValue.Float:
						value = entry.SubValueFloat;
						break;
					case FloatValueConstructorValue.StatValue: {
						foreach (FloatStatProperties stat in actor.ActorProperties.StatsFloats) {
							if (stat.Name == entry.SubValueString) {
								value = stat.Value;
								break;
							}
						}

						break;
					}
					case FloatValueConstructorValue.PositionX:
						value += actor.RealPosition.x;
						break;
					case FloatValueConstructorValue.PositionY:
						value += actor.RealPosition.y;
						break;
					case FloatValueConstructorValue.VelocityX:
						value += actor.Velocity.x;
						break;
					case FloatValueConstructorValue.VelocityY:
						value += actor.Velocity.y;
						break;
					case FloatValueConstructorValue.PlayerStatValue: {
						if (Boomerang2D.Player != null) {
							foreach (FloatStatProperties stat in Boomerang2D.Player.ActorProperties.StatsFloats) {
								if (stat.Name == entry.SubValueString) {
									value = stat.Value;
									break;
								}
							}
						}

						break;
					}
					case FloatValueConstructorValue.PlayerPositionX:
						if (Boomerang2D.Player != null) {
							value += Boomerang2D.Player.Transform.localPosition.x;
						}

						break;
					case FloatValueConstructorValue.PlayerPositionY:
						if (Boomerang2D.Player != null) {
							value += Boomerang2D.Player.Transform.localPosition.y;
						}

						break;
					case FloatValueConstructorValue.PlayerVelocityX:
						if (Boomerang2D.Player != null) {
							value += Boomerang2D.Player.Velocity.x;
						}

						break;
					case FloatValueConstructorValue.PlayerVelocityY:
						if (Boomerang2D.Player != null) {
							value += Boomerang2D.Player.Velocity.y;
						}

						break;
					case FloatValueConstructorValue.GameFlagValue:
						value += GameFlags.GetFloatFlag(entry.SubValueString);
						break;
					case FloatValueConstructorValue.DistanceToSolidUp:
						value += actor.DistanceToSolidUp;
						break;
					case FloatValueConstructorValue.DistanceToSolidRight:
						value += actor.DistanceToSolidRight;
						break;
					case FloatValueConstructorValue.DistanceToSolidDown:
						value += actor.DistanceToSolidDown;
						break;
					case FloatValueConstructorValue.DistanceToSolidLeft:
						value += actor.DistanceToSolidLeft;
						break;
					case FloatValueConstructorValue.RandomValue:
						value += Random.Range(entry.SubValueMin, entry.SubValueMax);
						break;
					case FloatValueConstructorValue.RandomInt:
						value += (float) Math.Round(Random.Range(entry.SubValueMin, entry.SubValueMax), MidpointRounding.AwayFromZero);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				switch (entry.Operation) {
					case FloatValueConstructorOperation.Add:
						outputValue += value;
						break;
					case FloatValueConstructorOperation.Minus:
						outputValue -= value;
						break;
					case FloatValueConstructorOperation.Multiply:
						outputValue *= value;
						break;
					case FloatValueConstructorOperation.Divide:
						outputValue /= value;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if (Entries.Any() && ClampMax) {
				if (outputValue > ClampMaxValue) {
					outputValue = ClampMaxValue;
				}
			}

			if (Entries.Any() && ClampMin) {
				if (outputValue < ClampMinValue) {
					outputValue = ClampMinValue;
				}
			}

			return outputValue;
		}
	}

	[Serializable]
	public class FloatValueConstructorEntry {
		public FloatValueConstructorOperation Operation;
		public FloatValueConstructorValue Value;

		public bool UsesSubValueFloat;
		public float SubValueFloat;

		public bool UsesSubValueMinMax;
		public float SubValueMin;
		public float SubValueMax;

		public bool UsesSubValueString;
		public string SubValueString;
	}

	public enum FloatValueConstructorOperation {
		Add,
		Minus,
		Multiply,
		Divide
	}

	public enum FloatValueConstructorValue {
		Float,
		StatValue,
		PositionX,
		PositionY,
		VelocityX,
		VelocityY,
		PlayerStatValue,
		PlayerPositionX,
		PlayerPositionY,
		PlayerVelocityX,
		PlayerVelocityY,
		GameFlagValue,
		DistanceToSolidUp,
		DistanceToSolidRight,
		DistanceToSolidDown,
		DistanceToSolidLeft,
		RandomValue,
		RandomInt
	}
}