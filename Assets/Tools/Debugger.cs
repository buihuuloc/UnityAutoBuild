using System;
using UnityEngine;

/// <summary>
/// To turn off log
/// remove Debug.Log
/// </summary>
public static class Debugger
{
	private static Action<object> LogTarget = new Action<object>(Debug.Log);
	
	private static Action<object> LogWarningTarget = new Action<object>(Debug.LogWarning);
	
	private static Action<object> LogErrorTarget = new Action<object>(Debug.LogError);
	
	public static Action<object> Log = delegate(object message)
	{
		Debug.Log(message);
	};
	
	public static Action<object> LogWarning = delegate(object message)
	{
		Debug.LogWarning(message);
	};
	
	public static Action<object> LogError = delegate(object message)
	{
		Debug.LogError(message);
	};
	
	public static Action<object> LogAlways = Debugger.LogTarget;
	
	public static Action<object> LogWarningAlways = Debugger.LogWarningTarget;
	
	public static Action<object> LogErrorAlways = Debugger.LogErrorTarget;
}