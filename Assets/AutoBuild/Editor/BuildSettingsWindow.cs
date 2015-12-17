using UnityEngine;
using System.Collections;
using UnityEditor;
using AlleyLabs.AutoBuild;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;


[CustomEditor(typeof(BuildSettings))]
public class BuildSettingsWindow : EditorWindow {
	private static BuildSettingsWindow _buildWindow;

	static AndroidBuildSettings _androidBuildSettings;
	static IOSBuildSettings _iOSBuildSettings;
	static BuildSettings _buildSettingsInstance;
	public bool showPosition = true;
	public string status = "Select a GameObject";

	private int toolbarButtonIndex = 0;

	public Texture2D logo;

	private bool showCommonSettings = true;
	private bool showAntBuildProperties = true;
	private Vector2 scrollPosition = Vector2.zero;
	public string[] iOSPlatform = new string[]{
		"iPhone Only",
		"iPad Only",
		"iPhone + iPad"
	};

	public string[] androidAPILevel = new string[]{
		"Android 2.3.1, Gingerbread, API level 9",
		"Android 2.3.3, Gingerbread, API level 10",
		"Android 3.0, Honeycomb, API level 11",
		"Android 3.1, Honeycomb, API level 12",
		"Android 3.2, Honeycomb, API level 13",
		"Android 4.0, Ice Cream Sandwich, API level 14",
		"Android 4.0.3, Ice Cream Sandwich, API level 15",
		"Android 4.1, Jelly Bean, API level 16",
		"Android 4.2, Jelly Bean, API level 17",
		"Android 4.3, Jelly Bean, API level 18",
		"Android 4.4, KitKat, API level 19",
		"Android 5.0, Lollipop, API level 21",
		"Android 5.1, Lollipop, API level 22"
	};
	static string[] provisioningFiles;
	static string[] keystoreFiles;
	void OnEnable(){
		logo = AssetDatabase.LoadAssetAtPath("Assets/AutoBuild/Resources/alleylabsIcon.png", typeof(Texture2D)) as Texture2D;

		_buildSettingsInstance = BuildSettings.Instance;
		//provisioningFiles = FileAllProvisiongFile ();
		provisioningFiles = FindAllProvisiongFile ();
		keystoreFiles = FindKeyStoreFile ();
		Debugger.Log (BuildCommand.FindFullFilePath (keystoreFiles [0]));
	}

	[MenuItem("Alley Labs/Build Server Config")]
	static void Init(){
		provisioningFiles = FindAllProvisiongFile ();
		keystoreFiles = FindKeyStoreFile ();

		_buildWindow = GetWindow(typeof (BuildSettingsWindow), false, "Build Server", true) as BuildSettingsWindow;
		
		_buildWindow.minSize = new Vector2(300, 200);

		EditorWindow.GetWindow (typeof(BuildSettingsWindow)).Show();
		_buildSettingsInstance = BuildSettings.Instance;


	}

