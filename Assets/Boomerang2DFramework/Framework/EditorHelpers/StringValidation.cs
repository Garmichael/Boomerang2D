using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Boomerang2DFramework.Framework.EditorHelpers {
	public static class StringValidation {
#if UNITY_EDITOR
		public struct StringValidationInfo {
			public bool IsValid;
			public string ValidationMessage;
		}

		public static StringValidationInfo GetValidity(string input, List<string> disallowedValues) {
			bool isValidName = true;
			string validationMessage = "";

			for (int index = 0; index < disallowedValues.Count; index++) {
				disallowedValues[index] = disallowedValues[index].ToLower();
			}

			Regex allowedCharacters = new Regex(@"^[a-zA-Z0-9_.-]+$");

			if (!allowedCharacters.IsMatch(input)) {
				isValidName = false;
				validationMessage = "Special Characters not allowed";
			}

			if (input.Trim() == "") {
				isValidName = false;
				validationMessage = "Name cant be blank";
			}

			if (disallowedValues.IndexOf(input.ToLower().Trim()) > -1) {
				isValidName = false;
				validationMessage = "Name already in use";
			}

			if (input.Length > 0 && !char.IsLetter(input[0])) {
				isValidName = false;
				validationMessage = "Must start with Letter";
			}

			return new StringValidationInfo {
				IsValid = isValidName,
				ValidationMessage = validationMessage
			};
		}
#endif
	}
}