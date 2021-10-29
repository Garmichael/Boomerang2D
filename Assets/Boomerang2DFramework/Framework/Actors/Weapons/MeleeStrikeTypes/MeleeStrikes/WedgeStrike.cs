using Boomerang2DFramework.Framework.Utilities;
using Boomerang2DFramework.Framework.Utilities.PolygonColliderControllers;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.Weapons.MeleeStrikeTypes.MeleeStrikes {
	public class WedgeStrike : MeleeStrikeType {
		private readonly WedgeColliderController _myColliderControllerScript;

		public WedgeStrike(MeleeStrikeProperties properties, WeaponBehavior parentBehavior = null) {
			Instantiate(properties, parentBehavior);
			_myColliderControllerScript = new WedgeColliderController();
		}

		public override Vector2[] GetColliderPoints() {
			return _myColliderControllerScript.GeneratePoints();
		}

		public override void Animate(float currentMomentOfStrike) {
			if (ShouldEnable(currentMomentOfStrike)) {
				Enable();
			} else {
				Disable();
				return;
			}

			CurrentActionProperties currentActionProperties = GetCurrentActionProperties(currentMomentOfStrike);

			if (currentActionProperties.CurrentAction == null) {
				return;
			}

			float currentPointInAction = currentMomentOfStrike - currentActionProperties.StartTimeOfCurrentAction;
			float endPointInAction = currentActionProperties.CurrentAction.Duration;

			WedgeStrikeProperties startState = (WedgeStrikeProperties) currentActionProperties.CurrentAction.StartState;
			WedgeStrikeProperties endState = (WedgeStrikeProperties) currentActionProperties.CurrentAction.EndState;

			_myColliderControllerScript.SetProperties(
				MyCollider,
				Mathf.FloorToInt(BoomerangUtils.EaseLerp(
					currentPointInAction,
					endPointInAction,
					startState.Chunkiness,
					endState.Chunkiness,
					currentActionProperties.CurrentAction.EaseMode
				)),
				BoomerangUtils.EaseLerp(
					currentPointInAction,
					endPointInAction,
					startState.Radius,
					endState.Radius,
					currentActionProperties.CurrentAction.EaseMode
				),
				BoomerangUtils.EaseLerp(
					currentPointInAction,
					endPointInAction,
					startState.Width,
					endState.Width,
					currentActionProperties.CurrentAction.EaseMode
				),
				BoomerangUtils.EaseLerp(
					currentPointInAction,
					endPointInAction,
					startState.Angle,
					endState.Angle,
					currentActionProperties.CurrentAction.EaseMode
				),
				BoomerangUtils.EaseLerp(
					currentPointInAction,
					endPointInAction,
					startState.OffsetX,
					endState.OffsetX,
					currentActionProperties.CurrentAction.EaseMode
				),
				BoomerangUtils.EaseLerp(
					currentPointInAction,
					endPointInAction,
					startState.OffsetY,
					endState.OffsetY,
					currentActionProperties.CurrentAction.EaseMode
				)
			);

		}
	}
}