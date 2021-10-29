using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	public class MoveOrtho : ActorState {
		private MoveOrthoProperties MyStateProperties => (MoveOrthoProperties) StateProperties;

		public MoveOrtho(Actor actor, StateMachine stateMachine, StateProperties stateProperties) {
			Actor = actor;
			MyStateMachine = stateMachine;
			StateProperties = stateProperties;
		}

		public override void OnEnterState() {
			base.OnEnterState();
			if (MyStateProperties.ForceFacingDirectionOnEntry) {
				Actor.FacingDirection = MyStateProperties.FacingDirectionOnEntry;
			}
		}

		public override void ProcessState() {
			base.ProcessState();

			float analogValueRight = BoomerangUtils.GetInputResultValue(
				MyStateProperties.MoveRightInputType,
				MyStateProperties.MoveRightInputValue
			);

			float analogValueLeft = BoomerangUtils.GetInputResultValue(
				MyStateProperties.MoveLeftInputType,
				MyStateProperties.MoveLeftInputValue
			);

			float analogValueUp = BoomerangUtils.GetInputResultValue(
				MyStateProperties.MoveUpInputType,
				MyStateProperties.MoveUpInputValue
			);

			float analogValueDown = BoomerangUtils.GetInputResultValue(
				MyStateProperties.MoveDownInputType,
				MyStateProperties.MoveDownInputValue
			);

			bool isWalkingRight = analogValueRight > 0;
			bool isWalkingLeft = analogValueLeft > 0;
			bool isWalkingUp = analogValueUp > 0;
			bool isWalkingDown = analogValueDown > 0;

			if (isWalkingRight) {
				if (MyStateProperties.WalkSnap == MoveOrthoProperties.WalkSnapEnum.FourWay) {
					isWalkingDown = false;
					isWalkingUp = false;
				}

				isWalkingLeft = false;

				if (MyStateProperties.UpdateFacingDirection) {
					if (isWalkingUp) {
						Actor.FacingDirection = Directions.UpRight;
					} else if (isWalkingDown) {
						Actor.FacingDirection = Directions.DownRight;
					} else {
						Actor.FacingDirection = Directions.Right;
					}
				}
			} else if (isWalkingLeft) {
				if (MyStateProperties.WalkSnap == MoveOrthoProperties.WalkSnapEnum.FourWay) {
					isWalkingDown = false;
					isWalkingUp = false;
				}

				if (MyStateProperties.UpdateFacingDirection) {
					if (isWalkingUp) {
						Actor.FacingDirection = Directions.UpLeft;
					} else if (isWalkingDown) {
						Actor.FacingDirection = Directions.DownLeft;
					} else {
						Actor.FacingDirection = Directions.Left;
					}
				}
			}

			if (isWalkingUp) {
				if (MyStateProperties.WalkSnap == MoveOrthoProperties.WalkSnapEnum.FourWay) {
					isWalkingRight = false;
					isWalkingLeft = false;
				}

				if (MyStateProperties.UpdateFacingDirection) {
					if (isWalkingLeft) {
						Actor.FacingDirection = Directions.UpLeft;
					} else if (isWalkingRight) {
						Actor.FacingDirection = Directions.UpRight;
					} else {
						Actor.FacingDirection = Directions.Up;
					}
				}

				isWalkingDown = false;
			} else if (isWalkingDown) {
				if (MyStateProperties.WalkSnap == MoveOrthoProperties.WalkSnapEnum.FourWay) {
					isWalkingRight = false;
					isWalkingLeft = false;
				}

				if (MyStateProperties.UpdateFacingDirection) {
					if (isWalkingLeft) {
						Actor.FacingDirection = Directions.DownLeft;
					} else if (isWalkingRight) {
						Actor.FacingDirection = Directions.DownRight;
					} else {
						Actor.FacingDirection = Directions.Down;
					}
				}
			}

			if (MyStateProperties.AutoWalk) {
				if (MyStateProperties.MoveInFacingDirection) {
					isWalkingUp = isWalkingLeft = isWalkingDown = isWalkingRight = false;

					if (Actor.FacingDirection == Directions.Right ||
					    Actor.FacingDirection == Directions.UpRight ||
					    Actor.FacingDirection == Directions.DownRight
					) {
						isWalkingRight = true;
					} else if (Actor.FacingDirection == Directions.Left ||
					           Actor.FacingDirection == Directions.UpLeft ||
					           Actor.FacingDirection == Directions.DownLeft
					) {
						isWalkingLeft = true;
					}

					if (Actor.FacingDirection == Directions.Up ||
					    Actor.FacingDirection == Directions.UpLeft ||
					    Actor.FacingDirection == Directions.UpRight
					) {
						isWalkingUp = true;
					} else if (Actor.FacingDirection == Directions.Down ||
					           Actor.FacingDirection == Directions.DownLeft ||
					           Actor.FacingDirection == Directions.DownRight
					) {
						isWalkingDown = true;
					}
				} else {
					isWalkingUp = isWalkingLeft = isWalkingDown = isWalkingRight = false;

					switch (MyStateProperties.MoveDirection) {
						case Directions.Up:
							isWalkingUp = true;
							break;
						case Directions.Right:
							isWalkingRight = true;
							break;
						case Directions.Down:
							isWalkingDown = true;
							break;
						case Directions.Left:
							isWalkingLeft = true;
							break;
						case Directions.UpRight:
							isWalkingUp = isWalkingRight = true;
							break;
						case Directions.DownRight:
							isWalkingDown = isWalkingRight = true;
							break;
						case Directions.UpLeft:
							isWalkingUp = isWalkingLeft = true;
							break;
						case Directions.DownLeft:
							isWalkingDown = isWalkingLeft = true;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					if (MyStateProperties.UpdateFacingDirection) {
						if (isWalkingUp) {
							Actor.FacingDirection = Directions.Up;
						} else if (isWalkingRight) {
							Actor.FacingDirection = Directions.Right;
						} else if (isWalkingDown) {
							Actor.FacingDirection = Directions.Down;
						} else {
							Actor.FacingDirection = Directions.Left;
						}

						if (MyStateProperties.WalkSnap == MoveOrthoProperties.WalkSnapEnum.EightWay) {
							if (isWalkingUp && isWalkingRight) {
								Actor.FacingDirection = Directions.UpRight;
							}

							if (isWalkingDown && isWalkingRight) {
								Actor.FacingDirection = Directions.DownRight;
							}

							if (isWalkingUp && isWalkingLeft) {
								Actor.FacingDirection = Directions.UpLeft;
							}

							if (isWalkingDown && isWalkingLeft) {
								Actor.FacingDirection = Directions.DownLeft;
							}
						}
					}
				}
			}

			float factoredAcceleration = MyStateProperties.Acceleration.BuildValue(Actor);
			float factoredDeceleration = MyStateProperties.Deceleration.BuildValue(Actor);
			float factoredSkidDeceleration = MyStateProperties.SkidDeceleration.BuildValue(Actor);

			if (isWalkingRight) {
				if (MyStateProperties.AnalogSpeed && !MyStateProperties.AutoWalk) {
					factoredAcceleration *= analogValueRight;
				}

				float factoredDifference = Actor.Velocity.x >= 0
					? factoredAcceleration
					: factoredSkidDeceleration;

				Actor.OffsetVelocityX(factoredDifference);
			} else if (isWalkingLeft) {
				if (MyStateProperties.AnalogSpeed) {
					factoredAcceleration *= analogValueLeft;
				}

				float factoredDifference = Actor.Velocity.x <= 0
					? factoredAcceleration
					: factoredSkidDeceleration;

				Actor.OffsetVelocityX(-factoredDifference);
			} else if (Actor.Velocity.x > 0f) {
				Actor.OffsetVelocityX(-factoredDeceleration);
				if (Actor.Velocity.x < 0f) {
					Actor.SetVelocityX(0);
				}
			} else if (Actor.Velocity.x < 0f) {
				Actor.OffsetVelocityX(factoredDeceleration);
				if (Actor.Velocity.x > 0f) {
					Actor.SetVelocityX(0);
				}
			}

			if (isWalkingUp) {
				if (MyStateProperties.AnalogSpeed) {
					factoredAcceleration *= analogValueUp;
				}

				float factoredDifference = Actor.Velocity.y >= 0
					? factoredAcceleration
					: factoredSkidDeceleration;

				Actor.OffsetVelocityY(factoredDifference);
			} else if (isWalkingDown) {
				if (MyStateProperties.AnalogSpeed) {
					factoredAcceleration *= analogValueDown;
				}

				float factoredDifference = Actor.Velocity.y <= 0
					? factoredAcceleration
					: factoredSkidDeceleration;

				Actor.OffsetVelocityY(-factoredDifference);
			} else if (Actor.Velocity.y > 0f) {
				Actor.OffsetVelocityY(-factoredDeceleration);
				if (Actor.Velocity.y < 0f) {
					Actor.SetVelocityY(0);
				}
			} else if (Actor.Velocity.y < 0f) {
				Actor.OffsetVelocityY(factoredDeceleration);
				if (Actor.Velocity.y > 0f) {
					Actor.SetVelocityY(0);
				}
			}

			float maxSpeed = MyStateProperties.MaxSpeed.BuildValue(Actor);

			Actor.SetVelocity(
				BoomerangUtils.ClampValue(Actor.Velocity.x, -maxSpeed, maxSpeed),
				BoomerangUtils.ClampValue(Actor.Velocity.y, -maxSpeed, maxSpeed)
			);

			if (MyStateProperties.ConstrictDiagonalSpeed && Vector2.Distance(Vector2.zero, Actor.Velocity) > maxSpeed) {
				float mag = Mathf.Sqrt(
					Actor.Velocity.x * Actor.Velocity.x +
					Actor.Velocity.y * Actor.Velocity.y
				);

				if (mag > 0) {
					Actor.SetVelocity(
						Actor.Velocity.x / mag * maxSpeed,
						Actor.Velocity.y / mag * maxSpeed
					);
				}
			}

			Actor.SetVelocityOrder(
				Actor.Velocity.x > Actor.Velocity.y
					? ActorVelocityOrder.VerticalFirst
					: ActorVelocityOrder.HorizontalFirst
			);

			if (isWalkingLeft || isWalkingRight) {
				List<List<List<RaycastHit2D>>> rayCollectionToUse = isWalkingLeft
					? Actor.RayDataLeft
					: Actor.RayDataRight;

				float yOffset = 0;
				foreach (List<List<RaycastHit2D>> rays in rayCollectionToUse) {
					foreach (List<RaycastHit2D> rayHits in rays) {
						if (rayHits.Count > 0 && Math.Abs(rayHits[0].distance) < 0.001f) {
							bool slopesUp = rayHits[0].normal.y > 0;
							float angleOfSlope = Vector3.Angle(rayHits[0].normal, Vector2.right) - 90;

							if (Mathf.Abs(angleOfSlope) <= MyStateProperties.MaxClimbableSlopeAngle) {
								yOffset = Mathf.Tan(angleOfSlope * Mathf.Deg2Rad) *
								          Actor.Velocity.x *
								          (slopesUp ? 1 : -1);
							}
						}
					}
				}

				if (Math.Abs(yOffset) > 0.001f) {
					Actor.SetVelocityY(yOffset);
					Actor.SetVelocityOrder(ActorVelocityOrder.VerticalFirst);
				}
			} else if (isWalkingUp || isWalkingDown) {
				List<List<List<RaycastHit2D>>> rayCollectionToUse = isWalkingUp ? Actor.RayDataUp : Actor.RayDataDown;

				float xOffset = 0;
				foreach (List<List<RaycastHit2D>> rays in rayCollectionToUse) {
					foreach (List<RaycastHit2D> rayHits in rays) {
						if (rayHits.Count > 0 && Math.Abs(rayHits[0].distance) < 0.001f) {
							bool slopesUp = rayHits[0].normal.x > 0;
							float angleOfSlope = Vector3.Angle(rayHits[0].normal, Vector2.up) - 90;

							if (Mathf.Abs(angleOfSlope) <= MyStateProperties.MaxClimbableSlopeAngle) {
								xOffset = Mathf.Tan(angleOfSlope * Mathf.Deg2Rad) *
								          Actor.Velocity.y *
								          (slopesUp ? 1 : -1);
							}
						}
					}
				}

				if (Math.Abs(xOffset) > 0.001f) {
					Actor.SetVelocityX(xOffset);
					Actor.SetVelocityOrder(ActorVelocityOrder.HorizontalFirst);
				}
			}
		}

		public override void ProcessPostFrameState() {
			if (MyStateProperties.GlueDistanceUp > 0) {
				Actor.GenerateRayData(Directions.Up);
				if (Actor.DistanceToSolidUp < MyStateProperties.GlueDistanceUp) {
					Actor.OffsetRealPositionY(Actor.DistanceToSolidUp);
				}
			}

			if (MyStateProperties.GlueDistanceDown > 0) {
				Actor.GenerateRayData(Directions.Down);
				if (Actor.DistanceToSolidDown < MyStateProperties.GlueDistanceDown) {
					Actor.OffsetRealPositionY(-Actor.DistanceToSolidDown);
				}
			}

			if (MyStateProperties.GlueDistanceRight > 0) {
				Actor.GenerateRayData(Directions.Right);
				if (Actor.DistanceToSolidRight < MyStateProperties.GlueDistanceRight) {
					Actor.OffsetRealPositionX(Actor.DistanceToSolidRight);
				}
			}

			if (MyStateProperties.GlueDistanceLeft > 0) {
				Actor.GenerateRayData(Directions.Left);
				if (Actor.DistanceToSolidLeft < MyStateProperties.GlueDistanceLeft) {
					Actor.OffsetRealPositionX(-Actor.DistanceToSolidLeft);
				}
			}
		}
	}
}