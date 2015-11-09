using UnityEngine;
using System.Collections;


public class MainGameScript : MonoBehaviour {
	public static MainGameScript instance;
	public int MyFrags=0,HisFrags=0;
	public int MyLeft = 500,HisLeft = 500;
	public string MyName,HisName;
	public int LevelNum = 1; 
	public GameStatus _gamestatus = GameStatus.starting;
	
	
	public int IDtokill = -1,IDtoDie = -1;
	
	// нормировочные коэффициенты 
	public float CircleSpeedK,SizeK = .015f; 
	
	
	public int CircleIDCounter = 0;
	public float ScreenLeft=10,ScreenTop=10,ScreenRight=20,ScreenBottom=-20;
	
	// service
	public bool TouchedFlag = false;
	
	Camera MainCamera;
	void Awake(){
			instance = this;
			
	}
	// Use this for initialization
	void Start () {
		Time.timeScale = 1;
		MainCamera = GameObject.Find("Main Camera").camera;
		StartCoroutine(MainGameCoroutine());
		CircleSpeedK = 1;
		AdjustingScreenResolution(); 
		
	}
	
	// Update is called once per frame
	void Update () {
		if(_gamestatus != GameStatus.going)
			return;
		int TouchX=0,TouchY=0;
				if (Input.GetMouseButtonDown(0))
					{
						TouchX = (int) Input.mousePosition.x;
				 		TouchY = (int) Input.mousePosition.y;
						
						TouchedFlag = true;
				}
				if (TouchedFlag){
						FindAimCircle(TouchX,TouchY);
						TouchedFlag = false;
				}
		
	}
	
	void FindAimCircle(int X,int Y){
		Ray ray;
			Vector3 ActionPos;
			RaycastHit localhit;
			GameObject ObjectToDestroy;
				ActionPos.x = X;
				ActionPos.y = Y;
				ActionPos.z = 0;
				ray = MainCamera.ScreenPointToRay(ActionPos);
				if (Physics.Raycast(ray, out localhit, 100)){
					ObjectToDestroy = localhit.collider.gameObject;
						if (ObjectToDestroy.tag == "Circle")
							{
								
								ObjectToDestroy.GetComponent<CircleScript>().Killed();
							}
				}
			
}
	
