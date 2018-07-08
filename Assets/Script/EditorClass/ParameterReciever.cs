using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace WndEditor
{
	public class ParameterReciever : MonoBehaviour
	{
		[SerializeField]
		bool isCombine= false;

		[SerializeField]
		MethodElement[] requestElementList;

		[HideInInspector]
		public string savedParams;

		public void ResolveReq(object param){
			if (param == null) {
				return;
			}

			if (requestElementList.Length > 0) {
				SetParams (JsonConversionExtensions.readJson (param.ToString ()));
			}
		}

		public void SetParams(Dictionary<string, object> _params){
			SaveParams (_params);
			if (isCombine) {
				SetElement ((object)_params, requestElementList[0]);
			}
			else {
				foreach (MethodElement element in requestElementList) {
					if (element.target == null || string.IsNullOrEmpty(_params [element.methodKey].ToString())) {
						Debug.Log (_params [element.methodKey]);
						continue;
					}
					ParameterReciever reciever = element.target.GetComponent<ParameterReciever> ();
					if (element.paramFunc != ParamFuncEnum.Defaut) {
						SetElement (JsonConversionExtensions.ConvertType (_params [element.methodKey], element._methodType), element);
					} else {
						if (reciever != null) {
							reciever.ResolveReq (_params [element.methodKey].ToString ());
						}
					}
				}
			}
		}

		void SaveParams(Dictionary<string, object> _params){
			Dictionary<string, object> _savedParams = new Dictionary<string, object> ();
			foreach (MethodElement element in requestElementList) {
				if (element.isParam == true) {
					_savedParams.Add (element.methodKey, _params [element.methodKey]);
				}
			}
			savedParams = JsonConversionExtensions.ConvertJson (_savedParams); 
		}

		void SetElement(object param , MethodElement element){
			if (element.target == null) {
				Debug.Log (param);
				return;
			}

			switch (element.paramFunc) {
			case ParamFuncEnum.SetLabel:
				//Debug.Log (param.ToString ());
				element.target.GetComponent<Text> ().text = param.ToString ();
				break;

			case ParamFuncEnum.SetCount:
				break;

			case ParamFuncEnum.SetTexture:
				element.target.GetComponent<Image> ().overrideSprite = Resources.Load (param.ToString (), typeof(Sprite)) as Sprite;
				;
				break;
			
			case ParamFuncEnum.SetListItem:
				element.target.GetComponent<ItemListContainer> ().SetListItem ((List<object>)param);
				element.target.GetComponent<ItemListContainer> ().ContainerCb += ElementCallBack;
				break;

			case ParamFuncEnum.SetActive:
				int parseValue = 0;
				if (Int32.TryParse (param.ToString(), out parseValue)) {
					element.target.gameObject.SetActive (parseValue == 1);
				} else {
					element.target.gameObject.SetActive (false);
				}
				break;

			case ParamFuncEnum.SetButton:
				element.target.GetComponent<ButtonSetting> ().SetButton(param);
				break;

			case ParamFuncEnum.SetData:
				gameObject.GetComponent<DataReciever> ().GetData (param);
				break;
			}


		}

		private void ElementCallBack(string cb,CallbackFuncEnum func){
			switch (func) {
			case CallbackFuncEnum.SetActive:
				break;

			case CallbackFuncEnum.ChangeWnd:
				break;

			case CallbackFuncEnum.PopupWnd:
				break;
			}

		}
	}
}