using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.MapGeneration;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors {
	/// <summary>
	/// Every frame, iterates through Actors and keeps them up to pace with each others.
	/// </summary>
	/// <remarks>
	/// This is done in multiple loops so that each actor remains at the same point in its life cycle. This is done
	/// so that when Actors check other actors for Overlapping and Solid Bounding Boxes, each other actor is where its
	/// expected to be. This helps prevent race conditions where slightly different behavior occurs based on which
	/// Actor executes first.
	/// </remarks>
	public class ActorManager : MonoBehaviour {
		public GameObject ActorPoolContainer;
		private readonly List<Actor> _actorPool = new List<Actor>();
		private List<Actor> _queuedForPooling = new List<Actor>();

		public int PoolSize = 0;

		public void Initialize() {
			ActorPoolContainer = new GameObject("ActorPoolContainer");
			ActorPoolContainer.transform.position = new Vector3(-1000, -1000, -1000);
		}

		public void Update() {
			PoolSize = _actorPool.Count;
			Actor[] actors = MapManager.GetActorCatalog().ToArray();

			foreach (Actor actor in actors) {
				actor.SetEnabled();

				if (actor.IsEnabled && !actor.IsPaused) {
					actor.UpdateState();
					actor.ProcessTriggers();
					actor.ProcessSetUpFrameState();
				}
			}

			GenerateRayData(actors);

			foreach (Actor actor in actors) {
				if (actor.IsEnabled && !actor.IsPaused) {
					actor.GetOverlappingCollisions();
					actor.SetIsGrounded();
				}
			}

			foreach (Actor actor in actors) {
				if (actor.IsEnabled && !actor.IsPaused) {
					actor.ProcessState();
				}
			}

			foreach (Actor actor in actors) {
				if (actor.IsEnabled && !actor.IsPaused) {
					if (actor.VelocityOrder == ActorVelocityOrder.VerticalFirst) {
						actor.GenerateAllRayData();
						actor.ApplyVerticalVelocity();
						actor.GenerateAllRayData();
						actor.ApplyHorizontalVelocity();
					} else {
						actor.GenerateAllRayData();
						actor.ApplyHorizontalVelocity();
						actor.GenerateAllRayData();
						actor.ApplyVerticalVelocity();
					}
				}
			}

			foreach (Actor actor in actors) {
				if (actor.IsEnabled && !actor.IsPaused) {
					actor.SnapToPositioning();
					actor.ProcessPostFrameStates();
				}
			}

			GenerateRayData(actors);

			foreach (Actor actor in actors) {
				if (actor.IsEnabled && !actor.IsPaused) {
					actor.ExecuteInteractionEvents();
					actor.ProcessTransitionTriggers();
					actor.EndOfFrameManagement();
				}
			}

			PoolQueuedActors();
		}

		private void GenerateRayData(IEnumerable<Actor> actorList) {
			foreach (Actor actor in actorList) {
				if (actor.IsEnabled && !actor.IsPaused) {
					actor.GenerateAllRayData();
				}
			}
		}

		public Actor GetActorFromPool(
			ActorProperties actorProperties,
			ActorInstanceProperties actorInstanceProperties,
			Transform container,
			Vector3 spawnPosition,
			MapLayerBehavior mapLayerBehavior,
			string mapLayerName,
			string mapId
		) {
			if (_actorPool.Count == 0) {
				Actor actorScript = new Actor();
				_actorPool.Add(actorScript);
			}

			Actor fromPool = _actorPool.First();
			_actorPool.Remove(fromPool);
			
			fromPool.SetProperties(
				actorProperties,
				actorInstanceProperties,
				container,
				spawnPosition,
				mapLayerBehavior,
				mapLayerName,
				mapId
			);

			return fromPool;
		}

		public void PutActorInPool(Actor actor) {
			_queuedForPooling.Add(actor);
		}

		private void PoolQueuedActors() {
			_queuedForPooling = _queuedForPooling.Distinct().ToList();
			
			foreach (Actor actor in _queuedForPooling) {
				foreach (Actor child in actor.ChildrenActors) {
					MapManager.RemoveActor(child);
					child.ResetProperties();
					_actorPool.Add(child);
				}

				MapManager.RemoveActor(actor);

				actor.ResetProperties();
				_actorPool.Add(actor);
			}

			_queuedForPooling.Clear();
		}

		public bool ActorIsPooled(Actor actor) {
			return _actorPool.Contains(actor);
		}

		public void ClearPool() {
			while (_actorPool.Count > 0) {
				Destroy(_actorPool[0].GameObject);
				_actorPool.RemoveAt(0);
			}
		}
	}
}