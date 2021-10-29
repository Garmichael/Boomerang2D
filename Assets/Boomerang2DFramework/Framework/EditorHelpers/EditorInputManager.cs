using UnityEngine;

namespace Boomerang2DFramework.Framework.EditorHelpers {
	public static class EditorInputManager {
#if UNITY_EDITOR
		private static bool _mouseLeftDownLastFrame;
		private static bool _mouseRightDownLastFrame;
		private static bool _mouseMiddleDownLastFrame;

		private static bool _controlDownLastFrame;
		private static bool _shiftDownLastFrame;
		private static bool _altDownLastFrame;

		private static Vector2 _mousePositionLastFrame;

		public struct InputValues {
			public bool MouseLeftUp;
			public bool MouseLeftDown;
			public bool MouseLeftHeld;

			public bool MouseRightUp;
			public bool MouseRightDown;
			public bool MouseRightHeld;

			public bool MouseMiddleUp;
			public bool MouseMiddleDown;
			public bool MouseMiddleHeld;

			public bool MouseMoved;
			public Vector2 MouseMovementDistance;

			public bool ScrolledUp;
			public bool ScrolledDown;

			public bool ControlHeld;
			public bool ShiftHeld;
			public bool AltHeld;

			public bool ControlDown;
			public bool ShiftDown;
			public bool AltDown;

			public Vector2 MouseLocation;
			public KeyCode KeyPressed;
		}

		public static InputValues GetInput() {
			InputValues inputValues = new InputValues();

			EventType eventType = Event.current.type;

			if (_mouseLeftDownLastFrame) {
				inputValues.MouseLeftHeld = true;
			}

			if (_mouseRightDownLastFrame) {
				inputValues.MouseRightHeld = true;
			}

			if (_mouseMiddleDownLastFrame) {
				inputValues.MouseMiddleHeld = true;
			}

			if (Event.current.button == 0) {
				if (eventType == EventType.MouseDown) {
					inputValues.MouseLeftDown = true;
					_mouseLeftDownLastFrame = true;
				}

				if (eventType == EventType.MouseUp) {
					inputValues.MouseLeftHeld = false;
					inputValues.MouseLeftUp = true;
					_mouseLeftDownLastFrame = false;
				}
			}

			if (Event.current.button == 1) {
				if (eventType == EventType.MouseDown) {
					inputValues.MouseRightDown = true;
					inputValues.MouseRightHeld = true;
					_mouseRightDownLastFrame = true;
				}

				if (eventType == EventType.MouseUp) {
					inputValues.MouseRightHeld = false;
					inputValues.MouseRightUp = true;
					_mouseRightDownLastFrame = false;
				}
			}

			if (Event.current.button == 2) {
				if (eventType == EventType.MouseDown) {
					inputValues.MouseMiddleDown = true;
					inputValues.MouseMiddleHeld = true;
					_mouseMiddleDownLastFrame = true;
				}

				if (eventType == EventType.MouseUp) {
					inputValues.MouseMiddleHeld = false;
					inputValues.MouseMiddleUp = true;
					_mouseMiddleDownLastFrame = false;
				}
			}

			if (eventType == EventType.ScrollWheel) {
				if (Event.current.delta.y < 0) {
					inputValues.ScrolledUp = true;
				}

				if (Event.current.delta.y > 0) {
					inputValues.ScrolledDown = true;
				}
			}

			inputValues.MouseLocation = Event.current.mousePosition;

			inputValues.MouseMoved = inputValues.MouseLocation != _mousePositionLastFrame;
			inputValues.MouseMovementDistance = inputValues.MouseLocation - _mousePositionLastFrame;
			_mousePositionLastFrame = inputValues.MouseLocation;

			inputValues.KeyPressed = Event.current.keyCode;

			inputValues.ShiftHeld = Event.current.shift;
			inputValues.ControlHeld = Event.current.control || Event.current.command;
			inputValues.AltHeld = Event.current.alt;

			if (inputValues.ShiftHeld && !_shiftDownLastFrame) {
				inputValues.ShiftDown = true;
				_shiftDownLastFrame = true;
			} else if (!inputValues.ShiftHeld) {
				_shiftDownLastFrame = false;
			}

			if (inputValues.ControlDown && !_controlDownLastFrame) {
				inputValues.ControlDown = true;
				_controlDownLastFrame = true;
			} else if (!inputValues.ControlHeld) {
				_controlDownLastFrame = false;
			}

			if (inputValues.AltDown && !_altDownLastFrame) {
				inputValues.AltDown = true;
				_altDownLastFrame = true;
			} else if (!inputValues.AltHeld) {
				_altDownLastFrame = false;
			}

			return inputValues;
		}

		public static void LoseFocus() {
			GUIUtility.keyboardControl = 0;
		}
#endif
	}
}