	void OnGUI(){
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false); 
		ShowAllSettings ();
		DevelopmentBuild ();
		GUILayout.EndScrollView();
	}

	void ShowAllSettings(){
		GUILayout.Label(logo,GUILayout.MaxHeight(40), GUILayout.MaxWidth(200));
		GUILayout.Space (20);
		
		GUIStyle labelStyle = new GUIStyle ("BoldLabel");
		
		EditorGUILayout.LabelField ("Common Settings",labelStyle);
		
		EditorGUILayout.HelpBox ("share between ios and android", MessageType.Info);
		
		EditorGUILayout.BeginVertical();
		_buildSettingsInstance.commonSettings.productName = EditorGUILayout.TextField ("Game Name", _buildSettingsInstance.commonSettings.productName);
		
		_buildSettingsInstance.commonSettings.gameBundle = EditorGUILayout.TextField ("Bundle Identifier", _buildSettingsInstance.commonSettings.gameBundle);
		
		PlayerSettings.defaultInterfaceOrientation = (UIOrientation)EditorGUILayout.EnumPopup ("Default Orientation", PlayerSettings.defaultInterfaceOrientation);
		
		if (PlayerSettings.defaultInterfaceOrientation == UIOrientation.AutoRotation) {
			GUILayout.Space(5);
			EditorGUILayout.LabelField ("Allowed Orientations for Auto Rotation",labelStyle);
			EditorGUI.indentLevel++;
			PlayerSettings.allowedAutorotateToLandscapeLeft = EditorGUILayout.Toggle("Landscape Left",PlayerSettings.allowedAutorotateToLandscapeLeft);
			PlayerSettings.allowedAutorotateToLandscapeRight = EditorGUILayout.Toggle("Landscape Left",PlayerSettings.allowedAutorotateToLandscapeRight);
			PlayerSettings.allowedAutorotateToPortrait = EditorGUILayout.Toggle("Portrait",PlayerSettings.allowedAutorotateToPortrait);
			PlayerSettings.allowedAutorotateToPortraitUpsideDown = EditorGUILayout.Toggle("Portrait Upside Down",PlayerSettings.allowedAutorotateToPortraitUpsideDown);
			EditorGUI.indentLevel--;
			GUILayout.Space(5);
		}
		_buildSettingsInstance.commonSettings.androidSDK = EditorGUILayout.TextField ("Android SDK Path", _buildSettingsInstance.commonSettings.androidSDK);

		EditorGUILayout.EndVertical();

		ShowAntBuildProperties ();
		ShowAndroidBuildCommon ();
		ShowIOSBuildCommon ();
		ShowVersionSettings ();

		EditorGUILayout.Space ();

		if (GUI.changed) {
			EditorUtility.SetDirty(_buildSettingsInstance);
			_buildSettingsInstance.antBuildProperties.SaveToFile();
		}
	}

	void ShowVersionSettings(){
		GUILayout.Space (10);

		EditorGUILayout.BeginHorizontal ();

		/*
		 * Development
		 */

		ShowVersionsSettingsConfig (_buildSettingsInstance.developmentVersion, "Development");

		/*
		 * Production
		 */
		ShowVersionsSettingsConfig (_buildSettingsInstance.productionVersion, "Production");

		EditorGUILayout.EndHorizontal ();
	}

	void ShowVersionsSettingsConfig(VersionSettings versionSettings, string lableHader){
		EditorGUILayout.BeginVertical (GUI.skin.box);
		
		EditorGUILayout.LabelField (lableHader,new GUIStyle ("BoldLabel"));
		
		EditorGUILayout.BeginVertical();
		EditorGUI.indentLevel++;
		versionSettings.buildFlag = (BuildSettingsFlags)EditorGUILayout.EnumPopup ("Build Flag", versionSettings.buildFlag);
		EditorGUILayout.LabelField ("Android Version",new GUIStyle ("BoldLabel"));
		
		EditorGUILayout.BeginVertical(GUI.skin.box);
		versionSettings.androidVersion.version = EditorGUILayout.TextField ("Version", versionSettings.androidVersion.version);
		//versionSettings.androidVersion.bundleCode = EditorGUILayout.IntField ("Bundle Code", versionSettings.androidVersion.bundleCode);
		
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.EndVertical ();
		EditorGUI.indentLevel--;
		
		
		EditorGUILayout.BeginVertical();
		EditorGUI.indentLevel++;
		EditorGUILayout.LabelField ("IOS Version",new GUIStyle ("BoldLabel"));
		
		EditorGUILayout.BeginVertical(GUI.skin.box);
		versionSettings.iOSVersion.version = EditorGUILayout.TextField ("Version",versionSettings.iOSVersion.version);
		//versionSettings.iOSVersion.bundleCode = EditorGUILayout.IntField ("Bundle Code", versionSettings.iOSVersion.bundleCode);
		
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.EndVertical ();
		
		
		EditorGUILayout.EndVertical ();
		EditorGUI.indentLevel--;
	}
	void DevelopmentBuild(){
		GUILayout.Space (10);
		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button("/B iOS Dev",GUILayout.MinHeight(50))) {
			BuildCommand.LocalIOSBuildDevelopment();
		}
		GUILayout.Space (20);
		if (GUILayout.Button("/B Android Prod",GUILayout.MinHeight(50))) {
			BuildCommand.LocalAndroidBuildDevelopment();
		}
		EditorGUILayout.EndHorizontal ();
	}
	void ProductionBuild(){
	}

	void ShowIOSBuildCommon(){
		GUILayout.Space (10);
		GUIStyle labelStyle = new GUIStyle ("BoldLabel");
		EditorGUILayout.LabelField ("iOS Build Common",labelStyle);

		EditorGUILayout.BeginVertical(GUI.skin.box);
		PlayerSettings.iOS.targetOSVersion = (iOSTargetOSVersion)EditorGUILayout.EnumPopup("Target version",PlayerSettings.iOS.targetOSVersion);
		PlayerSettings.iOS.targetDevice = (iOSTargetDevice)EditorGUILayout.Popup("Target device",PlayerSettings.iOS.targetDevice.GetHashCode(),iOSPlatform);
		EditorGUILayout.EndVertical();
	}
	void ShowAndroidBuildCommon(){
		var settings = _buildSettingsInstance.settings;
		GUILayout.Space (10);
		GUIStyle labelStyle = new GUIStyle ("BoldLabel");
		EditorGUILayout.LabelField ("Android Build Common",labelStyle);

		if (keystoreFiles != null && keystoreFiles.Length >= 1) {
			EditorGUILayout.Popup ("Keystore ", 0, keystoreFiles);
			settings.androidBuilSettings.keystore = "./" + keystoreFiles[0];
			_buildSettingsInstance.antBuildProperties.IOSAdHocPorvisioningFile = "./" + provisioningFiles [adHocFileIndex];
			EditorGUILayout.BeginVertical(GUI.skin.box);
			settings.androidBuilSettings.keystorePass = EditorGUILayout.TextField("KeyStore Pass", settings.androidBuilSettings.keystorePass);
			settings.androidBuilSettings.keyaliasName = EditorGUILayout.TextField("Alias Name", settings.androidBuilSettings.keyaliasName);
			settings.androidBuilSettings.keyaliasPass = EditorGUILayout.TextField("Alias Pass", settings.androidBuilSettings.keyaliasPass);
			EditorGUILayout.EndVertical();
		}
		PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)EditorGUILayout.Popup("Minimum API Level", PlayerSettings.Android.minSdkVersion.GetHashCode(),androidAPILevel);
	}

	void ShowSettings(Settings settings, bool development){
		GUILayout.Space (10);
		ShowAndroidSettings (settings);
		ShowIOSSettings (settings);
	}
	void ShowIOSSettings(Settings settings){
		settings.showIOSSettings = AlleyLabsEditorTools.BeginFoldOut ("IOS Settings", settings.showIOSSettings);
		if (settings.showIOSSettings) {
			EditorGUILayout.BeginVertical(GUI.skin.box);
//			settings.iOSBuildSettings.versionIdentifier.version = EditorGUILayout.TextField("Version", settings.iOSBuildSettings.versionIdentifier.version);
//			settings.iOSBuildSettings.versionIdentifier.bundleCode = EditorGUILayout.IntField("Bundle Code", settings.iOSBuildSettings.versionIdentifier.bundleCode);
			PlayerSettings.iOS.targetOSVersion = (iOSTargetOSVersion)EditorGUILayout.EnumPopup("Target version",PlayerSettings.iOS.targetOSVersion);
			PlayerSettings.iOS.targetDevice = (iOSTargetDevice)EditorGUILayout.Popup("Target device",PlayerSettings.iOS.targetDevice.GetHashCode(),iOSPlatform);
			EditorGUILayout.EndVertical();
		}
	}

	void ShowAntBuildProperties(){
		GUILayout.Space (10);

		showAntBuildProperties = AlleyLabsEditorTools.BeginFoldOut ("Ant Build Properties", showAntBuildProperties);
		if (showAntBuildProperties) {
			EditorGUILayout.BeginVertical(GUI.skin.box);
			_buildSettingsInstance.antBuildProperties.fileName = EditorGUILayout.TextField ("File Name", _buildSettingsInstance.antBuildProperties.fileName);

			_buildSettingsInstance.antBuildProperties.unityPath = EditorGUILayout.TextField ("Unity Path", _buildSettingsInstance.antBuildProperties.unityPath);
			//_buildSettingsInstance.antBuildProperties.xcrunPath = EditorGUILayout.TextField ("Xcrun Path", _buildSettingsInstance.antBuildProperties.xcrunPath);
			//_buildSettingsInstance.antBuildProperties.xcodeBuild = EditorGUILayout.TextField ("Xcode Build", _buildSettingsInstance.antBuildProperties.xcodeBuild);
			_buildSettingsInstance.antBuildProperties.appleSigningIdentity = EditorGUILayout.TextField ("Signing Identity", _buildSettingsInstance.antBuildProperties.appleSigningIdentity);
			EditorGUILayout.HelpBox ("Must include ' '", MessageType.Info);
			

			
			_buildSettingsInstance.distributionIndex = EditorGUILayout.Popup ("Distribution Provisioning", _buildSettingsInstance.distributionIndex, provisioningFiles);
			_buildSettingsInstance.antBuildProperties.IOSDistributionProvisioningFile = "./" + provisioningFiles [_buildSettingsInstance.distributionIndex];
			
			_buildSettingsInstance.adhocIndex = EditorGUILayout.Popup ("AdHoc Provisioning", _buildSettingsInstance.adhocIndex, provisioningFiles);
			_buildSettingsInstance.antBuildProperties.IOSAdHocPorvisioningFile = "./" + provisioningFiles [_buildSettingsInstance.adhocIndex];
			
			EditorGUILayout.EndVertical ();
		}

	}
	private int adHocFileIndex = 0;
	private int distributionFileIndex = 0;

	#region Find all file with extension

	/// <summary>
	/// Finds the file with extension.
	/// </summary>
	/// <returns>The file with extension.</returns>
	/// <param name="extension">Extension.</param>
	private static string[] FindFileWithExtension(string extension){

		List<string> provisionFiles = Directory.GetFiles (Application.dataPath, "*." + extension, SearchOption.AllDirectories).ToList ();
		string[] realProvisionPath = new string[provisionFiles.Count];
		foreach (var file in provisionFiles) {
			string path = Path.GetFileName(file);
			realProvisionPath[provisionFiles.IndexOf(file)] = path;
		}
		return realProvisionPath;
	}

	/// <summary>
	/// Finds all provisiong file.
	/// </summary>
	/// <returns>The all provisiong file.</returns>
	private static string[] FindAllProvisiongFile(){
		return FindFileWithExtension ("mobileprovision");
	}
	/// <summary>
	/// Finds the key store file.
	/// </summary>
	/// <returns>The key store file.</returns>
	private static string[] FindKeyStoreFile(){
		return FindFileWithExtension ("keystore");
	}

	#endregion

	void ShowAndroidSettings(Settings settings){

	}
	void ShowProductionSettings(){	
	}

}
