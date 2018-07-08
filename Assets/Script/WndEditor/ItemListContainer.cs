using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace WndEditor
{
	public class ItemListContainer : MonoBehaviour {

		[SerializeField]
		GameObject listItem;

		[SerializeField]
		MethodElement[] requestElementList;

		List<object> itemlist;

		[SerializeField]
		int pageMaxCount;

		public delegate void listItemCallback(string cb, CallbackFuncEnum func);

		public listItemCallback ContainerCb;

		[EnumPopup(typeof(CallbackFuncEnum),true)]
		public CallbackFuncEnum callbackFunc;

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void SetListItem(object param){
			itemlist = (List<object>)param;
			if (itemlist.Count > 0) {
				for (int i = 0; i < itemlist.Count; i++) {
					GameObject item = Instantiate (listItem);
					item.transform.parent = this.transform;
					item.GetComponent<ParameterReciever> ().ResolveReq (itemlist [i].ToString ());
				}
			}
		}

		public void SetListItemTest(int listCount){
			GridLayoutGroup grid = GetComponent<GridLayoutGroup> ();
			grid.cellSize = new Vector2 (listItem.GetComponent<RectTransform> ().rect.width, listItem.GetComponent<RectTransform> ().rect.height);
			if (listCount > 0) {
				for (int i = 0; i < listCount; i++) {
					GameObject item = Instantiate (listItem);
					item.name = "A" + i.ToString ();
					item.GetComponent<RectTransform> ().SetParent (this.transform);
					item.GetComponent<RectTransform> ().localScale = Vector3.one;

				}
			}
		}

		private void itemCallback(string cb){
			ContainerCb.Invoke (cb, callbackFunc);
		}
	}
}
