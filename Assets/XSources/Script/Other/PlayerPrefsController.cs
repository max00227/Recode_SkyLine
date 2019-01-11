using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsController{

	public static void SetInt(string key, int value){
		PlayerPrefs.SetInt (key, value);
	}

	public static int GetInt(string key, int defaultValue){
		int value = PlayerPrefs.GetInt (key, defaultValue);

		return value;
	}

	public static void SetFloat(string key, float value){
		PlayerPrefs.SetFloat (key, value);
	}

	public static float GetFloat(string key, float defaultValue){
		float value = PlayerPrefs.GetFloat (key, defaultValue);

		return value;
	}

	public static void SetString(string key, string value){
		PlayerPrefs.SetString (key, value);
	}

	public static string GetString(string key, string defaultValue){
		string value = PlayerPrefs.GetString (key, defaultValue);

		return value;
	}

	public static void DeleteKey(string key){
		PlayerPrefs.DeleteKey (key);
	}

	public static bool HasKey(string key){
		bool value = PlayerPrefs.HasKey (key);

		return value;
	}

	public static void DeleteAll(){
		PlayerPrefs.DeleteAll ();
	}

	public static void Save() {
		PlayerPrefs.Save();
	}
}
