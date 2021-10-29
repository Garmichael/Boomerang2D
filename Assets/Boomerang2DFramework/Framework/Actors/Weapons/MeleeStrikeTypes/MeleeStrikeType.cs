using System;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.Weapons.MeleeStrikeTypes {
	/// <summary>
	/// Base Class for Melee Strikes
	/// </summary>
	public abstract class MeleeStrikeType {
		private MeleeStrikeProperties _properties;
		protected PolygonCollider2D MyCollider;
		public bool IsEnabled = false;

		protected void Instantiate(MeleeStrikeProperties properties, WeaponBehavior parentBehavior = null) {
			SetProperties(properties);

			if (parentBehavior != null) {
				SetParent(parentBehavior);
			}
		}

		/// <summary>
		/// Returns a list of points that define the collider's shape.
		/// </summary>
		/// <returns></returns>
		public virtual Vector2[] GetColliderPoints() {
			return new Vector2[0];
		}

		private void SetParent(WeaponBehavior parentBehavior) {
			MyCollider = parentBehavior.gameObject.AddComponent<PolygonCollider2D>();
			MyCollider.enabled = false;
		}

		private void SetProperties(MeleeStrikeProperties properties) {
			_properties = properties;
			Type strikePropertiesType = Type.GetType(_properties.StrikeTypePropertiesClass);

			if (strikePropertiesType != null) {
				foreach (MeleeStrikeAction action in _properties.Actions) {
					action.StartState = JsonUtility.FromJson(action.StartStateProperties, strikePropertiesType);
					action.EndState = JsonUtility.FromJson(action.EndStateProperties, strikePropertiesType);
				}
			}
		}

		protected struct CurrentActionProperties {
			public MeleeStrikeAction CurrentAction;
			public float StartTimeOfCurrentAction;
		}

		protected CurrentActionProperties GetCurrentActionProperties(float currentMomentOfStrike) {
			MeleeStrikeAction currentAction = null;
			float accumulatedActionTime = _properties.StartTime;
			float startTimeOfCurrentAction = 0;

			foreach (MeleeStrikeAction action in _properties.Actions) {
				startTimeOfCurrentAction = accumulatedActionTime;
				accumulatedActionTime += action.Duration;

				if (currentMomentOfStrike > accumulatedActionTime) {
					continue;
				}

				currentAction = action;
				break;
			}

			return new CurrentActionProperties {
				CurrentAction = currentAction,
				StartTimeOfCurrentAction = startTimeOfCurrentAction
			};
		}

		/// <summary>
		/// Main logic of a weapon strike
		/// </summary>
		/// <param name="currentMomentOfStrike"></param>
		public virtual void Animate(float currentMomentOfStrike) { }

		public void EndAnimation() {
			Disable();
		}

		protected bool ShouldEnable(float currentMomentOfStrike) {
			return currentMomentOfStrike > _properties.StartTime &&
			       currentMomentOfStrike < _properties.StartTime + _properties.TotalTime;
		}

		protected void Enable() {
			IsEnabled = true;
			
			if (MyCollider && !MyCollider.enabled) {
				MyCollider.enabled = true;
			}
		}

		protected void Disable() {
			IsEnabled = false;

			if (MyCollider && MyCollider.enabled) {
				MyCollider.enabled = false;
			}
		}
	}
}