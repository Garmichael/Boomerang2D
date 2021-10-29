using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class UserInputNotHeld : ActorTrigger {
		[SerializeField] private UserInputNotHeldProperties _properties;

		public UserInputNotHeld(ActorTriggerProperties actorTriggerProperties) {
			_properties = (UserInputNotHeldProperties) actorTriggerProperties;
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

				return axisIsPositive
					? !(Input.GetAxis(inputValue) > 0f)
					: !(Input.GetAxis(inputValue) < 0f);
			}

			if (inputType == InputType.Button) {
				if (!Input.GetButton(inputValue)) {
					return true;
				}
			}

			if (inputType == InputType.Key) {
				if (!Input.GetKey(inputValue)) {
					return true;
				}
			}

			return false;
		}
	}
}