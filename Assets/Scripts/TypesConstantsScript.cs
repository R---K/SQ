using UnityEngine;
using System.Collections;

public class TypesConstantsScript : MonoBehaviour {
	public static TypesConstantsScript instance;
	public GameObject PartSyst;
	// Use this for initialization
	void Awake(){
		instance = this;
	}
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
public enum MsgType{
	born,
	die,
	killd,
	textureGenerate,
	textureChanging,
	namechange,
	reserved
	
}
public enum GameStatus{
	starting,
	going,
	win,
	lose,
	paused
}