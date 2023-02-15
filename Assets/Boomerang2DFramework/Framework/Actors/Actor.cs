using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.ActorFinderFilters;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties;
using Boomerang2DFramework.Framework.Actors.InteractionEvents;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;
using Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers;
using Boomerang2DFramework.Framework.Actors.Weapons;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.GlobalTimeManagement;
using Boomerang2DFramework.Framework.MapGeneration;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;


namespace Boomerang2DFramework.Framework.Actors {
	/// <summary>
	/// The Base Class for all Actors in the Boomerang2D Framework.
	/// </summary>
	public partial class Actor {
		public Actor() {
			GameObject = new GameObject("PooledActor") {layer = LayerMask.NameToLayer("Actor")};
			Transform = GameObject.transform;
			ActorBehavior = GameObject.AddComponent<ActorBehavior>();
			SpriteRenderer = GameObject.AddComponent<SpriteRenderer>();
			Rigidbody2D rigidBody2D = GameObject.AddComponent<Rigidbody2D>();
			rigidBody2D.isKinematic = true;
		}

		/// <summary>
		/// Resets the actor's properties to a default Actor
		/// </summary>
		public void ResetProperties() {
			SpawnPosition = Vector3.zero;
			ActorProperties = null;

			if (ActorBehavior) {
				ActorBehavior.ActorJsonData = null;
			}

			// GameObject.SetActive(false);
			GameObject.name = "Pooled Actor";
			GameObject.transform.parent = Boomerang2D.ActorManager.ActorPoolContainer.transform;
			ActorInstanceProperties = null;
			MapLayerBehavior = null;
			MapLayerName = "";
			MapId = "";
			Sprites = new Sprite[0];
			ActorBoundingBoxesHitByRaysUp.Clear();
			ActorBoundingBoxesHitByRaysLeft.Clear();
			ActorBoundingBoxesHitByRaysRight.Clear();
			ActorBoundingBoxesHitByRaysDown.Clear();
			SpriteRenderer.enabled = false;
			Material material = new Material(BoomerangDatabase.ShaderDatabaseEntries["Sprites/Default"]);
			SpriteRenderer.material = material;
			material.SetFloat(Shader.PropertyToID("_StartTime"), Time.time);
			
			ActorProperties?.InteractionEvents.Clear();
			
			foreach (BoxCollider2D boxCollider2D in BoundingBoxColliders) {
				boxCollider2D.enabled = false;
			}

			StateMachine?.ClearStates();
			ParticleEffects.Clear();
			_weapons.Clear();

			foreach (Transform child in GameObject.transform) {
				GameObject.Destroy(child.gameObject);
			}

			ParentActor = null;
			ChildrenActors.Clear();
			IsEnabled = false;
			RealPosition = Vector3.zero;
			Velocity = Vector2.zero;
			SpawnPosition = Vector3.zero;
			DistanceToSolidUp = 2000;
			DistanceToSolidRight = 2000;
			DistanceToSolidDown = 2000;
			DistanceToSolidLeft = 2000;
			RayDataUp = new List<List<List<RaycastHit2D>>>();
			RayDataRight = new List<List<List<RaycastHit2D>>>();
			RayDataDown = new List<List<List<RaycastHit2D>>>();
			RayDataLeft = new List<List<List<RaycastHit2D>>>();
			ActorBoundingBoxesHitByRaysUp = new List<ActorBoundingBoxes>();
			ActorBoundingBoxesHitByRaysRight = new List<ActorBoundingBoxes>();
			ActorBoundingBoxesHitByRaysDown = new List<ActorBoundingBoxes>();
			ActorBoundingBoxesHitByRaysLeft = new List<ActorBoundingBoxes>();
			CollidesWithGeometry = false;
			CollidesWithActors = false;
			OverlapsGeometry = false;
			OverlapsOtherActors = false;
			OverlapsWeapons = false;
			OverlappingActorFlags = new Dictionary<string, List<OverlappingCollider>>();
			OverlappingWeapons = new Dictionary<string, List<string>>();
			OverlappingTileFlags = new Dictionary<string, List<string>>();
			CanBeGrounded = false;
			IsGrounded = false;
			VelocityOrder = ActorVelocityOrder.VerticalFirst;
			_facingDirection = Directions.Right;
			PreviousFacingDirection = _facingDirection;

			foreach (GlobalTimeManager.QueuedAction queuedAction in QueuedActions) {
				GlobalTimeManager.CancelQueuedAction(queuedAction);
			}
			
			QueuedActions.Clear();
		}

		/// <summary>
		/// Sets an actor's properties
		/// </summary>
		public void SetProperties(
			ActorProperties actorProperties,
			ActorInstanceProperties actorInstanceProperties,
			Transform container,
			Vector3 spawnPosition,
			MapLayerBehavior mapLayerBehavior,
			string mapLayerName,
			string mapId
		) {
			SpawnPosition = spawnPosition;
			ActorProperties = actorProperties;

			GameObject.name = ActorProperties.Name;
			GameObject.transform.parent = container;
			Transform = GameObject.transform;
			Transform.localPosition = SpawnPosition;
			RealPosition = SpawnPosition;
			ActorBehavior.ActorJsonData = ActorProperties;
			ActorBehavior.Actor = this;
			SpriteRenderer.enabled = true;

			ActorInstanceProperties = actorInstanceProperties;
			_facingDirection = ActorInstanceProperties.FacingDirection;
			PreviousFacingDirection = ActorInstanceProperties.FacingDirection;

			MapLayerBehavior = mapLayerBehavior;
			MapLayerName = mapLayerName;
			MapId = mapId;

			if (actorProperties.IsPlayer) {
				Boomerang2D.Player = this;
				Boomerang2D.MainCameraController.Target = this;
			}

			MapManager.AddActorToCatalog(this);

			SetSpriteData();
			BuildBoundingBoxes();
			SetInitialStats();
			CreateStateInstances();
			CreateParticleEffectInstances();
			CreateWeaponInstances();
			BuildInteractionEventObjects();
			BuildChildren();

			IsEnabled = true;
		}

