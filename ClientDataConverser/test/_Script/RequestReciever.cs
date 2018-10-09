using System;
using UnityEngine;
using System.Collections.Generic;
//using UnityEngine.UI;

namespace test
{
    public class RequestReciever : MonoBehaviour
    {
        [SerializeField]
        string api;

        [SerializeField]
        bool isWnd = false;

        [SerializeField]
        bool isPost = false;


        [SerializeField]
        MethodElement[] requestElementList;



        public void RecieveReq(string param = "")
        {
            if (isWnd)
            {
                if (isPost)
                {

                }
                else
                {

                }
            }
            else
            {
                if (param == null)
                {
                    return;
                }
                else
                {
                    if (requestElementList.Length > 0)
                    {
                        //SendParams(JsonConvertExtension.readJson(param, requestElementList));
                    }
                }
            }
        }

        void SendParams(List<object> _params)
        {
            for (int i = 0; i < _params.Count; i++)
            {
                RequestReciever reciever = requestElementList[i].sendGameObject.GetComponent<RequestReciever>();
                if (reciever == null)
                {
                    continue;
                }
                if (requestElementList[i].sendGameObject == this.gameObject || requestElementList[i].sendGameObject == null)
                {
                    setParam(_params[i], requestElementList[i]);
                }
                reciever.RecieveReq(_params[i].ToString());
            }
        }

        void setParam(object param, MethodElement element)
        {
            switch (element.paramFunc)
            {
                /*case ParamFuncEnum.Defaut:
                    break;

                case ParamFuncEnum.SetLabel:
                    gameObject.GetComponent<Text>().text = param.ToString();

                case ParamFuncEnum.SetCount:
                    break;

                case ParamFuncEnum.SetTexture:
                    gameObject.GetComponent<Image>().overrideSprite = Resources.Load(param.ToString(), typeof(Sprite)) as Sprite; ;
                    break;

                case ParamFuncEnum.SetListItem:
                    break;
*/
            }

        }
    }
}