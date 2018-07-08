using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using model.data;

public static class MasterDataManager {

	private static List<CharaLargeData> charaLargeData;

	public static List<CharaLargeData> GetCaraLargeData{
		get{ 
			return charaLargeData;
		}
	}

	public static List<SkillLargeData> skillLargeData;

	public static List<SkillLargeData> GetSkillLargeData{
		get{ 
			return skillLargeData;
		}
	}

	public static List<RuleLargeData> ruleLargeData;

	public static List<RuleLargeData> GetRuleLargeData{
		get{ 
			return ruleLargeData;
		}
	}

	public static CharaLargeData GetCharaData(int id){
		return DataUtil.GetById<CharaLargeData> (id, ref charaLargeData);
	}

	public static void UpdataMasterdata(ClientLargeData clientData){
		charaLargeData = clientData.Chara;

		skillLargeData = clientData.Skill;

		ruleLargeData = clientData.Rule;
	}
}