		/// <summary>
		/// Retrieves the specified WeaponBehavior from the Actor
		/// </summary>
		/// <param name="weaponName">The string name of the weapon.
		/// This name matches the one defined in the Weapon Studio</param>
		/// <returns>The WeaponBehavior class for the Weapon</returns>
		public WeaponBehavior GetWeaponBehavior(string weaponName) {
			if (!_weapons.ContainsKey(weaponName)) {
				Debug.LogWarning("Named Weapon " + weaponName + " Does Not Exist on this Actor");
				return null;
			}

			return _weapons[weaponName].Behavior;
		}

		/// <summary>
		/// Retrieves the GameObject for the specified Weapon from the Actor 
		/// </summary>
		/// <param name="weaponName"></param>
		/// <returns>The GameObject associated with the Weapon</returns>
		public GameObject GetWeaponGameObject(string weaponName) {
			if (!_weapons.ContainsKey(weaponName)) {
				Debug.LogWarning("Named Weapon " + weaponName + " Does Not Exist on this Actor");
				return null;
			}

			return _weapons[weaponName].GameObject;
		}

		/// <summary>
		/// Sets the IsEnabled Property of the Actor.
		/// </summary>
		public void SetEnabled() {
			if (ActorProperties.ObeysChunking) {
				IsEnabled = ParentActor == null
					? MapLayerBehavior.PointIsInActiveChunk(RealPosition)
					: MapLayerBehavior.PointIsInActiveChunk(RealPosition + ParentActor.RealPosition);
			} else {
				IsEnabled = true;
			}

			GameObject.SetActive(IsEnabled);
		}

		/// <summary>
		/// Set the Paused Status of an Actor.
		/// </summary>
		/// <param name="isPaused">Whether the Actor is Paused or not</param>
		public void SetPaused(bool isPaused) {
			IsPaused = isPaused;
		}

		/// <summary>
		/// Sets a reference to the MapLayerBehavior that contains this Actor
		/// </summary>
		/// <param name="mapLayerBehavior"></param>
		public void SetMapLayerBehavior(MapLayerBehavior mapLayerBehavior) {
			MapLayerBehavior = mapLayerBehavior;
		}

		/// <summary>
		/// Set the Actor's RealPosition.
		/// </summary>
		/// <param name="realPosition">Vector3 Position</param>
		public void SetRealPosition(Vector3 realPosition) {
			RealPosition = realPosition;
		}

		/// <summary>
		/// Set the Actor's RealPosition.
		/// </summary>
		/// <param name="x">The X Coordinate</param>
		/// <param name="y">The Y Coordinate</param>
		public void SetRealPosition(float x, float y) {
			RealPosition = new Vector3(x, y, RealPosition.z);
		}

		/// <summary>
		/// Set the Actor's X Coordinate of the RealPosition
		/// </summary>
		/// <param name="x">The X Coordinate</param>
		public void SetRealPositionX(float x) {
			RealPosition = new Vector3(x, RealPosition.y, RealPosition.z);
		}

		/// <summary>
		/// Set the Actor's Y Coordinate of the RealPosition
		/// </summary>
		/// <param name="y">The Y Coordinate</param>
		public void SetRealPositionY(float y) {
			RealPosition = new Vector3(RealPosition.x, y, RealPosition.z);
		}

		/// <summary>
		/// Adds a value to the Coordinates of the Actor's RealPosition
		/// </summary>
		/// <param name="offset">Vector3 Offset</param>
		public void OffsetRealPosition(Vector3 offset) {
			RealPosition += offset;
		}

		/// <summary>
		/// Adds a value to the Coordinates of the Actor's RealPosition
		/// </summary>
		/// <param name="x">X Coordinate Offset</param>
		/// <param name="y">Y Coordinate Offset</param>
		public void OffsetRealPosition(float x, float y) {
			RealPosition += new Vector3(x, y, 0);
		}

		/// <summary>
		/// Adds a value to the X Coordinate of the Actor's RealPosition
		/// </summary>
		/// <param name="x">X Coordinate Offset</param>
		public void OffsetRealPositionX(float x) {
			RealPosition += new Vector3(x, 0, 0);
		}

		/// <summary>
		/// Adds a value to the Y Coordinate of the Actor's RealPosition
		/// </summary>
		/// <param name="y">Y Coordinate Offset</param>
		public void OffsetRealPositionY(float y) {
			RealPosition += new Vector3(0, y, 0);
		}

		/// <summary>
		/// Sets the Actor's Velocity
		/// </summary>
		/// <param name="velocity">The new Vector2 Velocity</param>
		public void SetVelocity(Vector2 velocity) {
			Velocity = velocity;
		}

		/// <summary>
		/// Sets the Actor's Velocity
		/// </summary>
		/// <param name="x">The Velocity's X value</param>
		/// <param name="y">The Velocity's Y value</param>
		public void SetVelocity(float x, float y) {
			Velocity = new Vector2(x, y);
		}

		/// <summary>
		/// Set the Actor's X Velocity
		/// </summary>
		/// <param name="x">The X Velocity value</param>
		public void SetVelocityX(float x) {
			Velocity = new Vector2(x, Velocity.y);
		}

		/// <summary>
		/// Set the Actor's Y Velocity
		/// </summary>
		/// <param name="y">The Y Velocity value</param>
		public void SetVelocityY(float y) {
			Velocity = new Vector2(Velocity.x, y);
		}

		/// <summary>
		/// Adds a value to the Actor's Velocity
		/// </summary>
		/// <param name="offset">The Vector2 to add to the Velocity</param>
		public void OffsetVelocity(Vector2 offset) {
			Velocity += offset;
		}

