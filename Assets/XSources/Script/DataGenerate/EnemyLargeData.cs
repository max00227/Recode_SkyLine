using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLargeData : MonoBehaviour {
    [Newtonsoft.Json.JsonProperty("Team")]
	public System.Collections.Generic.List<TeamLargeData> teams { get; set; }
}
