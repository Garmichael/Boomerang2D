using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.AnimationProperties;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.SoundEffectProperties;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.TriggerProperties;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.PropertyClasses {
	/// <summary>
	/// JSON object for setting the properties of an Actor State  
	/// </summary>
	[System.Serializable]
	public class ActorStateProperties {
		public string Name = "";
		public string Class = "";
		public string PropertiesClass = "";
		public string Properties = "{}";
		public List<TransitionTriggerProperties> TransitionTriggers = new List<TransitionTriggerProperties>();
		public List<RandomTransitionTriggerProperties> RandomTransitionTriggers = new List<RandomTransitionTriggerProperties>();
		public List<ModificationTriggerProperties> ModificationTriggers = new List<ModificationTriggerProperties>();
		public List<WeaponTriggerProperties> WeaponTriggers = new List<WeaponTriggerProperties>();
		public List<StatePropertiesAnimation> Animations = new List<StatePropertiesAnimation>();
		public List<StatePropertiesSoundEffect> SoundEffects = new List<StatePropertiesSoundEffect>();
		public Vector2 EditorPosition = Vector2.zero;
		public List<StateEntryExitEventProperties> StateEntryActorEvents = new List<StateEntryExitEventProperties>();
		public List<StateEntryExitEventProperties> StateExitActorEvents = new List<StateEntryExitEventProperties>();
	}
}