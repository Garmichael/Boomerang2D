using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.StateManagement {
	public class StateMachine {
		public readonly Dictionary<string, State> AllStates = new Dictionary<string, State>();
		private State _currentState;
		private State _nextState;
		private State _previousState;

		public State AddState(State state) {
			AllStates.Add(state.Name, state);
			return AllStates[state.Name];
		}

		public void ClearStates() {
			AllStates.Clear();
			_currentState = null;
			_previousState = null;
			_nextState = null;
		}
		
		public State GetState(string name) {
			return AllStates[name];
		}

		public State GetPreviousState() {
			return _previousState;
		}
		
		public State GetCurrentState() {
			return _currentState;
		}

		public void UpdateState() {
			if (_nextState != null) {
				SetState(_nextState);
			}
		}

		public void SetNextState(State newState) {
			_nextState = newState;
		}

		public void SetNextState(string newStateName) {
			if (AllStates.ContainsKey(newStateName)) {
				_nextState = AllStates[newStateName];
			}
		}

		private void SetState(State newState) {
			if (_currentState != null) {
				_currentState.OnExitState();
				_previousState = _currentState;
			}

			_currentState = newState;
			_currentState.OnEnterState();
			_nextState = null;


//			Debug.Log("========== NEW STATE: " + newState.Name + "==========");
		}

		public void ProcessSetUpFrameState() {
			_currentState?.ProcessSetUpFrameState();
		}

		public void ProcessState() {
			_currentState?.ProcessState();
		}

		public void ProcessPostFrameState() {
			_currentState?.ProcessPostFrameState();
		}
	}
}