using UnityEngine;
using System.Collections;

public class CircleScript : MonoBehaviour {
	public float size;
	public float speed;
	public int MyID;
	public int TextureID;
	// Use this for initialization
	void Start () {
		
		
		
		
	
	}
	/// <summary>
	/// Inits the object with parameters. now its  method with 1 parameter.  Do I Need  place this Object to the net?
	/// </summary>
	/// <param name='param'> 
	/// Parameter.
	/// </param>
	public void initObjWithParams(bool needIsendThis){
		if (!needIsendThis)
			return;
		var _msgPacket = new MsgPacket();
		_msgPacket.IdObj = MyID;
		_msgPacket._msgtype = MsgType.born;
		_msgPacket._objectSize = size;
		
		// normalize position
		float ScreenLeft = MainGameScript.instance.ScreenLeft, ScreenTop = MainGameScript.instance.ScreenTop;
		float ScreenRight = MainGameScript.instance.ScreenRight ,ScreenBottom = MainGameScript.instance.ScreenBottom;
		
		Vector3 _normalizedPosition = new Vector3((transform.position.x - ScreenLeft) / (ScreenRight-ScreenLeft),(transform.position.y - ScreenTop)/(ScreenBottom-ScreenTop),0);
		
		_msgPacket._objectPosition = new SerializableVector3(_normalizedPosition);
		_msgPacket._timeStamp = Time.time;
		_msgPacket._objSpeed = speed;
		_msgPacket._textureID = TextureID+4;
		NetworkManagerScript.instance.SendMessageViaNet(_msgPacket);
	}
	
	// Update is called once per frame
	void Update () {
		if(MainGameScript.instance._gamestatus != GameStatus.going)
			return;
		// changing coords
		transform.position += new Vector3(0,speed*Time.deltaTime,0); // attention Speed is negative value
		if (transform.position.y < MainGameScript.instance.ScreenBottom)
			if (tag == "Circle" || NetworkManagerScript.instance.IsTestServer)
			{
				
			  	Die();
			}
		if(/*MainGameScript.instance.IDtoDie != -1 &&*/ MainGameScript.instance.IDtoDie == MyID && tag == "OtherPlayerCircle")
			{
				MainGameScript.instance.IDtoDie = -1;
				
				Die ();
				
			}
		if(/*MainGameScript.instance.IDtokill != -1 &&*/ MainGameScript.instance.IDtokill == MyID &&  tag == "OtherPlayerCircle")
			{
				MainGameScript.instance.IDtokill = -1;
				
				Killed ();
				
			}
		
		
		
		
	}
	public void Die(){
		Debug.Log (MyID+"is Dead");
		// if local, send to oher side
		if (tag == "Circle"){
			var _msgPacket = new MsgPacket();
			_msgPacket.IdObj = MyID;
			_msgPacket._msgtype = MsgType.die;
			_msgPacket._timeStamp = Time.time;
			NetworkManagerScript.instance.SendMessageViaNet(_msgPacket);
			MainGameScript.instance.MyLeft--;
		}
		 else
		{
			MainGameScript.instance.HisLeft--;
		}
		Destroy(gameObject);
	}
	public void Killed(){
		GameObject _gameobject = Instantiate(TypesConstantsScript.instance.PartSyst,transform.position,transform.rotation) as GameObject;
		// if local, send to oher side
		if (tag == "Circle"){		
			var _msgPacket = new MsgPacket();
			_msgPacket.IdObj = MyID;
			_msgPacket._msgtype = MsgType.killd;
			_msgPacket._timeStamp = Time.time;
			NetworkManagerScript.instance.SendMessageViaNet(_msgPacket);
			MainGameScript.instance.MyFrags += 6-(int)size;
		}
		else
		{
			MainGameScript.instance.HisFrags +=6-(int)size;
		}
		Destroy(_gameobject,3);
		Destroy(gameObject);
	}
}
