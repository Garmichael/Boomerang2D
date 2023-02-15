using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors {
	/// <summary>
	/// Maps Actor Properties to the Containing GameObject for an Actor so they can be seen in the Inspector
	/// </summary>
	public class ActorBehavior : MonoBehaviour {
		public Actor Actor;

		public bool IsPaused;
		public bool IsPooled;
		public bool IsEnabled;

		public Vector2 Velocity;
		public Vector3 RealPosition;
		public Directions FacingDirection;

		public bool IsGrounded;
		
		public float DistanceToSolidUp;
		public float DistanceToSolidRight;
		public float DistanceToSolidDown;
		public float DistanceToSolidLeft;

		public string ActiveState;
		public List<string> Stats = new();
		public List<State> States;

		public List<BoundingBoxProperties> BoundingBoxProperties = new List<BoundingBoxProperties>();

		[Serializable]
		public struct ActorHitByRayData {
			public string Name;
			public float Distance;
			public BoundingBoxProperties BoundingBoxProperties;
		}

		public List<string> ActorsOverlapping = new List<string>();
		public List<ActorHitByRayData> ActorsHitByRaysUp = new List<ActorHitByRayData>();
		public List<ActorHitByRayData> ActorsHitByRaysRight = new List<ActorHitByRayData>();
		public List<ActorHitByRayData> ActorsHitByRaysDown = new List<ActorHitByRayData>();
		public List<ActorHitByRayData> ActorsHitByRaysLeft = new List<ActorHitByRayData>();
		public List<string> OverlappingWeapons = new List<string>();
		public List<string> OverlappingTileFlags = new List<string>();

		public ActorProperties ActorJsonData;

		public void Update() {
			IsPooled = Actor.IsPooled;
			IsPaused = Actor.IsPaused;
			IsEnabled = Actor.IsEnabled;

			States = Actor.StateMachine.AllStates.Values.ToList();

			if (Actor.ActorProperties == null) {
				return;
			}
			
			Stats.Clear();
			foreach (FloatStatProperties statProperties in Actor.ActorProperties.StatsFloats) {
				Stats.Add("[F] " + statProperties.Name + ": " + statProperties.Value);
			}
			
			foreach (BoolStatProperties statProperties in Actor.ActorProperties.StatsBools) {
				Stats.Add("[B] " + statProperties.Name + ": " + statProperties.Value);
			}
			
			foreach (StringStatProperties statProperties in Actor.ActorProperties.StatsStrings) {
				Stats.Add("[S] " + statProperties.Name + ": " + statProperties.Value);
			}

			DistanceToSolidUp = Actor.DistanceToSolidUp;
			DistanceToSolidRight = Actor.DistanceToSolidRight;
			DistanceToSolidDown = Actor.DistanceToSolidDown;
			DistanceToSolidLeft = Actor.DistanceToSolidLeft;

			Velocity = Actor.Velocity;
			RealPosition = Actor.RealPosition;
			FacingDirection = Actor.FacingDirection;
			IsGrounded = Actor.IsGrounded;
			ActiveState = Actor.StateMachine.GetCurrentState()?.Name;

			BoundingBoxProperties = Actor.CurrentBoundingBoxProperties;

			OverlappingWeapons.Clear();
			foreach (KeyValuePair<string, List<string>> dictionaryEntry in Actor.OverlappingWeapons) {
				foreach (string overlappingWeapon in dictionaryEntry.Value) {
					OverlappingTileFlags.Add(dictionaryEntry.Key + ": " + overlappingWeapon);
				}
			}

			OverlappingTileFlags = new List<string>();
			foreach (KeyValuePair<string, List<string>> dictionaryEntry in Actor.OverlappingTileFlags) {
				foreach (string overlappingFlag in dictionaryEntry.Value) {
					OverlappingTileFlags.Add(dictionaryEntry.Key + ": " + overlappingFlag);
				}
			}


			ActorsOverlapping.Clear();
			foreach (
				KeyValuePair<string, List<Actor.OverlappingCollider>> overlappingColliders
				in Actor.OverlappingActorFlags
			) {
				foreach (Actor.OverlappingCollider otherActorData in overlappingColliders.Value) {
					if (otherActorData.Actor.ActorProperties != null) {
						ActorsOverlapping.Add(
							"Self: " +
							overlappingColliders.Key +
							" | Other: " +
							otherActorData.Actor.ActorProperties.Name +
							"on flag " +
							otherActorData.Flag
						);
					}
				}
			}

			ActorsHitByRaysUp.Clear();

			foreach (Actor.ActorBoundingBoxes actorBoundingBox in Actor.ActorBoundingBoxesHitByRaysUp) {
				if (actorBoundingBox.Actor.ActorProperties != null) {
					ActorsHitByRaysUp.Add(new ActorHitByRayData() {
						Name = actorBoundingBox.Actor.ActorProperties.Name,
						Distance = actorBoundingBox.HitDistance,
						BoundingBoxProperties = actorBoundingBox.BoundingBoxProperties
					});
				}
			}

			ActorsHitByRaysRight.Clear();
			foreach (Actor.ActorBoundingBoxes actorBoundingBox in Actor.ActorBoundingBoxesHitByRaysRight) {
				if (actorBoundingBox.Actor.ActorProperties != null) {
					ActorsHitByRaysRight.Add(new ActorHitByRayData() {
						Name = actorBoundingBox.Actor.ActorProperties.Name,
						Distance = actorBoundingBox.HitDistance,
						BoundingBoxProperties = actorBoundingBox.BoundingBoxProperties
					});
				}
			}

			ActorsHitByRaysDown.Clear();
			foreach (Actor.ActorBoundingBoxes actorBoundingBox in Actor.ActorBoundingBoxesHitByRaysDown) {
				if (actorBoundingBox.Actor.ActorProperties != null) {
					ActorsHitByRaysDown.Add(new ActorHitByRayData() {
						Name = actorBoundingBox.Actor.ActorProperties.Name,
						Distance = actorBoundingBox.HitDistance,
						BoundingBoxProperties = actorBoundingBox.BoundingBoxProperties
					});
				}
			}

			ActorsHitByRaysLeft.Clear();
			foreach (Actor.ActorBoundingBoxes actorBoundingBox in Actor.ActorBoundingBoxesHitByRaysLeft) {
				if (actorBoundingBox.Actor.ActorProperties != null) {
					ActorsHitByRaysLeft.Add(new ActorHitByRayData() {
						Name = actorBoundingBox.Actor.ActorProperties.Name,
						Distance = actorBoundingBox.HitDistance,
						BoundingBoxProperties = actorBoundingBox.BoundingBoxProperties
					});
				}
			}
		}
	}
}