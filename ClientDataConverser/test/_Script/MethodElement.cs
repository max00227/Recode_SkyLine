using System;
using UnityEngine;
using System.Collections;

namespace test
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
        public GameObject sendGameObject = null;

        public ParamFuncEnum paramFunc;
    }

    public enum ParamFuncEnum
    {
        Defaut,
        SetLabel,
        SetTexture,
        SetCount,
        SetListItem
    }

   

}
