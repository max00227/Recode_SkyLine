using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackendAsynchronousSender : Singleton<BackendAsynchronousSender> {

	public void Execute(string request ="", string serviceApi = "", bool isPost=false){
		if (request == "" || serviceApi == "") {
			return;
		}
		string _url = "";

		byte[] postData = null;
		if (isPost) {
			_url = serviceApi;
			postData = System.Text.Encoding.UTF8.GetBytes (request);
		} 
		/*else {
			_url = serviceApi+"?"+
		}*/

		var header = new Dictionary<string, string>();
		header.Add("Content-Type", "application/json; charset=utf-8");
		WWW www = new WWW (serviceApi, postData, header);
	}
}
