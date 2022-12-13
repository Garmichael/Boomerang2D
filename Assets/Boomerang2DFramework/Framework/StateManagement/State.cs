using System;
using Boomerang2DFramework.Framework.GlobalTimeManagement;

namespace Boomerang2DFramework.Framework.StateManagement {
	/// <summary>
	/// Base State class for ActorStates and CameraStates
	/// </summary>
	[Serializable]
	public class State {
		public string Name = "Unnamed_State";
		public bool CompletedAction = false;
		protected StateProperties StateProperties;

		protected StateMachine MyStateMachine;

		public float TimeInState;

		/// <summary>
		/// Called once when this State is transitioned into
		/// </summary>
		public virtual void OnEnterState() {
			CompletedAction = false;
			TimeInState = 0f;
		}

		/// <summary>
		/// Called once when this State is transitioned out of
		/// </summary>
		public virtual void OnExitState() { }

		/// <summary>
		/// Called once at the beginning of the frame while this State is the Current State
		/// </summary>
		public virtual void ProcessSetUpFrameState() {
			TimeInState += GlobalTimeManager.DeltaTime;
		}

		/// <summary>
		/// Called once during the frame while this State is the Current State.
		/// This is where the main logic of the State lives
		/// </summary>
		public virtual void ProcessState() { }

		/// <summary>
		/// Called once at the end of the frame while this State is the Current State
		/// </summary>
		public virtual void ProcessPostFrameState() { }
	}

	/// <summary>
	/// An Attribute that registers a State's Method as a Modification Trigger
	/// </summary>
	public class CanBeCalledByModificationTriggerAttribute : Attribute { }
}