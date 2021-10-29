using UnityEditor;
using UnityEngine;

namespace Editor {
	[InitializeOnLoad]
	internal class CompileTime : EditorWindow
	{
		private static bool _isTrackingTime;
		private static double _startTime;

		static CompileTime()
		{
			EditorApplication.update += Update;
			_startTime = PlayerPrefs.GetFloat("CompileStartTime", 0);
			if (_startTime > 0)
			{
				_isTrackingTime = true;
			}
		}


		private static void Update()
		{
			if (EditorApplication.isCompiling && !_isTrackingTime)
			{
				_startTime = EditorApplication.timeSinceStartup;
				PlayerPrefs.SetFloat("CompileStartTime", (float)_startTime);
				_isTrackingTime = true;
			}
			else if (!EditorApplication.isCompiling && _isTrackingTime)
			{
				double finishTime = EditorApplication.timeSinceStartup;
				_isTrackingTime = false;
				double compileTime = finishTime - _startTime;
				PlayerPrefs.DeleteKey("CompileStartTime");
				Debug.Log("Script compile time: " + compileTime.ToString("0.000") + "s");
			}
		}
	}
}