		/// <summary>
		/// Adds a value to the Actor's Velocity
		/// </summary>
		/// <param name="x">The X value to add to the Velocity</param>
		/// <param name="y">The Y value to add to the Velocity</param>
		public void OffsetVelocity(float x, float y) {
			Velocity += new Vector2(x, y);
		}

		/// <summary>
		/// Adds a value to the Actor's X Velocity
		/// </summary>
		/// <param name="x">The X Value</param>
		public void OffsetVelocityX(float x) {
			Velocity += new Vector2(x, 0);
		}

		/// <summary>
		/// Adds a value to the Actor's Y Velocity
		/// </summary>
		/// <param name="y">The Y Value</param>
		public void OffsetVelocityY(float y) {
			Velocity += new Vector2(0, y);
		}

		/// <summary>
		/// Set the Actor's ability to be Grounded
		/// </summary>
		public void SetCanBeGrounded(bool canBeGrounded) {
			CanBeGrounded = canBeGrounded;
		}

		/// <summary>
		/// Set the Actor's ability to collide with the geometry
		/// </summary>
		public void SetCollidesWithGeometry(bool collidesWithGeometry) {
			CollidesWithGeometry = collidesWithGeometry;
		}

		/// <summary>
		/// Set the Actor's ability to overlap geometry 
		/// </summary>
		public void SetOverlapsGeometry(bool overlapsGeometry) {
			OverlapsGeometry = overlapsGeometry;
		}

		/// <summary>
		/// Set the Actor's ability to collide with other Actors
		/// </summary>
		public void SetCollidesWithActors(bool collidesWithActors) {
			CollidesWithActors = collidesWithActors;
		}

		/// <summary>
		/// Set the Actor's ability to overlap other Actors
		/// </summary>
		public void SetOverlapsOtherActors(bool overlapsOtherActors) {
			OverlapsOtherActors = overlapsOtherActors;
		}

		/// <summary>
		/// Set the Actor's ability to overlap Weapons
		/// </summary>
		public void SetOverlapsWeapons(bool overlapsWeapons) {
			OverlapsWeapons = overlapsWeapons;
		}

		/// <summary>
		/// Sets whether the Actor process its X Velocity or its  Y Velocity first
		/// </summary>
		/// <param name="velocityOrder">Enum for which Velocity Angle is processed first</param>
		public void SetVelocityOrder(ActorVelocityOrder velocityOrder) {
			VelocityOrder = velocityOrder;
		}

		/// <summary>
		/// Updates the collections that store references to Overlapping Actors, Overlapping Weapons, and Overlapping
		/// Tiles. Uses the OverlapsOtherActors, OverlapsWeapons, and OverlapsGeometry properties to collect these
		/// </summary>
		/// <remarks>
		/// Disabling one or two of these properties will give a performance boost, and disabling all three
		/// will give a more significant performance boost
		/// </remarks>
		public void GetOverlappingCollisions() {
			OverlappingActorFlags.Clear();
			OverlappingWeapons.Clear();
			OverlappingTileFlags.Clear();

			if (StateMachine.GetCurrentState() == null ||
			    ((ActorState) StateMachine.GetCurrentState()).ActiveAnimationFrame == null ||
			    !OverlapsOtherActors && !OverlapsWeapons && !OverlapsGeometry
			) {
				return;
			}

			List<BoundingBoxProperties> boundingBoxProperties =
				((ActorState) StateMachine.GetCurrentState()).ActiveAnimationFrame.BoundingBoxProperties;

			foreach (BoundingBoxProperties boundingBoxProperty in boundingBoxProperties) {
				if (!boundingBoxProperty.Enabled) {
					continue;
				}

				Collider2D[] hits = new Collider2D[50];

				if (OverlapsGeometry) {
					int totalHits = Physics2D.OverlapBoxNonAlloc(
						RealPosition + (Vector3) boundingBoxProperty.RealOffset,
						boundingBoxProperty.RealSize,
						0,
						hits
					);

					for (int index = 0; index < totalHits; index++) {
						Collider2D hit = hits[index];
						if (!hit || hit.gameObject == GameObject) {
							continue;
						}

						TileBehavior hitTileBehavior = hit.transform.parent.GetComponent<TileBehavior>();

						if (hitTileBehavior) {
							AddHitTileToCollection(boundingBoxProperty, hitTileBehavior);
						}
					}
				}

				if (OverlapsOtherActors) {
					int totalHits = Physics2D.OverlapBoxNonAlloc(
						RealPosition + (Vector3) boundingBoxProperty.RealOffset,
						boundingBoxProperty.RealSize,
						0,
						hits,
						1 << LayerMask.NameToLayer("Actor")
					);

					for (int index = 0; index < totalHits; index++) {
						Collider2D hit = hits[index];
						if (!hit || hit.gameObject == GameObject) {
							continue;
						}

						ActorBehavior otherActorBehavior = hit.GetComponent<ActorBehavior>();

						if (otherActorBehavior) {
							AddHitActorToCollection(boundingBoxProperty, hit, otherActorBehavior.Actor);
						}
					}
				}

				if (OverlapsWeapons) {
					int totalHits = Physics2D.OverlapBoxNonAlloc(
						RealPosition + (Vector3) boundingBoxProperty.RealOffset,
						boundingBoxProperty.RealSize,
						0,
						hits,
						1 << LayerMask.NameToLayer("Weapon")
					);

					for (int index = 0; index < totalHits; index++) {
						Collider2D hit = hits[index];
						if (!hit || hit.gameObject == GameObject) {
							continue;
						}

						Transform parent = hit.transform;
						WeaponBehavior hitWeapon = parent.GetComponent<WeaponBehavior>();

						if (hitWeapon) {
							AddHitWeaponToCollection(boundingBoxProperty, hitWeapon);
						}
					}
				}
			}
		}

