using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;
using AlleyLabs.AutoBuild;
using System.IO;
using System.Linq;
public static class BuildCommand{
	private static string[] GetBuildScenes()
	{
		List<string> names = new List<string>();
		
		foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
		{
			if (e == null)
			{
				continue;
			}
			
			if (e.enabled)
			{
				names.Add(e.path);
			}
		}
		
		return names.ToArray();
	}



	public static void BuildIOSCommand(BuildSettings settings,bool development)
	{
		string[] scenes = GetBuildScenes();
		string path = GetBuildPath("iPhone");
		if (scenes == null || scenes.Length == 0 || path == null)
		{
			return;
		}
		
		Debugger.Log(string.Format("Path: \"{0}\"", path));
		for (int i = 0; i < scenes.Length; ++i)
		{
			Debugger.Log(string.Format("Scene[{0}]: \"{1}\"", i, scenes[i]));
		}
		ApplyIOSBuildSettings (settings, development);
		BuildPipeline.BuildPlayer(scenes, path, BuildTarget.iOS, BuildOptions.None);
	}

	private static void BuildIOSDevelopment(){
		BuildIOSCommand (BuildSettings.Instance, true);
	}
	private static void BuildIOSProduction(){
		BuildIOSCommand (BuildSettings.Instance, false);
		AntRun.RunAntTarget ("export-ipa");
	}

	[MenuItem("Alley Labs/Build/iOS Production")]
	public static void LocalIOSBuildProduction(){
		AntRun.RunAntTarget ("clean");
		BuildIOSCommand (BuildSettings.Instance, false);
		AntRun.RunAntTarget ("export-ipa-production");
	}

	[MenuItem("Alley Labs/Build/iOS Development")]
	public static void LocalIOSBuildDevelopment(){
		AntRun.RunAntTarget ("clean");
		BuildIOSCommand (BuildSettings.Instance, true);
		AntRun.RunAntTarget ("export-ipa-development");
	}
	private static void ApplyCommonSettingsBetweenIOSAndAndroid(){
		
	}
	private static void ApplyIOSBuildSettings(BuildSettings buildSettings, bool development = true, bool localBuild = false){
		var iOSVersion = development ? buildSettings.developmentVersion.iOSVersion : buildSettings.productionVersion.iOSVersion;
		string buildFlags = development ? buildSettings.developmentVersion.buildFlag.ToString () : buildSettings.productionVersion.buildFlag.ToString ();
		var iOSSettings = buildSettings.settings.iOSBuildSettings;
		PlayerSettings.iOS.targetDevice = iOSSettings.targetDevice;
		PlayerSettings.iOS.targetOSVersion = iOSSettings.targetiOSVersion;


		PlayerSettings.iOS.buildNumber = GetBundleCode ().ToString();
		PlayerSettings.bundleVersion = iOSVersion.version;

		PlayerSettings.bundleIdentifier = buildSettings.commonSettings.gameBundle;
		PlayerSettings.companyName = buildSettings.commonSettings.companyName;
		PlayerSettings.productName = buildSettings.commonSettings.productName;

		PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.iOS, buildFlags);

		//Default Settings
		PlayerSettings.iOS.targetResolution = iOSTargetResolution.Native;

	}

	private static void ApplyAndroidBuildSettings(BuildSettings buildSettings, bool development = true, bool localBuild = false){
		var androidVersion = development ? buildSettings.developmentVersion.androidVersion : buildSettings.productionVersion.androidVersion;
		var androidSettings = buildSettings.settings.androidBuilSettings;
		string buildFlags = development ? buildSettings.developmentVersion.buildFlag.ToString () : buildSettings.productionVersion.buildFlag.ToString ();

		PlayerSettings.Android.keystoreName = FindFullFilePath (androidSettings.keystore);
		PlayerSettings.Android.keystorePass = androidSettings.keystorePass;
		PlayerSettings.Android.keyaliasName = androidSettings.keyaliasName;
		PlayerSettings.Android.keyaliasPass = androidSettings.keyaliasPass;
		PlayerSettings.bundleVersion = androidVersion.version;

		PlayerSettings.bundleIdentifier = buildSettings.commonSettings.gameBundle;
		PlayerSettings.companyName = buildSettings.commonSettings.companyName;
		PlayerSettings.productName = buildSettings.commonSettings.productName;

		PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.Android, buildFlags);

		PlayerSettings.Android.bundleVersionCode = GetBundleCode ();

		if (!localBuild) {
			EditorPrefs.SetString("AndroidSdkRoot", buildSettings.commonSettings.androidSDK);
		}
	
	}
	public static string FindFullFilePath(string fileName){
		List<string> provisionFiles = Directory.GetFiles (Application.dataPath, fileName, SearchOption.AllDirectories).ToList ();
		return provisionFiles[0];
	}

	private static int GetBundleCode(){
		string build_number_string = Environment.GetEnvironmentVariable("BUILD_NUMBER");
		if (build_number_string != null) {
			try {
				int buildNumber = 1;
				Int32.TryParse(build_number_string,out buildNumber);
				return buildNumber;
			} catch (Exception ex) {
				return 1;
			}
		}
		return 1;
	}
	private static string GetBuildPath(string target)
	{
		string dirPath = Application.dataPath + "/../Build/" + target;
		if (!System.IO.Directory.Exists (dirPath)) {
			var dir = new DirectoryInfo(Application.dataPath + "/../Build/");
			dir.Delete(true);

			System.IO.Directory.CreateDirectory (dirPath);
		} else {
			var dir = new DirectoryInfo(Application.dataPath + "/../Build/");
			Debug.Log(dir.ToString());
			dir.Delete(true);
			System.IO.Directory.CreateDirectory (dirPath);
		}
		return dirPath;
	}
	private static void BuildAndroid(BuildSettings settings, bool development, bool localBuild)
	{
		Debugger.Log("Command line build android version\n------------------\n------------------");

		string[] scenes = GetBuildScenes();

		string path = GetBuildPath("android");

		if (scenes == null || scenes.Length == 0 || path == null)
		{
			Debugger.Log("No scene avaiable");
			return;
		}
		ApplyAndroidBuildSettings (settings,development,localBuild);
		BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, BuildOptions.None);	
	}
	public static void BuildAndroidDevelopmentRemote(){
		BuildAndroid (BuildSettings.Instance, true, false);
	}
	private static void BuildAndroidProductionRemote(){
		BuildAndroid (BuildSettings.Instance, false, false);
	}

	[MenuItem("Alley Labs/Build/Android Development")]
	public static void LocalAndroidBuildDevelopment(){
		BuildAndroid (BuildSettings.Instance, false, true);
		AntRun.RunAntTarget ("rename-android-build");
	}

	[MenuItem("Alley Labs/Build/Android Production")]
	public static void LocalAndroidBuildProduction(){
		BuildAndroid (BuildSettings.Instance, true, true);
		AntRun.RunAntTarget ("rename-android-build");
	}
	
}
