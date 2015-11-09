using UnityEngine;
using System.Collections;

public class StatWinScript : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
			
	}
	void OnGUI(){
		int W,H;
		string statStr = "";
		W = Screen.width;
		H = Screen.height;
		statStr = NetworkManagerScript.instance.ToString();
		GUI.Box(new Rect(W*.7f,H*.6f,W*.25f,H*.25f),"statistic:\n"+statStr );
		
	}
}
