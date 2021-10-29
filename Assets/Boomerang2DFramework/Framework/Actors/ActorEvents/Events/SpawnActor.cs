using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SpawnActor : ActorEvent {
		private SpawnActorProperties MyProperties => (SpawnActorProperties) Properties;

		public SpawnActor(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			string json = BoomerangDatabase.ActorJsonDatabaseEntries[MyProperties.Actor].text;

			if (json == null) {
				return;
			}

			Vector3 spawnLocation = new Vector3(
				MyProperties.PositionX.BuildValue(targetActor) +
				(MyProperties.RelativePosition ? targetActor.RealPosition.x : 0),
				MyProperties.PositionY.BuildValue(targetActor) +
				(MyProperties.RelativePosition ? targetActor.RealPosition.y : 0),
				MyProperties.PositionZ.BuildValue(targetActor) +
				(MyProperties.RelativePosition ? targetActor.RealPosition.z : 0)
			);

			ActorProperties actorProperties = JsonUtility.FromJson<ActorProperties>(json);

			Actor actorScript = Boomerang2D.ActorManager.GetActorFromPool(
				actorProperties,
				MyProperties.ActorInstanceProperties,
				targetActor.Transform.parent.transform,
				spawnLocation,
				targetActor.MapLayerBehavior,
				targetActor.MapLayerName,
				""
			);

			if (!MyProperties.StartInDefaultState) {
				actorScript.StateMachine.SetNextState(MyProperties.StateToStartIn);
			}

			if (MyProperties.SetFacingDirection) {
				actorScript.FacingDirection = MyProperties.FacingDirection;
			}
		}
	}
}