using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.IO;
using System.Reflection;
using UnityEngine.Experimental.Rendering;

public class ScreenCaptureWindow : EditorWindow
{
	const string BaseSaveKey = "uxtuno/ScreenCaptureWindow/";
	const string FolderPathSaveKey = BaseSaveKey + "FolderPathKey";
	const string FileNameSaveKey = BaseSaveKey + "FileNameKey";
	const string ShortCutKeySaveKey = BaseSaveKey + "ShortCutKey";
	const string IsPrefixDateSaveKey = BaseSaveKey + "IsPrefixDateSaveKey";

	[MenuItem("Window/Screen Capture Window")]
	static void Create()
	{
		var instance = GetWindow<ScreenCaptureWindow>();
	}

	[InitializeOnLoadMethod]
	static void initialize()
	{
		FolderPath = EditorPrefs.GetString(FolderPathSaveKey, Application.dataPath);
		FileName = EditorPrefs.GetString(FileNameSaveKey, FileName);
		ShortCutKey = (KeyCode)EditorPrefs.GetInt(ShortCutKeySaveKey, (int)ShortCutKey);
		IsPrefixDate = EditorPrefs.GetBool(IsPrefixDateSaveKey, false);

		FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
		EditorApplication.CallbackFunction functions = (EditorApplication.CallbackFunction)info.GetValue(null);
		functions += eventHandler;
		info.SetValue(null, (object)functions);
	}

	static string FolderPath;
	static KeyCode ShortCutKey = KeyCode.P;
	static string FileName = "ScreenShot";
	static bool IsPrefixDate = false;

	static void eventHandler()
	{
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == ShortCutKey)
		{
			capture();
		}
	}

	void OnDisable()
	{
	}

	void OnGUI()
	{
		using (var scope = new EditorGUI.ChangeCheckScope())
		{
			using (_ = new EditorGUILayout.HorizontalScope())
			{
				FolderPath = EditorGUILayout.TextField("Save Folder", FolderPath);

				if (GUILayout.Button("Select Folder", GUILayout.Width(96)))
				{
					FolderPath = EditorUtility.OpenFolderPanel("Pick a folder", Application.dataPath, "");
					EditorPrefs.SetString(FolderPathSaveKey, FolderPath);
				}

			}
			using (_ = new EditorGUILayout.HorizontalScope())
			{
				FileName = EditorGUILayout.TextField("File Name", FileName);
				IsPrefixDate = EditorGUILayout.ToggleLeft("Is Prefix Date", IsPrefixDate, GUILayout.Width(96));
			}
			ShortCutKey = (KeyCode)EditorGUILayout.EnumPopup("Shortcut Key", ShortCutKey, GUILayout.ExpandWidth(false));

			if (scope.changed)
			{
				EditorPrefs.SetString(FolderPathSaveKey, FolderPath);
				EditorPrefs.SetString(FileNameSaveKey, FileName);
				EditorPrefs.SetInt(ShortCutKeySaveKey, (int)ShortCutKey);
				EditorPrefs.SetBool(IsPrefixDateSaveKey, IsPrefixDate);
			}
		}

		if (GUILayout.Button("Capture"))
		{
			capture();
		}
	}

	static void capture()
	{
		var tempFileName = FileName;
		if (IsPrefixDate)
		{
			tempFileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + tempFileName;
		}
		var savePath = Path.Combine(FolderPath, tempFileName);

		savePath += ".png";

		ScreenCapture.CaptureScreenshot(savePath);

		string[] screenres = UnityStats.screenRes.Split('x');
		int width = int.Parse(screenres[0]);
		int height = int.Parse(screenres[1]);

		Debug.Log($"Save Screen Shot.\n Path({savePath})\n Size({width}, {height})");
	}
}
