using Boomerang2DFramework.Framework.Utilities;
using Boomerang2DFramework.Framework.Utilities.PolygonColliderControllers;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.Weapons.MeleeStrikeTypes.MeleeStrikes {
	public class BoxStrike : MeleeStrikeType {
		private readonly BetterBoxColliderController _myColliderControllerScript;

		public BoxStrike(MeleeStrikeProperties properties, WeaponBehavior parentBehavior = null) {
			Instantiate(properties, parentBehavior);
			_myColliderControllerScript = new BetterBoxColliderController();
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

			BoxStrikeProperties startState = (BoxStrikeProperties) currentActionProperties.CurrentAction.StartState;
			BoxStrikeProperties endState = (BoxStrikeProperties) currentActionProperties.CurrentAction.EndState;

			_myColliderControllerScript.SetProperties(
				MyCollider,
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
					startState.Height,
					endState.Height,
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
					startState.OriginX,
					endState.OriginX,
					currentActionProperties.CurrentAction.EaseMode
				), BoomerangUtils.EaseLerp(
					currentPointInAction,
					endPointInAction,
					startState.OriginY,
					endState.OriginY,
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