		/// <summary>
		/// Adds an Actor that was hit in GetOverlappingCollisions
		/// </summary>
		/// <param name="boundingBoxProperty">This Actor's Bounding Box that is collecting the hits</param>
		/// <param name="hit">The Raycast hit</param>
		/// <param name="otherActor">The Actor that was hit</param>
		/// <remarks>
		/// Called from GetOverlappingCollisions if OverlapsOtherActors is enabled.
		/// </remarks>
		private void AddHitActorToCollection(
			BoundingBoxProperties boundingBoxProperty,
			Collider2D hit,
			Actor otherActor
		) {
			List<BoxCollider2D> otherBoundingBoxes = otherActor.BoundingBoxColliders;

			// Find the bounding box that was hit on the other Actor
			BoundingBoxProperties otherBoundingBoxProperties = null;

			for (int i = 0; i < otherBoundingBoxes.Count; i++) {
				if (otherBoundingBoxes[i] == hit && otherActor.CurrentBoundingBoxProperties != null) {
					otherBoundingBoxProperties = otherActor.CurrentBoundingBoxProperties[i];
					break;
				}
			}

			if (otherBoundingBoxProperties == null) {
				return;
			}

			// Store the hits into the collections sorted by both Bounding Boxes' flag properties
			if (boundingBoxProperty.Flags.Count == 0) {
				if (!OverlappingActorFlags.ContainsKey("")) {
					OverlappingActorFlags.Add("", new List<OverlappingCollider>());
				}

				foreach (string otherFlag in otherBoundingBoxProperties.Flags) {
					OverlappingActorFlags[""].Add(new OverlappingCollider {
						Actor = otherActor,
						Flag = otherFlag
					});
				}
			} else {
				foreach (string selfFlag in boundingBoxProperty.Flags) {
					if (!OverlappingActorFlags.ContainsKey(selfFlag)) {
						OverlappingActorFlags.Add(selfFlag, new List<OverlappingCollider>());
					}

					foreach (string otherFlag in otherBoundingBoxProperties.Flags) {
						OverlappingActorFlags[selfFlag].Add(new OverlappingCollider {
							Actor = otherActor,
							Flag = otherFlag
						});
					}
				}
			}
		}

		/// <summary>
		/// Adds a Weapon that was hit in GetOverlappingCollisions
		/// </summary>
		/// <param name="boundingBoxProperty">This Actor's Bounding Box that is collecting the hits</param>
		/// <param name="hitWeapon">The weapon that was hit</param>
		/// <remarks>
		/// Called from GetOverlappingCollisions if OverlapsWeapons is enabled.
		/// </remarks>
		private void AddHitWeaponToCollection(
			BoundingBoxProperties boundingBoxProperty,
			WeaponBehavior hitWeapon
		) {
			if (boundingBoxProperty.Flags.Count == 0) {
				if (!OverlappingWeapons.ContainsKey("")) {
					OverlappingWeapons.Add("", new List<string>());
				}

				OverlappingWeapons[""].Add(hitWeapon.name);
			} else {
				foreach (string selfFlag in boundingBoxProperty.Flags) {
					if (!OverlappingWeapons.ContainsKey(selfFlag)) {
						OverlappingWeapons.Add(selfFlag, new List<string>());
					}

					OverlappingWeapons[selfFlag].Add(hitWeapon.name);
				}
			}
		}

		/// <summary>
		/// Adds a Tile that was hit in GetOverlappingCollisions
		/// </summary>
		/// <param name="boundingBoxProperty">This Actor's Bounding Box that is collecting the hits</param>
		/// <param name="hitTile">The Tile that was hit</param>
		/// <remarks>
		/// Called from GetOverlappingCollisions if OverlapsTiles is enabled.
		/// </remarks>
		private void AddHitTileToCollection(BoundingBoxProperties boundingBoxProperty, TileBehavior hitTile) {
			if (boundingBoxProperty.Flags.Count == 0) {
				if (!OverlappingTileFlags.ContainsKey("")) {
					OverlappingTileFlags.Add("", new List<string>());
				}

				OverlappingTileFlags[""].AddRange(hitTile.Properties.Flags);
				OverlappingTileFlags[""] = OverlappingTileFlags[""].Distinct().ToList();
			} else {
				foreach (string selfFlag in boundingBoxProperty.Flags) {
					if (!OverlappingTileFlags.ContainsKey(selfFlag)) {
						OverlappingTileFlags.Add(selfFlag, new List<string>());
					}

					OverlappingTileFlags[selfFlag].AddRange(hitTile.Properties.Flags);
					OverlappingTileFlags[selfFlag] = OverlappingTileFlags[selfFlag].Distinct().ToList();
				}
			}
		}

		/// <summary>
		/// Generates Ray data for all four directions
		/// </summary>
		public void GenerateAllRayData() {
			SnapToPositioning();

			GenerateRayData(Directions.Up);
			GenerateRayData(Directions.Down);
			GenerateRayData(Directions.Left);
			GenerateRayData(Directions.Right);
		}

