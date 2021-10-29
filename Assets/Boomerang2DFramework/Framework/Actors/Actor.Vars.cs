using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.Actors.Weapons;
using Boomerang2DFramework.Framework.GlobalTimeManagement;
using Boomerang2DFramework.Framework.MapGeneration;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors {
	public partial class Actor {
		/// <summary>
		/// Stores the initial Spawn Location of this Actor
		/// </summary>
		public Vector3 SpawnPosition { get; private set; }

		/// <summary>
		/// Defines if the Actor is Enabled or not
		/// </summary>
		/// <remarks>
		/// This is not the same as the Enabled property of a GameObject in Unity.
		/// IsEnabled will be used by the ActorManager to set the Enabled property of the GameObject to match
		/// </remarks>
		public bool IsEnabled { get; private set; } = true;

		/// <summary>
		/// Actor is in the ActorManager Pool
		/// </summary>
		public bool IsPooled => Boomerang2D.ActorManager.ActorIsPooled(this);
		
		/// <summary>
		/// Sets the Paused state
		/// </summary>
		/// <remarks>
		///	An actor that is Paused will still render in-game and can be interacted with by other Actors, however it
		/// will not execute any of its normal functionality, including: Animations, States Transitions, Triggers,
		/// Game/Actor Events, etc.. 
		/// </remarks>
		public bool IsPaused { get; private set; }

		/// <summary>
		/// A Reference to the GameObject containing the Actor's sprite, bounding box, and other visual elements for
		/// use in the game's playspace. 
		/// </summary>
		public GameObject GameObject { get; private set; }

		/// <summary>
		/// A Reference to the Transform of the GameObject 
		/// </summary>
		public Transform Transform { get; private set; }

		/// <summary>
		/// A collection of references to all child actors
		/// </summary>
		public List<Actor> ChildrenActors { get; private set; } = new List<Actor>();

		/// <summary>
		/// A reference to this Actor's ParentActor in the case that it's a child to another Actor.
		/// </summary>
		public Actor ParentActor { get; private set; }

		/// <summary>
		/// The name of this actor's MapLayer
		/// </summary>
		public string MapLayerName { get; private set; }

		/// <summary>
		/// A reference to the MapLayerBehavior of the Map Layer this Actor lives in
		/// </summary>
		public MapLayerBehavior MapLayerBehavior { get; private set; }

		/// <summary>
		/// The Actor's MapId
		/// </summary>
		/// <remarks>
		///	MapId's can be set to actors in the Mad Editor, and used by the ActorFinder system to reference them for
		/// use in triggers and events.
		///	</remarks>
		public string MapId { get; private set; }

		/// <summary>
		/// A reference to this Actor's State Machine
		/// </summary>
		public StateMachine StateMachine { get; private set; }

		/// <summary>
		/// A reference to the pre-defined properties as set in the Actor Studio
		/// </summary>
		/// <remarks>
		/// These Properties are set on initialization based on the values set in the Actor Studio. They are not
		/// intended to change over time.
		/// </remarks>
		public ActorProperties ActorProperties { get; private set; }

		/// <summary>
		/// A reference to the ActorInstanceProperties
		/// </summary>
		/// <remarks>
		/// Instance Properties are the ones set on the Actor inside the Map Editor.
		/// These are properties that can potentially change often through an Actor's lifespan, but can be set to
		/// specific value on initialization.
		/// </remarks>
		public ActorInstanceProperties ActorInstanceProperties { get; private set; }

		/// <summary>
		/// ActorBehavior for showing properties in the Inspector during runtime
		/// </summary>
		private ActorBehavior ActorBehavior { get; set; }
		
		/// <summary>
		/// The Actor's Velocity
		/// </summary>
		/// <remarks>
		/// Each frame, the actor's RealPosition is adjusted by it's Velocity.
		/// </remarks>
		public Vector2 Velocity { get; private set; } = Vector3.zero;

		/// <summary>
		/// The Actor's RealPosition
		/// </summary>
		/// <remarks>
		/// An Actor's RealPosition is the exact coordinates in game-space that the actor lives on.
		/// To make everything Pixel-Perfect, Boomerang 2D snaps the actor's visual position to align to pixels
		/// which helps the in rendering of the final image. Without this snap, sprites can look distorted.  However,
		/// Actors rarely move an exact number of pixels per frame, and snapping the Actor's position to pixels will
		/// result in imprecise movement per frame and an overall choppiness. RealPosition allows the Actor to remember
		/// its sub-pixel position.
		/// For example, an actor that move 1.25 pixels per frame will travel a total distance
		/// of 4 pixels over 3 frames using this technique, yet snapping the Actor's position each frame will cause it
		/// to only move 3 pixels over 3 frames (as 1.25 rounds down to 1). 
		/// </remarks>
		public Vector3 RealPosition { get; private set; } = Vector3.zero;

		/// <summary>
		/// The buffer distance for casting rays. 
		/// </summary>
		/// <remarks>
		///	When casting rays, if a ray starts on the exact coordinates that a collider's  edge, the ray will travel
		/// through that edge and will not register the hit. So we move the ray's starting position back by the
		/// distance of RaycastSkinWidth, and then add it to the distance of each of the ray's hits. This allows the
		/// framework to register distances of 0 units. 
		/// </remarks>
		private const float RaycastSkinWidth = 0.001f;

		/// <summary>
		/// The distance between the Actor and the first solid object in the up direction. 
		/// </summary>
		public float DistanceToSolidUp { get; private set; }

		/// <summary>
		/// The distance between the Actor and the first solid object in the right direction. 
		/// </summary>
		public float DistanceToSolidRight { get; private set; }

		/// <summary>
		/// The distance between the Actor and the first solid object in the down direction. 
		/// </summary>
		public float DistanceToSolidDown { get; private set; }

		/// <summary>
		/// The distance between the Actor and the first solid object in the left direction. 
		/// </summary>
		public float DistanceToSolidLeft { get; private set; }

		/// <summary>
		/// A multi-tier collection of all raycast hits from rays that fire up
		/// </summary>
		/// <remaeks>
		/// The outer list is a Collection of the Actor's Bounding Boxes. The next nested list is a collection of the
		/// Rays cast by this bounding box in this direction. The deepest list is a collection of the ray's hits.
		/// </remaeks>
		public List<List<List<RaycastHit2D>>> RayDataUp { get; private set; } = new List<List<List<RaycastHit2D>>>();

		/// <summary>
		/// A multi-tier collection of all raycast hits from rays that fire right
		/// </summary>
		/// <remaeks>
		/// The outer list is a Collection of the Actor's Bounding Boxes. The next nested list is a collection of the
		/// Rays cast by this bounding box in this direction. The deepest list is a collection of the ray's hits.
		/// </remaeks>
		public List<List<List<RaycastHit2D>>> RayDataRight { get; private set; } = new List<List<List<RaycastHit2D>>>();

		/// <summary>
		/// A multi-tier collection of all raycast hits from rays that fire down
		/// </summary>
		/// <remaeks>
		/// The outer list is a Collection of the Actor's Bounding Boxes. The next nested list is a collection of the
		/// Rays cast by this bounding box in this direction. The deepest list is a collection of the ray's hits.
		/// </remaeks>
		public List<List<List<RaycastHit2D>>> RayDataDown { get; private set; } = new List<List<List<RaycastHit2D>>>();

		/// <summary>
		/// A multi-tier collection of all raycast hits from rays that fire left
		/// </summary>
		/// <remaeks>
		/// The outer list is a Collection of the Actor's Bounding Boxes. The next nested list is a collection of the
		/// Rays cast by this bounding box in this direction. The deepest list is a collection of the ray's hits.
		/// </remaeks>
		public List<List<List<RaycastHit2D>>> RayDataLeft { get; private set; } = new List<List<List<RaycastHit2D>>>();

		/// <summary>
		/// A collection of all Actor's Bounding Boxes that were hit by up-firing Rays in GenerateRayData(), if
		/// CollidesWithActors is enabled
		/// </summary>
		public List<ActorBoundingBoxes> ActorBoundingBoxesHitByRaysUp { get; private set; } =
			new List<ActorBoundingBoxes>();

		/// <summary>
		/// A collection of all Actor's Bounding Boxes that were hit by right-firing Rays in GenerateRayData(), if
		/// CollidesWithActors is enabled
		/// </summary>
		public List<ActorBoundingBoxes> ActorBoundingBoxesHitByRaysRight { get; private set; } =
			new List<ActorBoundingBoxes>();

		/// <summary>
		/// A collection of all Actor's Bounding Boxes that were hit by down-firing Rays in GenerateRayData(), if
		/// CollidesWithActors is enabled
		/// </summary>
		public List<ActorBoundingBoxes> ActorBoundingBoxesHitByRaysDown { get; private set; } =
			new List<ActorBoundingBoxes>();

		/// <summary>
		/// A collection of all Actor's Bounding Boxes that were hit by left-firing Rays in GenerateRayData(), if
		/// CollidesWithActors is enabled
		/// </summary>
		public List<ActorBoundingBoxes> ActorBoundingBoxesHitByRaysLeft { get; private set; } =
			new List<ActorBoundingBoxes>();

		public struct ActorBoundingBoxes {
			public BoundingBoxProperties BoundingBoxProperties;
			public Actor Actor;
			public float HitDistance;
		}


		/// <summary>
		/// Allows this Actor to collide with Solid tiles. 
		/// </summary>
		/// <remarks>
		/// When Rays are cast in GenerateRayData(), this property sets the DistanceToSolid values from Tiles
		/// </remarks>
		public bool CollidesWithGeometry { get; set; }

		/// <summary>
		/// Allows this Actor to collide with Solid Bounding Boxes on other Actors
		/// </summary>
		/// <remarks>
		/// When Rays are cast in GenerateRayData(), this property sets the DistanceToSolid values from Actors
		/// </remarks>
		public bool CollidesWithActors { get; private set; }

		/// <summary>
		///  Allows this Actor to Overlap Geometry
		/// </summary>
		/// <remarks>
		/// When Overlaps are checked in GetOverlappingCollisions(), this property collects flags from Tiles
		/// </remarks>
		public bool OverlapsGeometry { get; private set; }

		/// <summary>
		///  Allows this Actor to Overlap other Actors
		/// </summary>
		/// <remarks>
		/// When Overlaps are checked in GetOverlappingCollisions(), this property collects flags from Actors
		/// </remarks>
		public bool OverlapsOtherActors { get; private set; }

		/// <summary>
		///  Allows this Actor to Overlap Weapons
		/// </summary>
		/// <remarks>
		/// When Overlaps are checked in GetOverlappingCollisions(), this property collects Weapon names
		/// </remarks>
		public bool OverlapsWeapons { get; private set; }

		public struct OverlappingCollider {
			public Actor Actor;
			public string Flag;
		}

		/// <summary>
		/// The collection of Actor Flags gathered in GetOverlappingCollisions when OverlapsOtherActors is set
		/// </summary>
		public Dictionary<string, List<OverlappingCollider>> OverlappingActorFlags { get; private set; } =
			new Dictionary<string, List<OverlappingCollider>>();

		/// <summary>
		/// The collection of Weapons gathered in GetOverlappingCollisions when OverlapsWeapons is set
		/// </summary>
		public Dictionary<string, List<string>> OverlappingWeapons { get; private set; } =
			new Dictionary<string, List<string>>();

		/// <summary>
		/// The collection of Tile Flags gathered in GetOverlappingCollisions when OverlapsGeometry is set
		/// </summary>
		public Dictionary<string, List<string>> OverlappingTileFlags { get; private set; } =
			new Dictionary<string, List<string>>();

		/// <summary>
		/// Allows this Actor to be Grounded through the IsGrounded property
		/// </summary>
		public bool CanBeGrounded { get; private set; }

		/// <summary>
		/// If CanBeGrounded is enabled, this property is set per-frame when the Actor is on the Ground
		/// </summary>
		public bool IsGrounded { get; private set; }

		/// <summary>
		/// Determines if Horizontal or Vertical velocity should be applied first when adjusting the Actor's
		/// RealPosition
		/// </summary>
		/// <remarks>
		/// Depending on the needs of an Actor's State, this property can be used for more precise control. Take a look
		/// at the MoveOrtho state where Slopes are considered to get an idea of when this is useful
		/// </remarks>
		public ActorVelocityOrder VelocityOrder { get; private set; } = ActorVelocityOrder.VerticalFirst;

		/// <summary>
		/// The Actor's BoundingBox Colliders
		/// </summary>
		public readonly List<BoxCollider2D> BoundingBoxColliders = new List<BoxCollider2D>();

		/// <summary>
		/// The Actor's Current Bounding Box properties
		/// </summary>
		/// <remarks>
		/// The Actor has a set of Bounding Boxes. Each frame of an animation has its own values for the Bounding Box's
		/// width, height, etc. This references the current frame's bounding box properties. 
		/// </remarks>
		public List<BoundingBoxProperties> CurrentBoundingBoxProperties =>
			StateMachine?.GetCurrentState() == null ||
			((ActorState) StateMachine.GetCurrentState()).ActiveAnimationFrame == null
				? null
				: ((ActorState) StateMachine.GetCurrentState()).ActiveAnimationFrame.BoundingBoxProperties;

		/// <summary>
		/// The Actor's facing Direction
		/// </summary>
		/// <remarks>
		/// Can be Up, Left, Right, or Down
		/// </remarks>
		private Directions _facingDirection;

		/// <summary>
		/// The Actor's facing Direction
		/// </summary>
		/// <remarks>
		/// Can be Up, Left, Right, or Down
		/// </remarks>
		public Directions FacingDirection {
			get => _facingDirection;
			set {
				PreviousFacingDirection = _facingDirection;
				_facingDirection = value;
			}
		}

		/// <summary>
		/// The Actor's facing direction before this direction 
		/// </summary>
		public Directions PreviousFacingDirection { get; private set; }

		/// <summary>
		/// A reference to this Actor's Sprites.
		/// </summary>
		public Sprite[] Sprites { get; private set; }

		/// <summary>
		/// A reference to this Actor's SpriteRenderer.
		/// </summary>
		public SpriteRenderer SpriteRenderer { get; private set; }

		private struct BuiltWeapon {
			public string Name;
			public GameObject GameObject;
			public WeaponBehavior Behavior;
		}

		/// <summary>
		/// All of this Actor's Weapons
		/// </summary>
		private readonly Dictionary<string, BuiltWeapon> _weapons = new Dictionary<string, BuiltWeapon>();
		
		/// <summary>
		/// A reference to the GameObject that contains all of this Actor's Weapon Game Objects
		/// </summary>
		private GameObject _weaponContainer;

		/// <summary>
		/// A references to this Actor's ParticleEffects
		/// </summary>
		public Dictionary<string, GameObject> ParticleEffects { get; private set; } =
			new Dictionary<string, GameObject>();

		public List<GlobalTimeManager.QueuedAction> QueuedActions = new List<GlobalTimeManager.QueuedAction>();
	}
}