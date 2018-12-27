using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using model.data;
using System.Linq;

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

	public static float LimitFloat(float input, float limit, bool isReturnMin){
		if (!isReturnMin) {
			if (input > limit) {
				return limit;
			}
		} 
		else {
			if (input < limit) {
				return limit;
			}
		}
		return input;
	}

	/// <summary>
	/// Randoms the list.
	/// </summary>
	/// <returns>The list.</returns>
	/// <param name="randomCount">抽選數量</param>
	/// <param name="array">抽選清單</param>
	/// <param name="lastCount">真實數量</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static List<T> RandomList<T>(int randomCount, T[] array, int? lastCount= null){
		List<T> ListT = new List<T> ();
		int count = lastCount==null?array.Length:(int)lastCount;
		if (count > randomCount) {
			for (int i = 0; i < randomCount; i++) {
				int idx = UnityEngine.Random.Range (0, array.Length);
				while (ListT.Contains (array [idx])) {
					idx = UnityEngine.Random.Range (0, array.Length);
				}

				ListT.Add (array [idx]);
			}
		} else if (count <= randomCount) {
			for (int i = 0; i < array.Length; i++) {
				if (!ListT.Contains (array [i])) {
					ListT.Add (array [i]);
				}
			}
		}
		return ListT;
	}

	public static Dictionary<T1,T2> ToDictionary<T1, T2>(KeyValuePair<T1, T2> kvp)
	{
		var dict = new Dictionary<T1, T2>();
		Debug.LogError (kvp.Key);
		dict.Add(kvp.Key, kvp.Value);
		return dict;
	}

	public static bool CheckArray<T> (T[] array, T param){
		return array.Any (f => f.Equals(param));
	}


	public static TargetType ReverseTarget(TargetType tType){
		if (tType == TargetType.Player) {
			return TargetType.Enemy;
		} 
		else {
			return TargetType.Player;
		}
	}
}
