using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InterFaceManagerScript : MonoBehaviour {
	public static  InterFaceManagerScript instance;
	bool ShowMessageLogFlag = true;
	public GUIStyle ScoreStyle;
	
	public static int  MaxStrCount = 15;
	public string MessageString = "";
	void Awake(){
			instance = this;
			
	}
	//
	// Use this for initialization
	void Start () {
		// init first string list
		MessageServiceClass.Inizialize();
		MessageServiceClass.MessageProcessing("Starting..");
	}
	
	// Update is called once per frame
	void Update () {
			if (MessageServiceClass.activityflag)
				{
					SendMyMessage(MessageServiceClass.MessageString);
				}
			if (Input.GetKeyDown("`"))
				ShowMessageLogFlag = !ShowMessageLogFlag;
			if(Input.GetKeyDown(KeyCode.Escape))
				{	
					MainGameScript.instance._gamestatus = GameStatus.paused;
					StartCoroutine( NetworkManagerScript.instance.CloseConnectionsAndRestart());
					
				}
			
	}
	void OnGUI(){
		int W,H;
		W = Screen.width;
		H = Screen.height;
		if (ShowMessageLogFlag){
			GUI.Label(new Rect(W*.2f,H*.45f,W*.7f,H*.55f),MessageString);	
			if (GUI.Button(new Rect(W*.1f,H*.1f,W*.2f,H*.1f),"Exit"))
				Application.Quit();
		}
		if(NetworkManagerScript.instance.NetwInterfaceEx != null && NetworkManagerScript.instance.NetwInterfaceEx.ConnectionStatus){
			GUI.Label(new Rect(W*.05f,.05f*H,W/2,H/2),MainGameScript.instance.MyName,ScoreStyle);
			GUI.Label(new Rect(W*.05f,.1f*H,W/2,H/2),"Score: "+MainGameScript.instance.MyFrags.ToString(),ScoreStyle);
			GUI.Label(new Rect(W*.05f,.15f*H,W/2,H/2),"Balls Left: "+MainGameScript.instance.MyLeft.ToString(),ScoreStyle);
		
			GUI.Label(new Rect(W*.55f,.05f*H,W/2,H/2),MainGameScript.instance.HisName,ScoreStyle);
			GUI.Label(new Rect(W*.55f,.1f*H,W/2,H/2),"Score: "+MainGameScript.instance.HisFrags.ToString(),ScoreStyle);
			GUI.Label(new Rect(W*.55f,.15f*H,W/2,H/2),"Balls Left: "+MainGameScript.instance.HisLeft.ToString(),ScoreStyle);
		}	
		if(MainGameScript.instance.MyLeft < 1)
			{
				MainGameScript.instance._gamestatus = GameStatus.lose;
				GUI.Label(new Rect(W*.05f,.55f*H,W/2,H/2),"You Lost",ScoreStyle );
			}
		if(MainGameScript.instance.HisLeft < 1)
			{
				MainGameScript.instance._gamestatus = GameStatus.win;
				GUI.Label(new Rect(W*.05f,.55f*H,W/2,H/2),"You Win",ScoreStyle );
			}
		if(MainGameScript.instance._gamestatus == GameStatus.lose || MainGameScript.instance._gamestatus == GameStatus.win)
			{
				if (GUI.Button(new Rect(W*.1f,H*.61f,W*.2f,H*.1f),"Exit"))
					Application.Quit();
				if(GUI.Button(new Rect(W*.41f,H*.61f,W*.2f,H*.1f),"Again")){
					StartCoroutine( NetworkManagerScript.instance.CloseConnectionsAndRestart());
					
				}
			}
		
	}
	public void SendMyMessage(string message){
		

			Debug.Log (message);
			MessageServiceClass.activityflag = false;
	}
}
/// <summary>
/// Messenger service class. need to serve messageout from different (include background) threads 
/// </summary>

public static class MessageServiceClass{
		public static bool activityflag = false;
		public static string MessageString;
		public static List<string> Messages = new List<string>();
		public static void Inizialize(){
			Messages.Clear();
			for (int i = 0; i < InterFaceManagerScript.MaxStrCount;i++)
				Messages.Add(".");
		}
		public static void MessageProcessing(string _message){
			Messages.RemoveAt(0);
			Messages.Add(_message);
			MessageString = "";	
			for (int i = 0;i < InterFaceManagerScript.MaxStrCount;i++)
				{
					MessageString += Messages[i]+"\n";
				}
			InterFaceManagerScript.instance.MessageString = MessageString;
			activityflag = true;
			
	}
}