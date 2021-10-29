using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class MathOnStat : ActorEvent {
		private MathOnStatProperties MyProperties => (MathOnStatProperties) Properties;

		public MathOnStat(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null || sourceActor?.ActorProperties == null) {
				return;
			}
			
			float adjustmentValue = MyProperties.AdjustmentValue.BuildValue(sourceActor);
			Dictionary<string, FloatStatProperties> hashedStats =
				targetActor.ActorProperties.StatsFloats.ToDictionary(actorPropertiesStat => actorPropertiesStat.Name);

			if (hashedStats.ContainsKey(MyProperties.StatName)) {
				if (MyProperties.Operation == MathOperation.Plus) {
					hashedStats[MyProperties.StatName].Value += adjustmentValue;
				} else if (MyProperties.Operation == MathOperation.Minus) {
					hashedStats[MyProperties.StatName].Value -= adjustmentValue;
				} else if (MyProperties.Operation == MathOperation.Divide) {
					hashedStats[MyProperties.StatName].Value /= adjustmentValue;
				} else if (MyProperties.Operation == MathOperation.Multiply) {
					hashedStats[MyProperties.StatName].Value *= adjustmentValue;
				} else if (MyProperties.Operation == MathOperation.Square) {
					hashedStats[MyProperties.StatName].Value *= hashedStats[MyProperties.StatName].Value;
				} else if (MyProperties.Operation == MathOperation.Root) {
					hashedStats[MyProperties.StatName].Value = (float) Math.Sqrt(hashedStats[MyProperties.StatName].Value);
				} else if (MyProperties.Operation == MathOperation.Modulo) {
					hashedStats[MyProperties.StatName].Value %= adjustmentValue;
				}
			}
		}
	}
}