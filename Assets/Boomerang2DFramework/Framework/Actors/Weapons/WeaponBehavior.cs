using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.Weapons.MeleeStrikeTypes;
using Boomerang2DFramework.Framework.GlobalTimeManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.Weapons {
	/// <summary>
	/// Monobehavior that's added to a GameObject when the Weapon is built. It controls the timing and execution
	/// of Weapons. 
	/// </summary>
	public class WeaponBehavior : MonoBehaviour {
		public bool IsActing;
		private float _totalWeaponDuration;
		private float _currentTime;

		private readonly List<MeleeStrikeType> _builtMeleeStrikes = new List<MeleeStrikeType>();

		protected void Update() {
			if (!IsActing) {
				return;
			}

			_currentTime += GlobalTimeManager.DeltaTime;

			if (_currentTime >= _totalWeaponDuration) {
				TerminateAttack();
				return;
			}

			foreach (MeleeStrikeType meleeStrikeType in _builtMeleeStrikes) {
				meleeStrikeType.Animate(_currentTime);
			}
		}

		public void Initialize(WeaponProperties weaponProperties) {
			_totalWeaponDuration = weaponProperties.TotalWeaponDuration;
			_builtMeleeStrikes.Clear();

			foreach (MeleeStrikeProperties meleeStrikeProperties in weaponProperties.MeleeStrikes) {
				BuildMeleeStrike(meleeStrikeProperties);
			}
		}

		private void BuildMeleeStrike(MeleeStrikeProperties meleeStrikeProperties) {
			float meleeStrikeEndTime = meleeStrikeProperties.StartTime + meleeStrikeProperties.TotalTime;

			if (meleeStrikeEndTime > _totalWeaponDuration) {
				_currentTime = meleeStrikeEndTime;
			}

			Type strikeType = Type.GetType(meleeStrikeProperties.StrikeTypeClass);

			if (strikeType != null) {
				MeleeStrikeType newStrike = (MeleeStrikeType) Activator.CreateInstance(strikeType, meleeStrikeProperties, this);
				_builtMeleeStrikes.Add(newStrike);
			}
		}

		public float GetPercentageComplete() {
			return _currentTime / _totalWeaponDuration;
		}

		public void Attack() {
			_currentTime = 0f;
			IsActing = true;
		}

		private void TerminateAttack() {
			IsActing = false;

			foreach (MeleeStrikeType meleeStrikeType in _builtMeleeStrikes) {
				meleeStrikeType.EndAnimation();
			}
		}
	}
}