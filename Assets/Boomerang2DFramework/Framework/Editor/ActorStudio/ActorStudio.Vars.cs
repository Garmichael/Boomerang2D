using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.AnimationProperties;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.TriggerProperties;
using Boomerang2DFramework.Framework.Actors.InteractionEvents;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.ActorStudio {
	public partial class ActorStudio : EditorWindow {
		private static ActorStudio _window;

		private static float _windowWidth;
		private static float _windowHeight;

		private float _repaintTime;
		private bool _repaintNext;
		private float _totalTime;
		private float _timeToCheckSave;
		private bool _fileHasChanged;

		private readonly List<ActorProperties> _allActorProperties = new List<ActorProperties>();

		private enum Mode {
			Normal,
			Create,
			Rename,
			Clone,
			Delete
		}

		private Mode _mode = Mode.Normal;
		private Mode _animationMode = Mode.Normal;

		private string _inputName = "";

		private int _mainTabSelection = -1;

		private Vector2 _mousePositionInStateArea;
		private bool _isMouseLeftJustClicked;
		private bool _isMouseLeftClick;
		private bool _isMouseRightClick;

		private Rect _stateAreaPosition;

		private ActorStateProperties _highlighted;
		private ActorStateProperties _dragged;
		private Vector2 _draggedStateOffset;

		private int _indexForActiveState;

		private ActorStateProperties ActiveState => BoomerangUtils.IndexInRange(ActiveActor.States, _indexForActiveState)
			? ActiveActor.States[_indexForActiveState]
			: null;

		private static readonly Dictionary<int, string> MainTabs = new Dictionary<int, string> {
			{0, "Actor Properties"},
			{1, "States"},
			{2, "Bounding Boxes"},
			{3, "Stats"},
			{4, "Particle Effects"},
			{5, "Attached Weapons"},
			{6, "Interaction Events"},
			{7, "Children Actors"}
		};

		private int _statesSubSelection;

		private static readonly string[] StateSelectionOptions = {
			"State Properties",
			"Transition Triggers",
			"Animations",
			"Sound Effects",
			"Modification Triggers",
			"Weapon Triggers",
			"Entry Events",
			"Exit Events"
		};

		private int _indexForActiveActor = -1;
		private int _previousIndexForActiveActor = -1;

		private ActorProperties ActiveActor => BoomerangUtils.IndexInRange(_allActorProperties, _indexForActiveActor)
			? _allActorProperties[_indexForActiveActor]
			: null;

		private bool _addNewStateMode;
		private bool _cloneStateMode;

		private Vector2 _stateAreaScrollPosition;

		private bool _renameStateMode;
		private bool _deleteStateMode;
		private int _indexForNewTransitionTrigger;
		private int _indexForNewModTrigger;
		private int _indexForActiveAnimation;

		private StatePropertiesAnimation ActiveAnimation => BoomerangUtils.IndexInRange(ActiveState.Animations, _indexForActiveAnimation)
			? ActiveState.Animations[_indexForActiveAnimation]
			: null;

		private List<Texture2D> _activeActorTextures = new List<Texture2D>();

		private int _selectedAnimationFrame;

		private struct AnimationFrameDetails {
			public int SpriteFrame;
			public int AnimationFrame;
		}

		private readonly List<string> _allWeapons = new List<string>();
		private int _newWeaponToAdd;
		private int _indexForNewWeapon;

		private readonly List<string> _allParticleEffects = new List<string>();
		private int _newParticleEffectToAdd;
		private int _indexForNewParticleEffect;

		private int _indexForSelectedStateEntryActorEvent;

		private StateEntryExitEventProperties StateEntryEvent =>
			BoomerangUtils.IndexInRange(ActiveState.StateEntryActorEvents, _indexForSelectedStateEntryActorEvent)
				? ActiveState.StateEntryActorEvents[_indexForSelectedStateEntryActorEvent]
				: null;

		private int _indexForSelectedStateExitActorEvent;
		
		private StateEntryExitEventProperties StateExitEvent =>
			BoomerangUtils.IndexInRange(ActiveState.StateExitActorEvents, _indexForSelectedStateExitActorEvent)
				? ActiveState.StateExitActorEvents[_indexForSelectedStateExitActorEvent]
				: null;

		private bool _addNewStatFloatMode;
		private bool _addNewStatStringMode;
		private bool _addNewStatBoolMode;

		private int _selectedInteractionEvent;

		private ActorInteractionEvent ActiveActorInteractionEvent => BoomerangUtils.IndexInRange(ActiveActor.InteractionEvents, _selectedInteractionEvent)
			? ActiveActor.InteractionEvents[_selectedInteractionEvent]
			: null;

		private int _selectedRandomTransitionTriggerIndex;

		private RandomTransitionTriggerProperties ActiveRandomTransitionTrigger =>
			BoomerangUtils.IndexInRange(ActiveState.RandomTransitionTriggers, _selectedRandomTransitionTriggerIndex)
				? ActiveState.RandomTransitionTriggers[_selectedRandomTransitionTriggerIndex]
				: null;


		private int _selectedTransitionTriggerType;
	}
}