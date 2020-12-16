using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager
{
	public static void SaveFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
		PlayerPrefs.Save();
	}

	public static float LoadFloat(string key, float defaultValue)
	{
		if (PlayerPrefs.HasKey(key))
			return PlayerPrefs.GetFloat(key);
		else
			return defaultValue;
	}

	public static void SaveBool(string key, bool value)
	{
		PlayerPrefs.SetInt(key, value ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static bool LoadBool(string key, bool defaultValue)
	{
		if (PlayerPrefs.HasKey(key))
		{
			int value = PlayerPrefs.GetInt(key);
			return value != 0 ? true : false;
		}
		else
			return defaultValue;
	}
}
