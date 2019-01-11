using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyUserData  {

	private static List<TeamLargeData> TeamListData;

	public static TeamLargeData GetTeamData(int teamIdx){
		return TeamListData[teamIdx];
	}

	public static List<TeamLargeData> GetTeamListData(){
		return TeamListData;
	}




	public static void UpdataUserdata(MyUserLargeData userData){
		Debug.LogWarning ("Update User Data");
		TeamListData = userData.TeamData;
	}

}