	void AdjustingScreenResolution(){
		Plane AdjustPlane;
		float Distance;
		Vector3 PointPlane;
			AdjustPlane = new Plane(new Vector3(-10,-10,0),new Vector3(10,-10,0),new Vector3(10,10,0));
			// left top
			Ray MyRay = MainCamera.ScreenPointToRay(new Vector3(0, Screen.height, 0));
			AdjustPlane.Raycast(MyRay,out Distance);
			PointPlane = MyRay.GetPoint(Distance);	
			ScreenLeft = PointPlane.x;
			ScreenTop = PointPlane.y;
			// right bottom
			MyRay = MainCamera.ScreenPointToRay(new Vector3(Screen.width, 0, 0));
			AdjustPlane.Raycast(MyRay,out Distance);
			PointPlane = MyRay.GetPoint(Distance);	
			ScreenRight = PointPlane.x;
			ScreenBottom = PointPlane.y;
			
			
	}
	IEnumerator MainGameCoroutine(){
		float _timer;
		// checking resources
		while(ResManagerScript.ImBusy)
			yield return new WaitForSeconds(.1f);
		// checking network
		while(NetworkManagerScript.instance.NetwInterfaceEx == null || !NetworkManagerScript.instance.NetwInterfaceEx.ConnectionStatus) // client or server doesnt matter
			yield return new WaitForSeconds(.1f);
		
		
		// turn on the central strip
		GameObject tmpObj = GameObject.Find("CentralPlane").gameObject;
		// sending myname to other side
		tmpObj.renderer.enabled = true;
		var _msgPacket = new MsgPacket();
		_msgPacket._msgtype = MsgType.namechange;
		_msgPacket.RawData = MyName;
		NetworkManagerScript.instance.SendMessageViaNet(_msgPacket);
		
		Color _color = new Color(Random.value,Random.value,Random.value,1); // first time textures creatin
		
		StartCoroutine(ResManagerScript.instance.MaterialCreate(_color,true));
		
		while(ResManagerScript.ImBusy)
			{
				
				yield return new WaitForSeconds(.2f);
			}
		// tell first time other side to create texture
		
		TellOtherSideToGenerateTextures(_color);
		while(ResManagerScript.ImBusy)
			{
				
				yield return new WaitForSeconds(.2f);
			}
		// timeout to receive other side init packages from net. must be optimized
		yield return new WaitForSeconds(.5f);
		
		// texture Swap
		ResManagerScript.instance.TextureSwap(true);
		TellOtherSideToChangeTextures();
		
		//!!!!!
		while(ResManagerScript.ImBusy)
			yield return new WaitForSeconds(.1f);
		
		
		
		
		_color = new Color(Random.value,Random.value,Random.value,1); // prepare next matrial
		
		StartCoroutine(ResManagerScript.instance.MaterialCreate(_color,true));
		while(ResManagerScript.ImBusy)
			{
				
				yield return new WaitForSeconds(.2f);
			}
		TellOtherSideToGenerateTextures(_color);
		
		
		
		// is resManager free? all resources are created 
		while(ResManagerScript.ImBusy)
			yield return new WaitForSeconds(.1f);
		
		// start 
		_gamestatus = GameStatus.going;
		InvokeRepeating("BornMyCircle",1,1.5f);// debug
		_timer = Time.time;
		while (_gamestatus == GameStatus.going){
			if(Time.time > _timer+20){
				_timer = Time.time;
				// Increaseing speed changing TexturePack
				NextLevel();
			}
			yield return new WaitForSeconds(.02f);
		}
		CancelInvoke("BornMyCircle");
		
		
		
	}
	public void NextLevel(){
		LevelNum++;
		MessageServiceClass.MessageProcessing ("New ( "+LevelNum.ToString()+" )Level'll be a little bit faster");
		// increase speed
		CircleSpeedK *=  1.1f;
		// change Pack
		ResManagerScript.instance.TextureSwap(true);
		// tells other side... imchanging material
		TellOtherSideToChangeTextures();
		// Start generate new textures
		Color _color = new Color(Random.value,Random.value,Random.value,1);
		StartCoroutine(ResManagerScript.instance.MaterialCreate(_color,true));
		// 
		
		
		TellOtherSideToGenerateTextures(_color);
		
		
	}
	public void TellOtherSideToGenerateTextures(Color _color){
		var _msgPacket = new MsgPacket();
		_msgPacket._msgtype = MsgType.textureGenerate;
		_msgPacket._textureColor = new SeriaziableColor(_color);
		_msgPacket._timeStamp = Time.time;
		NetworkManagerScript.instance.SendMessageViaNet(_msgPacket);
		
	}
	public void TellOtherSideToChangeTextures(){
		var _msgPacket = new MsgPacket();
		_msgPacket._msgtype = MsgType.textureChanging;
		_msgPacket._timeStamp = Time.time;
		NetworkManagerScript.instance.SendMessageViaNet(_msgPacket);
		
	}//TellOtherSideToChangeTextures
	/// <summary>
	/// Borns the new circle.
	/// </summary>
	/// <returns>
	/// The new circle.
	/// </returns>
	/// <param name='_normalizedposition'>
	/// _normalizedposition. 0 - 0.5 in screen part
	/// </param>
	/// <param name='ObjectSize'>
	/// Object size.
	/// </param>
	/// <param name='ID'>
	/// I.
	/// </param>
	/// <param name='_tag'>
	/// _tag.
	/// </param>
	/// <param name='textureID'>
	/// Texture I.
	/// </param>
	/// <param name='_speed'>
	/// _speed.
	/// </param>
	public GameObject BornNewCircle(Vector3 _normalizedposition, float ObjectSize,int ID,string _tag,int textureID,float _speed){
		float size = (2+ObjectSize)*SizeK; 
		GameObject BornObject;
		Quaternion rotation = Quaternion.Euler(0,180, 0);
		CircleScript CircleScriptEx;
		
			// Scale position to screen coords
			Vector3 position = new Vector3 (ScreenLeft+_normalizedposition.x*(ScreenRight-ScreenLeft) ,ScreenTop+_normalizedposition.y*(ScreenBottom-ScreenTop),0);
		
			BornObject = GameObject.Instantiate(ResManagerScript.instance.CircleGameObjPref,position, rotation) as GameObject;
			// scaling
			BornObject.transform.localScale = new Vector3(size,size,size);
			// collider add
			BornObject.AddComponent("SphereCollider");
			CircleScriptEx = BornObject.AddComponent<CircleScript>();
			CircleScriptEx.size = ObjectSize;
			CircleScriptEx.speed = _speed;
			CircleScriptEx.TextureID = textureID;
			CircleScriptEx.MyID = ID;
			BornObject.tag = _tag;
			// TextureChanging
			BornObject.renderer.material.SetTexture("_MainTex",ResManagerScript.instance.WorkTextures2d[textureID]);
		
		return(BornObject);
		
	}
	public void BornMyCircle(){
		GameObject BornObject;
		Vector3 BornTransform;
		float RndVal;
		
			RndVal = Random.Range(2f,6f);
			
			
			BornTransform = new Vector3(Random.Range(0.05f,0.45f),0,0);
			BornObject = BornNewCircle(BornTransform,RndVal,CircleIDCounter,"Circle",(int)(RndVal)-2,0);
			
			BornObject.GetComponent<CircleScript>().speed = ((10-RndVal))*CircleSpeedK*-0.5f;
			BornObject.GetComponent<CircleScript>().MyID = CircleIDCounter++;
			BornObject.GetComponent<CircleScript>().initObjWithParams(true); // true  - My circle will send message of his bornin
			
			
			
	}
}
