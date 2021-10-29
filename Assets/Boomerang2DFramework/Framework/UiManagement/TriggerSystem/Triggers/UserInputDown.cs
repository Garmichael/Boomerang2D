using System;
using Boomerang2DFramework.Framework.UiManagement.UiElements;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.TriggerSystem.Triggers {
	[Serializable]
	public class UserInputDown : HudObjectTrigger {
		[SerializeField] private UserInputDownProperties Properties;
		[SerializeField] private bool _axisDownLastFrame = true;

		public UserInputDown(HudObjectTriggerProperties properties) {
			Properties = (UserInputDownProperties) properties;
		}

		public override bool IsTriggered(HudObjectBehavior hudObjectBehavior) {
			InputType inputType = Properties.InputType;
			string inputValue = Properties.InputValue;

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
					? !_axisDownLastFrame && Input.GetAxis(inputValue) > 0f
					: !_axisDownLastFrame && Input.GetAxis(inputValue) < 0f;

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