		/// <summary>
		/// Generates Ray data for a single direction
		/// </summary>
		/// <param name="direction">The Direction to generate ray data for</param>
		public void GenerateRayData(Directions direction) {
			float shortestSolidHitDistance = 2000f;

			Dictionary<Directions, List<ActorBoundingBoxes>> actorBoundingBoxHitByRayCollectionMap =
				new() {
					{Directions.Up, ActorBoundingBoxesHitByRaysUp},
					{Directions.Right, ActorBoundingBoxesHitByRaysRight},
					{Directions.Down, ActorBoundingBoxesHitByRaysDown},
					{Directions.Left, ActorBoundingBoxesHitByRaysLeft}
				};
			
			actorBoundingBoxHitByRayCollectionMap[direction].Clear();
			
			List<List<List<RaycastHit2D>>> raysInThisDirection = new();

			foreach (BoundingBoxProperties actorPropertiesBoundingBox in ActorProperties.BoundingBoxes) {
				bool boundingBoxCastsInThisDirection =
					direction == Directions.Up && actorPropertiesBoundingBox.RayCastUp ||
					direction == Directions.Right && actorPropertiesBoundingBox.RayCastRight ||
					direction == Directions.Down && actorPropertiesBoundingBox.RayCastDown ||
					direction == Directions.Left && actorPropertiesBoundingBox.RayCastLeft;

				if (boundingBoxCastsInThisDirection) {
					Vector2 rayDirection =
						direction == Directions.Up ? new Vector2(0, 1) :
						direction == Directions.Right ? new Vector2(1, 0) :
						direction == Directions.Down ? new Vector2(0, -1) :
						direction == Directions.Left ? new Vector2(-1, 0) :
						throw new ArgumentOutOfRangeException();

					float rayLength =
						direction switch {
							Directions.Up => actorPropertiesBoundingBox.RayLengthUp,
							Directions.Right => actorPropertiesBoundingBox.RayLengthRight,
							Directions.Down => actorPropertiesBoundingBox.RayLengthDown,
							_ => actorPropertiesBoundingBox.RayLengthLeft
						};

					raysInThisDirection.Add(new List<List<RaycastHit2D>>());

					IEnumerable<Vector3> originList = BuildOriginList(direction, actorPropertiesBoundingBox);

					CastRays(originList, direction, rayDirection, rayLength, raysInThisDirection.Last());

					foreach (List<List<RaycastHit2D>> directionRays in raysInThisDirection) {
						foreach (List<RaycastHit2D> ray in directionRays) {
							foreach (RaycastHit2D raycastHit2D in ray) {
								if (raycastHit2D.distance < shortestSolidHitDistance) {
									shortestSolidHitDistance = raycastHit2D.distance;
								}
							}
						}
					}
				}
			}

			if (direction == Directions.Up) {
				DistanceToSolidUp = shortestSolidHitDistance;
				RayDataUp = raysInThisDirection;
			} else if (direction == Directions.Down) {
				DistanceToSolidDown = shortestSolidHitDistance;
				RayDataDown = raysInThisDirection;
			} else if (direction == Directions.Left) {
				DistanceToSolidLeft = shortestSolidHitDistance;
				RayDataLeft = raysInThisDirection;
			} else if (direction == Directions.Right) {
				DistanceToSolidRight = shortestSolidHitDistance;
				RayDataRight = raysInThisDirection;
			}
		}

		/// <summary>
		/// Generates a collection of points where Raycasting should fire from given the inputs
		/// </summary>
		/// <param name="direction">The Direction of the Ray</param>
		/// <param name="boundingBox">The bounding box checking for hits</param>
		/// <returns>A list of Vector3 positions for where Rays begin</returns>
		private IEnumerable<Vector3> BuildOriginList(
			Directions direction,
			BoundingBoxProperties boundingBox
		) {
			Vector3[] originList;
			float extraRays;

			float addedInset =
				direction == Directions.Up ? boundingBox.RayInsetUp :
				direction == Directions.Right ? boundingBox.RayInsetRight :
				direction == Directions.Down ? boundingBox.RayInsetDown :
				direction == Directions.Left ? boundingBox.RayInsetLeft :
				throw new ArgumentOutOfRangeException();

			addedInset *= GameProperties.PixelSize;

			Vector2 boundingBoxSize = new Vector2(
				BoomerangUtils.RoundToPixelPerfection(boundingBox.RealSize.x),
				BoomerangUtils.RoundToPixelPerfection(boundingBox.RealSize.y)
			);
			Vector2 boundingBoxOffset = new Vector2(
				BoomerangUtils.RoundToPixelPerfection(boundingBox.RealOffset.x),
				BoomerangUtils.RoundToPixelPerfection(boundingBox.RealOffset.y)
			);

			if (direction == Directions.Up || direction == Directions.Down) {
				if (direction == Directions.Up) {
					originList = new Vector3[boundingBox.RayCountUp];
					float yPosition = RealPosition.y + boundingBoxOffset.y + boundingBoxSize.y / 2 - RaycastSkinWidth -
					                  addedInset;
					float middleX = RealPosition.x + boundingBoxOffset.x;
					extraRays = boundingBox.RayCountUp - 2;

					if (boundingBox.RayCountUp == 1) {
						originList[0] = new Vector3(middleX, yPosition);
					} else {
						originList[0] = new Vector3(
							middleX - boundingBoxSize.x / 2 +
							boundingBox.RayInsetFirstUp * GameProperties.PixelSize + RaycastSkinWidth, yPosition);
						originList[1] = new Vector3(
							middleX + boundingBoxSize.x / 2 -
							boundingBox.RayInsetLastUp * GameProperties.PixelSize - RaycastSkinWidth, yPosition);
					}
				} else {
					originList = new Vector3[boundingBox.RayCountDown];
					float yPosition = RealPosition.y + boundingBoxOffset.y - boundingBoxSize.y / 2 +
					                  RaycastSkinWidth + addedInset;
					float middleX = RealPosition.x + boundingBoxOffset.x;
					extraRays = boundingBox.RayCountDown - 2;

					if (boundingBox.RayCountDown == 1) {
						originList[0] = new Vector3(middleX, yPosition);
					} else {
						originList[0] = new Vector3(
							middleX - boundingBoxSize.x / 2 +
							boundingBox.RayInsetFirstDown * GameProperties.PixelSize + RaycastSkinWidth,
							yPosition);
						originList[1] = new Vector3(
							middleX + boundingBoxSize.x / 2 -
							boundingBox.RayInsetLastDown * GameProperties.PixelSize - RaycastSkinWidth, yPosition);
					}
				}

				if (extraRays > 0) {
					float newPointSpacing = (originList[1].x - originList[0].x) / (extraRays + 1);

					for (int j = 1; j <= extraRays; j++) {
						originList[j + 1] = originList[0] + new Vector3(newPointSpacing * j, 0, 0);
					}

					Array.Sort(originList, (a, b) => a.x.CompareTo(b.x));
				}
			} else {
				if (direction == Directions.Right) {
					originList = new Vector3[boundingBox.RayCountRight];
					float xPosition = RealPosition.x + boundingBoxOffset.x + boundingBoxSize.x / 2 - RaycastSkinWidth -
					                  addedInset;
					float middleY = RealPosition.y + boundingBoxOffset.y;
					extraRays = boundingBox.RayCountRight - 2;

					if (boundingBox.RayCountRight == 1) {
						originList[0] = new Vector3(xPosition, middleY);
					} else {
						originList[0] = new Vector3(xPosition,
							middleY + boundingBoxSize.y / 2 -
							boundingBox.RayInsetFirstRight * GameProperties.PixelSize - RaycastSkinWidth);
						originList[1] = new Vector3(xPosition,
							middleY - boundingBoxSize.y / 2 +
							boundingBox.RayInsetLastRight * GameProperties.PixelSize + RaycastSkinWidth);
					}
				} else {
					originList = new Vector3[boundingBox.RayCountLeft];
					float xPosition = RealPosition.x + boundingBoxOffset.x - boundingBoxSize.x / 2 + RaycastSkinWidth +
					                  addedInset;
					float middleY = RealPosition.y + boundingBoxOffset.y;
					extraRays = boundingBox.RayCountLeft - 2;

					if (boundingBox.RayCountLeft == 1) {
						originList[0] = new Vector3(xPosition, middleY);
					} else {
						originList[0] = new Vector3(xPosition,
							middleY + boundingBoxSize.y / 2 -
							boundingBox.RayInsetFirstLeft * GameProperties.PixelSize - RaycastSkinWidth);
						originList[1] = new Vector3(xPosition,
							middleY - boundingBoxSize.y / 2 +
							boundingBox.RayInsetLastLeft * GameProperties.PixelSize + RaycastSkinWidth);
					}
				}

				if (extraRays > 0) {
					float newPointSpacing = (originList[1].y - originList[0].y) / (extraRays + 1);

					for (int j = 1; j <= extraRays; j++) {
						originList[j + 1] = originList[0] + new Vector3(0, newPointSpacing * j, 0);
					}

					Array.Sort(originList, (b, a) => a.y.CompareTo(b.y));
				}
			}

			return originList;
		}

