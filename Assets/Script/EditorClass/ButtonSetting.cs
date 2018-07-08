using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WndEditor
{
	[RequireComponent(typeof(ParameterReciever))]
	[RequireComponent(typeof(Button))]
	public class ButtonSetting : MonoBehaviour {
		[SerializeField]
		ButtonFuncEnum buttonFunc;

		[SerializeField]
		string wndName;

		[SerializeField]
		MethodElement[] elements;

		[SerializeField]
		ParameterReciever reciever;

		[HideInInspector]
		public bool isEnable = true;

		public void SetButton(object param){
			reciever.ResolveReq (param);
		}

		public void OnClickButton() {
			if (isEnable) {
				switch (buttonFunc) {
				case ButtonFuncEnum.defaut:
					break;
				case ButtonFuncEnum.TranWnd:
					if (!string.IsNullOrEmpty (wndName)) {
						WindowManager.Instance.TranWnd (wndName, reciever.savedParams);
					}
					break;
				case ButtonFuncEnum.PopupWnd:
					if (!string.IsNullOrEmpty (wndName)) {
						WindowManager.Instance.PopupWnd (wndName, reciever.savedParams);
					}
					break;
				case ButtonFuncEnum.SetTure:
					break;
				case ButtonFuncEnum.SetFalse:
					break;
				case ButtonFuncEnum.Purchase:
					WindowManager.Instance.TranWnd (wndName);
					break;
				}	
			}
		}
	}


	public enum ButtonFuncEnum
	{
		defaut,
		TranWnd,
		PopupWnd,
		SetTure,
		SetFalse,
		Purchase
	}
}
