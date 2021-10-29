using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class UserInputDown : ActorTrigger {
		[SerializeField] private UserInputDownProperties _properties;
		[SerializeField] private bool _axisDownLastFrame = true;

		public UserInputDown(ActorTriggerProperties actorTriggerProperties) {
			_properties = (UserInputDownProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			InputType inputType = _properties.InputType;
			string inputValue = _properties.InputValue;

			if (inputType == InputType.Axis) {
				bool axisIsPositive = true;

				if (inputValue.Length > 0 &&
				    inputValue[inputValue.Length - 1] == '-' ||
				    inputValue[inputValue.Length - 1] == '+'
				) {
					axisIsPositive = inputValue[inputValue.Length - 1] == '+';
					inputValue = inputValue.Substring(0, inputValue.Length - 1);
				}

				bool axisDown = axisIsPositive
					? !_axisDownLastFrame && Input.GetAxisRaw(inputValue) > 0f
					: !_axisDownLastFrame && Input.GetAxisRaw(inputValue) < 0f;

				_axisDownLastFrame = Math.Abs(Input.GetAxisRaw(inputValue)) > 0.01f;

				return axisDown;
			}

			if (inputType == InputType.Button) {
				if (Input.GetButtonDown(inputValue)) {
					return true;
				}
			}

			if (inputType == InputType.Key) {
				if (Input.GetKeyDown(inputValue)) {
					return true;
				}
			}

			return false;
		}
	}
}