		private void CastRays(
			IEnumerable<Vector3> originList,
			Directions direction,
			Vector2 rayDirection,
			float rayLength,
			List<List<RaycastHit2D>> raysInThisDirection
		) {
			foreach (Vector3 originPoint in originList) {
				Debug.DrawLine(originPoint, originPoint + (Vector3) (rayDirection * rayLength), Color.magenta);

				RaycastHit2D[] raycastHit2Ds = new RaycastHit2D[200];

				int layerMask = 1 << LayerMask.NameToLayer("Actor") |
				                1 << LayerMask.NameToLayer("Solid") |
				                1 << LayerMask.NameToLayer("SolidOnTop") |
				                1 << LayerMask.NameToLayer("SolidOnBottom") |
				                1 << LayerMask.NameToLayer("SolidOnRight") |
				                1 << LayerMask.NameToLayer("SolidOnLeft");

				int hitCount = Physics2D.RaycastNonAlloc(new Vector2(originPoint.x, originPoint.y), rayDirection,
					raycastHit2Ds, rayLength, layerMask);

				raysInThisDirection.Add(new List<RaycastHit2D>());

				if (hitCount > 0) {
					IterateRayHits(hitCount, raycastHit2Ds, direction, raysInThisDirection.Last());
				}
			}
		}

