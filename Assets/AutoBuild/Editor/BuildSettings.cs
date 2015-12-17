using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System;
using System.Reflection;
using System.Linq;

namespace AlleyLabs.AutoBuild{

	public enum BuildSettingsFlags{
		PRODUCTION,
		DEVELOPMENT
	}

	[System.Serializable]
	public class AntBuildProperties{
		public string fileName = "AlleyLabs";
		public string unityPath = "/Applications/Unity/Unity.app/Contents/MacOS/Unity";
		public string xcodeBuild = "/usr/bin/xcodebuild";
		public string xcrunPath = "/usr/bin/xcrun";
		public string IOSDistributionProvisioningFile = "";
		public string IOSAdHocPorvisioningFile = "";
		public string ituneConnectID = "dungnt@sixthgearstudios.com";
		public string schemeName = "Unity-iPhone";
		public string buildDir = "../../../Build";
		public string devDir = "../../..";
		public string appleSigningIdentity = "'iPhone Distribution: Alley Labs (9Q2287C3QD)'";

		public override string ToString ()
		{
			return string.Format ("[AndroidBuildSettings]");
		}

		public void SaveToFile(){
			string resultData = "";
			var bindingFlags = BindingFlags.Instance |
							BindingFlags.NonPublic |
							BindingFlags.Public;
			var fieldValues = this.GetType()
				.GetFields(bindingFlags)
					.Select(field => field.GetValue(this))
					.ToList();
			var fieldNames = typeof(AntBuildProperties).GetFields ().Select (field => field.Name).ToList ();
			foreach (var name in fieldNames) {	
				resultData += name.ToString() + "=" + fieldValues[fieldNames.IndexOf(name)].ToString() + '\n';
			}
			string PropertiesPath = Application.dataPath + "/AutoBuild/Settings/Dev_En.properties";
			File.WriteAllText(PropertiesPath, resultData);

			Debug.Log (resultData);
		}
		
		
		
	}
	[System.Serializable]
	public class AndroidBuildSettings{
		public string keystore = "";
		public string keystorePass = "";
		public string keyaliasName = "";
		public string keyaliasPass = "";
		public AndroidSdkVersions androidAPILevel = AndroidSdkVersions.AndroidApiLevel10;

	}
	
	[System.Serializable]
	public class VersionIdentifier{
		//public int bundleCode = 1;
		public string version = "1.0";
	}
	
	[System.Serializable]
	public class IOSBuildSettings{
		public iOSTargetOSVersion targetiOSVersion = iOSTargetOSVersion.iOS_7_0;
		public iOSTargetDevice targetDevice = iOSTargetDevice.iPhoneAndiPad;
	}

	[System.Serializable]
	public class Settings{
		public AndroidBuildSettings androidBuilSettings;
		public IOSBuildSettings iOSBuildSettings;
		
		[HideInInspector]
		public bool showAndroidSettings = false;
		
		[HideInInspector]
		public bool showIOSSettings = false;

	}
	[System.Serializable]
	public class VersionSettings{
		public BuildSettingsFlags buildFlag;
		public VersionIdentifier iOSVersion;
		public VersionIdentifier androidVersion;
	}
	[System.Serializable]
	public class DevelopmentVersion : VersionSettings{

	}
	
	[System.Serializable]
	public class ProductionVersion : VersionSettings{
	}
		
	[System.Serializable]
	public class CommonSettings{
		public string gameBundle = "com.alleylab.productname";
		public string companyName = "Alley Labs";
		public string productName = "My Game";
		public UIOrientation orientation = UIOrientation.AutoRotation;
		public string androidSDK = "";
	}
	public class BuildSettings : ScriptableObject {
		
		
		const string buildSettingsAssetName = "BuildSettings";
		const string buildSettingsPath = "AutoBuild/Resources";
		const string buildSettingsAssetExtension = ".asset";
		
		private static BuildSettings _instance;
		
		public AntBuildProperties antBuildProperties;

		public Settings settings;
		public DevelopmentVersion developmentVersion;
		public ProductionVersion productionVersion;


		public int distributionIndex = 0;
		public int adhocIndex = 0;
		public CommonSettings commonSettings;
		
		public static BuildSettings Instance{
			get{
				_instance = Resources.Load(buildSettingsAssetName) as BuildSettings;
				if (_instance == null) {
					// If not found, autocreate the asset object.
					_instance = CreateInstance<BuildSettings>();
					#if UNITY_EDITOR
					string properPath = Path.Combine(Application.dataPath, buildSettingsPath);
					if (!Directory.Exists(properPath))
					{
						AssetDatabase.CreateFolder("Assets/AutoBuild", "Resources");
					}
					
					string fullPath = Path.Combine(Path.Combine("Assets", buildSettingsPath),
					                               buildSettingsAssetName + buildSettingsAssetExtension
					                               );
					AssetDatabase.CreateAsset(_instance, fullPath);
					#endif
				}
				return _instance;
			}
		}	
	}
}

