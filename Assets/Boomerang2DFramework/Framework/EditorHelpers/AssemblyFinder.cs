using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.ActorFinderFilters;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;
using Boomerang2DFramework.Framework.Actors.Weapons.MeleeStrikeTypes;
using Boomerang2DFramework.Framework.GameEvents;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.UiManagement.TriggerSystem;
using Boomerang2DFramework.Framework.UiManagement.UiElements;

namespace Boomerang2DFramework.Framework.EditorHelpers {
	public static class AssemblyFinder {
#if UNITY_EDITOR
		static AssemblyFinder() {
			BuildClassAssemblies();
		}

		public struct ClassAssemblies {
			public Dictionary<string, string> ActorStates;
			public Dictionary<string, string> ActorStateProperties;
			public Dictionary<string, string> CameraStates;
			public Dictionary<string, string> CameraStateProperties;
			public Dictionary<string, string> ActorTriggers;
			public Dictionary<string, string> ActorTriggerProperties;
			public Dictionary<string, string> ActorFilters;
			public Dictionary<string, string> ActorFilterProperties;
			public Dictionary<string, string> ActorEvents;
			public Dictionary<string, string> ActorEventProperties;
			public Dictionary<string, string> GameEvents;
			public Dictionary<string, string> GameEventProperties;
			public Dictionary<string, string> WeaponStrikes;
			public Dictionary<string, string> WeaponStrikeProperties;
			public Dictionary<string, string> UiElementProperties;
			public Dictionary<string, string> UiElementBehaviors;
			public Dictionary<string, string> UiElementEditors;
			public Dictionary<string, string> HudObjectTriggers;
			public Dictionary<string, string> HudObjectTriggerProperties;
		}

		public static ClassAssemblies Assemblies;

		private static void BuildClassAssemblies() {
			Assemblies = new ClassAssemblies {
				ActorStates = GetClasses(GameProperties.EngineActorStateClassNs, GameProperties.CustomActorStateClassNs, typeof(State)),
				ActorStateProperties = GetClasses(GameProperties.EngineActorStateClassNs, GameProperties.CustomActorStateClassNs, typeof(StateProperties)),
				CameraStates = GetClasses(GameProperties.EngineCameraStateClassNs, GameProperties.CustomCameraStateClassNs, typeof(State)),
				CameraStateProperties = GetClasses(GameProperties.EngineCameraStateClassNs, GameProperties.CustomCameraStateClassNs, typeof(StateProperties)),
				ActorTriggers = GetClasses(GameProperties.EngineActorTriggerClassNs, GameProperties.CustomActorTriggerClassNs, typeof(ActorTrigger)),
				ActorTriggerProperties = GetClasses(GameProperties.EngineActorTriggerClassNs, GameProperties.CustomActorTriggerClassNs,
					typeof(ActorTriggerProperties)),
				ActorFilters = GetClasses(GameProperties.EngineActorFilterClassNs, GameProperties.CustomActorFilterClassNs, typeof(ActorFinderFilter)),
				ActorFilterProperties = GetClasses(GameProperties.EngineActorFilterClassNs, GameProperties.CustomActorFilterClassNs,
					typeof(ActorFinderFilterProperties)),
				ActorEvents = GetClasses(GameProperties.EngineActorEventsClassNs, GameProperties.CustomActorEventsClassNs, typeof(ActorEvent)),
				ActorEventProperties = GetClasses(GameProperties.EngineActorEventsClassNs, GameProperties.CustomActorEventsClassNs,
					typeof(ActorEventProperties)),
				GameEvents = GetClasses(GameProperties.EngineGameEventsClassNs, GameProperties.CustomGameEventsClassNs, typeof(GameEvent)),
				GameEventProperties = GetClasses(GameProperties.EngineGameEventsClassNs, GameProperties.CustomGameEventsClassNs, typeof(GameEventProperties)),
				WeaponStrikes = GetClasses(GameProperties.EngineWeaponStrikeClassNs, GameProperties.CustomWeaponStrikeClassNs, typeof(MeleeStrikeType)),
				WeaponStrikeProperties = GetClasses(GameProperties.EngineWeaponStrikeClassNs, GameProperties.CustomWeaponStrikeClassNs,
					typeof(MeleeStrikeTypeProperties)),
				UiElementProperties = GetClasses(GameProperties.EngineUiElementsPropertiesClassNs, GameProperties.CustomUiElementsPropertiesClassNs,
					typeof(UiElementProperties)),
				UiElementBehaviors = GetClasses(GameProperties.EngineUiElementsBehaviorClassNs, GameProperties.CustomUiElementsBehaviorClassNs,
					typeof(UiElementBehavior)),
				UiElementEditors = GetClasses(GameProperties.EngineUiElementsEditorClassNs, GameProperties.CustomUiElementsEditorClassNs,
					typeof(UiElementEditor)),
				HudObjectTriggers = GetClasses(GameProperties.EngineUiElementsTriggerClassNs, GameProperties.CustomHudObjectTriggerClassNs,
					typeof(HudObjectTrigger)),
				HudObjectTriggerProperties = GetClasses(GameProperties.EngineUiElementsTriggerClassNs, GameProperties.CustomHudObjectTriggerClassNs,
					typeof(HudObjectTriggerProperties))
			};
		}

		private static Dictionary<string, string> GetClasses(string builtInPath, string customPath, Type matchedType) {
			Dictionary<string, string> builtIn = GetTypesInNamespace(true, builtInPath, matchedType);
			Dictionary<string, string> custom = GetTypesInNamespace(false, customPath, matchedType);

			foreach (KeyValuePair<string, string> customClass in custom) {
				builtIn[customClass.Key] = customClass.Value;
			}

			return builtIn;
		}

		private static Dictionary<string, string> GetTypesInNamespace(bool isB2DClass, string nameSpace, Type matchedType) {
			Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			Assembly assemblyWithNameSpace = null;

			foreach (Assembly assembly in allAssemblies) {
				bool foundAssembly = false;

				if (assembly.GetTypes().Any(type => type.Namespace == nameSpace)) {
					assemblyWithNameSpace = assembly;
					foundAssembly = true;
				}

				if (foundAssembly) {
					break;
				}
			}

			if (assemblyWithNameSpace == null) {
				return new Dictionary<string, string>();
			}

			Dictionary<string, string> classes = new Dictionary<string, string>();

			Type[] classList = assemblyWithNameSpace.GetTypes();
			Array.Sort(classList, (a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

			foreach (Type type in classList) {
				if (type.Namespace != nameSpace || !IsSameOrSubclass(matchedType, type)) {
					continue;
				}

				string key = isB2DClass ? "B2D: " : "Ext: ";
				key += type.Name;

				classes[key] = type.AssemblyQualifiedName;
			}

			return classes;
		}

		private static bool IsSameOrSubclass(Type baseType, Type checkType) {
			return checkType.IsSubclassOf(baseType) || checkType == baseType;
		}
#endif
	}
}