		private void IterateRayHits(
			int hitCount,
			IReadOnlyList<RaycastHit2D> raycastHit2Ds,
			Directions direction,
			List<RaycastHit2D> rayDataCollection
		) {
			for (int hitIndex = 0; hitIndex < hitCount; hitIndex++) {
				RaycastHit2D hit = raycastHit2Ds[hitIndex];

				RaycastHit2D shortestSolidHit = new RaycastHit2D {
					distance = 2000f
				};

				if (!hit || !hit.collider || hit.collider.gameObject == GameObject) {
					continue;
				}

				Collider2D hitCollider = hit.collider;
				GameObject hitGameObject = hitCollider.gameObject;

				hit.distance -= RaycastSkinWidth;

				if (CollidesWithGeometry) {
					MapManager.MapTilesByGameObject.TryGetValue(hitGameObject.transform.parent.gameObject,
						out TileBehavior hitTile);

					if (hitTile &&
					    hit.distance < shortestSolidHit.distance &&
					    hitGameObject.layer == LayerMask.NameToLayer("Solid") ||
					    hitGameObject.layer == LayerMask.NameToLayer("SolidOnTop") && direction == Directions.Down ||
					    hitGameObject.layer == LayerMask.NameToLayer("SolidOnBottom") && direction == Directions.Up ||
					    hitGameObject.layer == LayerMask.NameToLayer("SolidOnRight") && direction == Directions.Left ||
					    hitGameObject.layer == LayerMask.NameToLayer("SolidOnLeft") && direction == Directions.Right
					) {
						shortestSolidHit = hit;
					}
				}

				if (CollidesWithActors) {
					Actor hitActor = MapManager.GetActorFromCatalogByGameObject(hitGameObject);

					if (hitActor != null && hitActor != this) {
						BoundingBoxProperties matchedBoundingBoxProperties = null;

						for (int j = 0; j < hitActor.BoundingBoxColliders.Count; j++) {
							if (hitActor.BoundingBoxColliders[j] == (BoxCollider2D) hitCollider) {
								matchedBoundingBoxProperties = hitActor.ActorProperties.BoundingBoxes[j];
								break;
							}
						}

						if (matchedBoundingBoxProperties != null && matchedBoundingBoxProperties.Enabled) {
							ActorBoundingBoxes newActorBoundingBox = new ActorBoundingBoxes {
								Actor = hitActor,
								BoundingBoxProperties = matchedBoundingBoxProperties,
								HitDistance = hit.distance
							};

							Dictionary<Directions, List<ActorBoundingBoxes>> boundingBoxHitByRayCollectionMap =
								new() {
									{Directions.Up, ActorBoundingBoxesHitByRaysUp},
									{Directions.Right, ActorBoundingBoxesHitByRaysRight},
									{Directions.Down, ActorBoundingBoxesHitByRaysDown},
									{Directions.Left, ActorBoundingBoxesHitByRaysLeft}
								};

							if (boundingBoxHitByRayCollectionMap[direction].IndexOf(newActorBoundingBox) == -1) {
								boundingBoxHitByRayCollectionMap[direction].Add(newActorBoundingBox);
							}

							foreach (string flag in matchedBoundingBoxProperties.Flags) {
								if (
									hit.distance < shortestSolidHit.distance &&
									flag == "Solid" ||
									direction == Directions.Down && flag == "SolidOnTop" ||
									direction == Directions.Left && flag == "SolidOnRight" ||
									direction == Directions.Up && flag == "SolidOnBottom" ||
									direction == Directions.Right && flag == "SolidOnLeft"
								) {
									shortestSolidHit = hit;
								}
							}
						}
					}
				}

				if (Math.Abs(shortestSolidHit.distance) < 0.001f) {
					shortestSolidHit.distance = 0f;
				}

				if (!(hitGameObject.layer == LayerMask.NameToLayer("Actor") && shortestSolidHit.distance >= 2000)) {
					rayDataCollection.Add(shortestSolidHit);	
				}
				
			}
		}

		/// <summary>
		/// Checks to see if the Actor meets the criteria for being Grounded, and then set it if true
		/// </summary>
		public void SetIsGrounded() {
			if (Math.Abs(Velocity.y) < 0.001f) {
				SetVelocity(Velocity.x, 0);
			}

			if (Math.Abs(DistanceToSolidDown) < 0.01 && CanBeGrounded) {
				SetVelocity(Velocity.x, 0);
				IsGrounded = true;
			} else {
				IsGrounded = false;
			}
		}

		/// <summary>
		/// Tells the StateMachine to set the Current State to the queued NextState
		/// </summary>
		public void UpdateState() {
			StateMachine?.UpdateState();
		}

		/// <summary>
		/// Tells the StateMachine to Process the Current State's ProcessSetUpFrameState method
		/// </summary>
		public void ProcessSetUpFrameState() {
			StateMachine?.ProcessSetUpFrameState();
		}

		/// <summary>
		/// Tells the StateMachine to Process the Current State's ProcessState method
		/// </summary>
		public void ProcessState() {
			if (StateMachine?.GetCurrentState() != null) {
				StateMachine.ProcessState();
			}
		}

		/// <summary>
		/// Tells the StateMachine to Process the Current State's ProcessPostFrameStates method
		/// </summary>
		public void ProcessPostFrameStates() {
			StateMachine?.ProcessPostFrameState();
		}

		/// <summary>
		/// Tells the StateMachine to Process the Triggers for State Modifications Methods, Weapons, and Animations
		/// </summary>
		public void ProcessTriggers() {
			((ActorState) StateMachine.GetCurrentState()).ProcessTriggers();
		}

		/// <summary>
		/// Tells the StateMachine to Process the Triggers for Transitioning to another State
		/// </summary>
		public void ProcessTransitionTriggers() {
			((ActorState) StateMachine.GetCurrentState()).ProcessTransitionTriggers();
		}

		/// <summary>
		/// Adds the Actor's Velocity.y to its RealPosition
		/// </summary>
		public void ApplyVerticalVelocity() {
			float newYDistance = Velocity.y * GameProperties.PixelSize;
			newYDistance = ConstrainVerticalVelocity(newYDistance);

			RealPosition += new Vector3(0, newYDistance, 0);
		}

		/// <summary>
		/// Adds the Actor's Velocity.x to its RealPosition
		/// </summary>
		public void ApplyHorizontalVelocity() {
			float newXDistance = Velocity.x * GameProperties.PixelSize;
			newXDistance = ConstrainHorizontalVelocity(newXDistance);

			RealPosition += new Vector3(newXDistance, 0, 0);
		}

		/// <summary>
		/// Constrains the vertical offset when applying the vertical velocity to the RealPosition based on the
		/// Distance to Solid objects found in the raycasting 
		/// </summary>
		/// <param name="distance">The distance to be added</param>
		/// <returns>The constrained distance</returns>
		private float ConstrainVerticalVelocity(float distance) {
			if (distance > 0f && distance >= DistanceToSolidUp) {
				distance = DistanceToSolidUp;
			} else if (distance < 0f && -distance >= DistanceToSolidDown) {
				distance = -DistanceToSolidDown;
			}

			return distance;
		}

		/// <summary>
		/// Constrains the horizontal offset when applying the horizontal velocity to the RealPosition based on the
		/// Distance to Solid objects found in the raycasting 
		/// </summary>
		/// <param name="distance">The distance to be added</param>
		/// <returns>The constrained distance</returns>
		private float ConstrainHorizontalVelocity(float distance) {
			if (distance > 0f && distance >= DistanceToSolidRight) {
				distance = DistanceToSolidRight;
			} else if (distance < 0f && -distance >= DistanceToSolidLeft) {
				distance = -DistanceToSolidLeft;
			}

			return distance;
		}

