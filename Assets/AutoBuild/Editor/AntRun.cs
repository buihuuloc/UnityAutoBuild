using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;
using System.IO;
public class AntRun{

	public static void  RunAntTarget(string target){
		string buildXmlPath = BuildCommand.FindFullFilePath ("build.xml");
		Debugger.Log (GetWorkingDirectory());
		Process p = new Process();
		p.StartInfo.RedirectStandardError=true;
		p.StartInfo.RedirectStandardInput = true;
		p.StartInfo.RedirectStandardOutput=true;
		p.StartInfo.CreateNoWindow = true;
		p.StartInfo.UseShellExecute = false;
		//p.StartInfo.WorkingDirectory = Application.dataPath + "/AutoBuild/Settings";
		p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
		p.StartInfo.FileName = "sh";
		p.StartInfo.Arguments = " " + Application.dataPath + "/AutoBuild/Settings/build.sh" + " " + buildXmlPath + " " + target;
		p.Start ();
		UnityEngine.Debug.Log(p.StandardOutput.ReadToEnd());
		p.WaitForExit();
		p.Close ();
	}
	public static string GetWorkingDirectory(){
		string workingPath = BuildCommand.FindFullFilePath ("build.xml");
		return workingPath.Remove(workingPath.Length-11,11);
	}

}
