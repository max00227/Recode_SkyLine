using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using model.data;

public class DataUtil{
	public static T GetById<T>(int id,ref List<T> list)
		where T: IdentifiableEntity
	{
		if (list != null) {
			foreach (T item in list) {
				if (item.id == id) {
					
					return item;
				}
			}
		}

		return default(T);
	}
}