		/// <summary>
		/// Snaps the physical location of the Actor to align to the pixel-grid for pixel-perfect rendering
		/// </summary>
		public void SnapToPositioning() {
			Transform.localPosition = BoomerangUtils.RoundToPixelPerfection(RealPosition);
		}

		/// <summary>
		/// Executes each of this Actor's Interaction Events
		/// </summary>
		public void ExecuteInteractionEvents() {
			foreach (ActorInteractionEvent interactionEvent in ActorProperties.InteractionEvents) {
				if (interactionEvent.ActorEvents.Count == 0 ||
				    interactionEvent.HasExecuted ||
				    !interactionEvent.Enabled
				) {
					continue;
				}

				List<Actor> filteredActors = new List<Actor>(MapManager.GetActorCatalog());
				bool allTriggersMet = true;
				bool actorFilterCountMet = true;
				bool usesActorFilters = interactionEvent.AfterFinderFilters.Count > 0;
				bool usesTriggers = interactionEvent.Triggers.Count > 0;

				if (usesActorFilters) {
					foreach (ActorFinderFilter actorFinderFilter in interactionEvent.AfterFinderFilters) {
						filteredActors = actorFinderFilter.FetchMatchingActors(this, filteredActors);
					}

					actorFilterCountMet =
						interactionEvent.FoundActorsComparison == ValueComparison.LessThan &&
						filteredActors.Count < interactionEvent.FoundActorsCount ||
						interactionEvent.FoundActorsComparison == ValueComparison.LessThanOrEqual &&
						filteredActors.Count <= interactionEvent.FoundActorsCount ||
						interactionEvent.FoundActorsComparison == ValueComparison.Equal &&
						filteredActors.Count == interactionEvent.FoundActorsCount ||
						interactionEvent.FoundActorsComparison == ValueComparison.GreaterThanOrEqual &&
						filteredActors.Count >= interactionEvent.FoundActorsCount ||
						interactionEvent.FoundActorsComparison == ValueComparison.GreaterThan &&
						filteredActors.Count > interactionEvent.FoundActorsCount;
				}

				if (usesTriggers) {
					foreach (ActorTrigger trigger in interactionEvent.Triggers) {
						if (!trigger.IsTriggered(this, StateMachine.GetCurrentState())) {
							allTriggersMet = false;
						}
					}
				}

				if (!allTriggersMet || !actorFilterCountMet) {
					continue;
				}

				ExecuteInteractionEvent(interactionEvent, usesActorFilters, filteredActors);
			}
		}

		/// <summary>
		/// Executes one of this Actor's specific Interaction Events
		/// </summary>
		public void ExecuteInteractionEvent(
			ActorInteractionEvent actorInteractionEvent,
			bool usesActorFilters,
			List<Actor> filteredActors = null
		) {
			if (actorInteractionEvent.ActorEvents.Count == 0 ||
			    actorInteractionEvent.HasExecuted ||
			    !actorInteractionEvent.Enabled
			) {
				return;
			}

			float latestStartTime = 0;
			actorInteractionEvent.HasExecuted = true;

			foreach (ActorEvent actorEvent in actorInteractionEvent.ActorEvents) {
				if (actorEvent.StartTime > latestStartTime) {
					latestStartTime = actorEvent.StartTime;
				}

				if (actorEvent.HasExecuted) {
					continue;
				}

				ActorEvent actorEventLocal = actorEvent;

				if (usesActorFilters && actorEventLocal.AffectFilteredActors && filteredActors != null) {
					actorEventLocal.HasExecuted = true;
					QueuedActions.Add(
						GlobalTimeManager.PerformAfter(
							actorEventLocal.StartTime,
							() => {
								foreach (Actor actor in filteredActors) {
									actorEventLocal.ApplyOutcome(actor, this);
								}
							}
						)
					);
				} else {
					actorEventLocal.HasExecuted = true;
					QueuedActions.Add(
						GlobalTimeManager.PerformAfter(
							actorEventLocal.StartTime,
							() => { actorEventLocal.ApplyOutcome(this, this); }
						)
					);
				}
			}

			ActorInteractionEvent actorInteractionEventLocal = actorInteractionEvent;

			QueuedActions.Add(
				GlobalTimeManager.PerformAfter(latestStartTime, () => {
					actorInteractionEventLocal.HasExecuted = false;
					foreach (ActorEvent actorEvent in actorInteractionEventLocal.ActorEvents) {
						actorEvent.HasExecuted = false;
					}
				})
			);
		}

		/// <summary>
		/// Takes care of anything that needs to be done at the end of this Actor's Frame
		/// </summary>
		public void EndOfFrameManagement() {
			PreviousFacingDirection = _facingDirection;
			RemoveActivatedQueuedActions();
		}

		/// <summary>
		/// Removes all activated queued actions that have executed from the list
		/// </summary>
		private void RemoveActivatedQueuedActions() {
			List<int> queuedActionsToRemove = new List<int>();

			for (int index = 0; index < QueuedActions.Count; index++) {
				GlobalTimeManager.QueuedAction queuedAction = QueuedActions[index];
				if (!GlobalTimeManager.QueuedActionExists(queuedAction)) {
					queuedActionsToRemove.Add(index);
				}
			}

			queuedActionsToRemove.Reverse();

			foreach (int index in queuedActionsToRemove) {
				QueuedActions.RemoveAt(index);
			}
		}
		
		/// <summary>
		/// Kills this actor. Calls MapManager to remove it from the Actor Catalogs
		/// </summary>
		public void Kill() {
			Boomerang2D.ActorManager.PutActorInPool(this);
		}
	}
}