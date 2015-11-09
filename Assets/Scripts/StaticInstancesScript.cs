using UnityEngine;
using System.Collections;
using System.Threading;


public class StaticInstancesScript : MonoBehaviour {
	public static  StaticInstancesScript instance;

	void Awake(){
			instance = this;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}
}
