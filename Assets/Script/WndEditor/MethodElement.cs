using System;
using UnityEngine;
using System.Collections;

namespace WndEditor
{
    [Serializable]
    public class MethodElement
    {
        public enum methodType
        {
            Defaut,
            String,
            Int,
            Bool,
            List,
            Dictionary
        }

        public methodType _methodType = methodType.Defaut;
        public string methodKey = "";
		public GameObject target;
		[EnumPopup(typeof(ParamFuncEnum),true)]
		public ParamFuncEnum paramFunc;

		public GameObject cbTarget;
		public string wndName;

		public bool isParam = false;
    }

    public enum ParamFuncEnum
    {
        Defaut,
        SetLabel,
        SetTexture,
        SetCount,
        SetListItem,
		SetActive,
		SetButton,
		SetData
    }

	public enum CallbackFuncEnum
	{
		SetActive,
		ChangeWnd,
		PopupWnd
	}
}
