using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class ABBuilder : MonoBehaviour {
	[MenuItem("MyProject/Builder")]
	static public void AssetBundleBuilder(){
		string outPutPath = Application.dataPath.Replace("Assets", "ABPath");
		Debug.Log (outPutPath);
		if (!Directory.Exists (outPutPath)) {
			Directory.CreateDirectory (outPutPath);
		}
		BuildPipeline.BuildAssetBundles(outPutPath, 
			BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, 
			EditorUserBuildSettings.activeBuildTarget);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
