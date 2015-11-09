using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading;

public enum MyNetworkRole{
		ImServer,
		ImClient,
		none,
		reserved
}

public class NetworkManagerScript : MonoBehaviour {
	public MyNetworkRole MynetwStats=MyNetworkRole.none ; // My networkStatus
	public static NetworkManagerScript instance;
	public bool NeedIconnectionMenu = true,ServerBusyflag = false,IsTestServer = false;
	public NetwInterface DemoServerInterfaceEx,NetwInterfaceEx;
	public int DemoServerCircleID=0; 
	public int PackageCounter;
	

	IPEndPoint ipMyPoint,ipTargetPoint = null;
	IPEndPoint ipServerDemoMypoint,ipServerDemoTargetPoint; // in case local DemoServer
	
	string stringToEdit = "0.0.0.0";
	IPAddress tmpIPAdress = IPAddress.Parse("0.0.0.0");
	
	
	
	
	
	// Use this for initialization
	void Awake(){
			instance = this;
	}
	void Start () {

			NetworkStart();
			
			
			
	}
	
	// Update is called once per frame
	void Update () {
		

		
		if (NetwInterfaceEx != null)
			if ( NetwInterfaceEx.PackageAr.Count > 0){
				var _msgPacket = NetwInterfaceEx.ProcessingReceivedMessage((byte[])NetwInterfaceEx.PackageAr[0]);
				NetwInterfaceEx.PackageAr.RemoveAt(0);
			
				if(_msgPacket._msgtype == MsgType.namechange){
						
						MainGameScript.instance.HisName = _msgPacket.RawData;	
				}
				if (_msgPacket._msgtype == MsgType.born){
					
					var NewCirclePos = new Vector3(_msgPacket._objectPosition.x+0.5f,_msgPacket._objectPosition.y,_msgPacket._objectPosition.z);
					
				
					MainGameScript.instance.BornNewCircle(NewCirclePos,_msgPacket._objectSize, _msgPacket.IdObj,"OtherPlayerCircle",_msgPacket._textureID,_msgPacket._objSpeed);
				}// born new circle
			
				if(_msgPacket._msgtype == MsgType.textureGenerate)
					{
						
						var _color = new Color(_msgPacket._textureColor.r,_msgPacket._textureColor.g,_msgPacket._textureColor.b,1);
						
						StartCoroutine( ResManagerScript.instance.MaterialCreate(_color,false)); 
						
					} // generate new texture
			
				if(_msgPacket._msgtype == MsgType.textureChanging)
					ResManagerScript.instance.TextureSwap(false);
				if(_msgPacket._msgtype == MsgType.die)
					MainGameScript.instance.IDtoDie = _msgPacket.IdObj;
				if(_msgPacket._msgtype == MsgType.killd)
					{
						MainGameScript.instance.IDtokill = _msgPacket.IdObj;
						MessageServiceClass.MessageProcessing("KillMessage received ID "+_msgPacket.IdObj.ToString());
					}
				
				
			}				
	}
	void OnGUI(){
		int W,H;
		
		if(!NeedIconnectionMenu)
			return;
		if(NetwInterfaceEx != null)
			return;
		
		W = Screen.width;
		H = Screen.height;
		// My network ststus not defined
		if(MynetwStats == MyNetworkRole.none)
		{
			if(GUI.Button(new Rect(W*.1f,H*.25f,W*.3f,H*.1f),"Start As Server")){
				MynetwStats = MyNetworkRole.ImServer;
				ServerStart(ipMyPoint); //Here is starting the server
			}
			if(GUI.Button(new Rect(W*.1f,H*.35f,W*.3f,H*.1f),"Start As null DemoServer")){
				MynetwStats = MyNetworkRole.ImClient;
				IsTestServer = true;
				stringToEdit = IPAddress.Loopback.ToString();//!!!!!!!!!!
			}
			if (GUI.Button(new Rect(W*.6f,H*.25f,W*.3f,H*.1f),"Start As Client")){
				MynetwStats = MyNetworkRole.ImClient;
				
				
			}
			GUI.Label(new Rect(W*.51f,H*.05f,W*.4f,H*.2f),"Enter Your Name here:");
			MainGameScript.instance.MyName = GUI.TextField(new Rect(W*.51f,H*.1f,W*.4f,H*.08f),MainGameScript.instance.MyName,16);
			
		}// My network ststus not defined
		// receive server target IP
		if(MynetwStats == MyNetworkRole.ImClient)
		{
			
		
			stringToEdit = GUI.TextField(new Rect(W*.1f,H*.25f,W*.4f,H*.1f),stringToEdit,16);
			
			
			
			if(GUI.Button(new Rect(W*.6f,H*.25f,W*.3f,H*.1f),"GoGoGo!")){
				if(IPAddress.TryParse(stringToEdit,out tmpIPAdress))
				{	
				
					ipTargetPoint = new IPEndPoint(tmpIPAdress, 1100);
				}
				else
				{
					stringToEdit = ipTargetPoint.Address.ToString();
				}
				
				
				
				// connection starts
				if(IsTestServer) // debug IpTargetpoint sets to loopback address
					{
						StartCoroutine(DemoServerStart());
					}
				ClientStart(ipMyPoint,ipTargetPoint);
				
			}
			
			
			
		}// receive server target IP
}
	public IEnumerator CloseConnectionsAndRestart(){
						
		
			if(NetwInterfaceEx != null)
				if ( NetwInterfaceEx.ConnectionStatus)
					if(MynetwStats == MyNetworkRole.ImClient)
					{
						NetwInterfaceEx.Disconnect();
						MessageServiceClass.MessageProcessing("Client Interface closed");
					}
		 		  else
					{
						yield  return new WaitForSeconds(1);
						NetwInterfaceEx.Disconnect();
						MessageServiceClass.MessageProcessing("Server Interface closed");
						// timeout to close
						
					}
			if(DemoServerInterfaceEx != null)
				if( DemoServerInterfaceEx.ConnectionStatus){
					yield  return new WaitForSeconds(1);	
					NetwInterfaceEx.Disconnect();
					MessageServiceClass.MessageProcessing("Demoserver closed");
				}
			yield  return new WaitForSeconds(1);
			
			Application.LoadLevel(0);
		}
	
	
	public void NetworkStart(){
		
			IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            ipMyPoint = new IPEndPoint(ipAddr, 1100);
			MessageServiceClass.MessageProcessing ("Host Name "+Dns.GetHostName().ToString());
			MessageServiceClass.MessageProcessing (ipMyPoint.ToString());
	}
	
	public IEnumerator DemoServerStart(){
			
			DemoServerInterfaceEx = new DemoServer();
			MessageServiceClass.MessageProcessing ("DemoServer was created"+DemoServerInterfaceEx.ToString());
			DemoServerInterfaceEx.StartMain(); //!!!
			
			while( !DemoServerInterfaceEx.ConnectionStatus)
				yield return new WaitForSeconds(.1f);
			ChangeTextureInDemoServer(); // first launch
			InvokeRepeating("DemoServerRunBot",3,1.5f);
			
			InvokeRepeating("ChangeTextureInDemoServer",1,20f);
		
	}
	
	
	
	void ClientStart(IPEndPoint _ipMyPoint,IPEndPoint _ipTargetPoint){
			
			NetwInterfaceEx = new Client();
			MessageServiceClass.MessageProcessing ("Client was created"+NetwInterfaceEx.ToString()+"with target"+_ipTargetPoint.Address.AddressFamily);
			NetwInterfaceEx.StartMain(_ipMyPoint, _ipTargetPoint);
			
	}
	
	void ServerStart(IPEndPoint _ipMyPoint){
			NetwInterfaceEx = new Server();
			MessageServiceClass.MessageProcessing ("Server was created"+_ipMyPoint.Address.ToString());
			NetwInterfaceEx.StartMain(_ipMyPoint);
	}
	
	/// <summary>
	/// Sends the message via net.
	/// Serialize _msgPacket pushes it to network
	/// </summary>
	/// <param name='_msgPacket'>
	/// _msg packet.
	/// </param>
	
	public void SendMessageViaNet(MsgPacket _msgpacket){
			SendMessageViaNet(NetwInterfaceEx,_msgpacket);
	}
	//overload previous method with point of network interface to push packet
	public void SendMessageViaNet(NetwInterface _netInterface,MsgPacket _msgPacket){
			_msgPacket._packagenum = PackageCounter++;
		// serialize
			byte []MyData;
			System.IO.MemoryStream MyMemoryStream;
			MyMemoryStream =  new System.IO.MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			
			formatter.Serialize(MyMemoryStream, _msgPacket);
			MyData = MyMemoryStream.ToArray();
			
			
		// SendTo net
			
			_netInterface.Send(MyData);
		
				
	}

	/// <summary>
	/// Debug! Born New circles and send they via net 
	/// </summary>
	public void DemoServerRunBot(){
		if( NetwInterfaceEx == null)
			return;
		if ( !NetwInterfaceEx.ConnectionStatus)
			return;
		float RndVal = UnityEngine.Random.Range(2f,6f);
		
		
		
		
		
		
		
		
		var _msgPacket = new MsgPacket();
		float _floatPos = .25f; // left quarter of screen
		
		_msgPacket._objectPosition  = new SerializableVector3(new Vector3(_floatPos,0,0));
		
		_msgPacket.IdObj = DemoServerCircleID++;
		_msgPacket._msgtype = MsgType.born;
		_msgPacket._objectSize = RndVal;
		_msgPacket._objSpeed = ((10-RndVal))*-0.5f;
		_msgPacket._textureID = UnityEngine.Random.Range(4,7);
		
		
		

		_msgPacket._timeStamp = Time.time;
		SendMessageViaNet(DemoServerInterfaceEx, _msgPacket);
			
	}
	
	public void ChangeTextureInDemoServer(){
		var _msgPacket = new MsgPacket();
		MessageServiceClass.MessageProcessing("Demo Server Texure was changed");
		_msgPacket._msgtype = MsgType.textureChanging;
		_msgPacket._timeStamp = Time.time;
		SendMessageViaNet(DemoServerInterfaceEx, _msgPacket);
			
		
		Color _color = new Color(UnityEngine.Random.value,UnityEngine.Random.value,UnityEngine.Random.value,1);
		
		_msgPacket._msgtype = MsgType.textureGenerate;
		_msgPacket._textureColor = new SeriaziableColor(_color);
		_msgPacket._timeStamp = Time.time;
		SendMessageViaNet(DemoServerInterfaceEx, _msgPacket);
		
		
	}
	

}// NetworkManagerScript

public class MyStateClass {
	public Socket workSocket = null;
    public byte[] Buffer = new byte[1024];
}

/// <summary>
/// Netw interface. abstract class  needs to create interface for mainGameLogic
/// </summary>
public class NetwInterface  {
	public bool ConnectionStatus = false;
	public Socket MySocket;
	public IPEndPoint ipMyPoint, ipTargetPoint;
	public ArrayList  PackageAr =  new ArrayList();
	
	public int bytesRead=0;
	public bool SentDoneFlag = true;
	
	 private static ManualResetEvent sendDone = new ManualResetEvent(false);
	// methods
	public virtual int StartMain(){
		
		return 0;
	}
	public virtual int StartMain(IPEndPoint _ipMyPoint){return 0;}
	public virtual int StartMain(IPEndPoint _ipMyPoint,IPEndPoint _ipTargetPoint){
		ipMyPoint = _ipMyPoint;
		ipTargetPoint = _ipTargetPoint;
		return (0);
	}
	public virtual void Disconnect(){
		ConnectionStatus = false;

		if(MySocket.Connected)
			MySocket.Shutdown(SocketShutdown.Both);
		MySocket.Close();
		
	}
   
	
	
	/// <summary>
	/// NetwInterface.Send the specified buffer.
	/// </summary>
	/// <param name='buffer'>
	/// Buffer.
	/// </param>
	public  virtual int Send(byte [] _buffer){
		SentDoneFlag = false;
			if(ConnectionStatus)
				MySocket.BeginSend(_buffer, 0, _buffer.Length, 0,new AsyncCallback(SendCallback), MySocket);
		return 0;
	}// send()
	
	private  void SendCallback(IAsyncResult ar) {
        try {
            // Retrieve the socket from the state object.
            Socket client = (Socket) ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = client.EndSend(ar);
           
			SentDoneFlag = true;
            // Signal that all bytes have been sent.
            sendDone.Set();
        } 
		catch (Exception e) {
			SentDoneFlag = true;
            MessageServiceClass.MessageProcessing(e.ToString());
        }
    }
	public void BeginReceive(){
		MyStateClass MyStateObj = new MyStateClass();
		MyStateObj.workSocket = MySocket;
				MySocket.BeginReceive(MyStateObj.Buffer, 0,MyStateObj.Buffer.Length,0, new AsyncCallback(ReceiveCallback), MyStateObj );
			
	}
	private void ReceiveCallback(IAsyncResult ar){
		MyStateClass MyStateObj = (MyStateClass) ar.AsyncState;
		
		bytesRead = MyStateObj.workSocket.EndReceive(ar);
		PackageAr.Add(MyStateObj.Buffer);	
		
		
	} // ReceiveCallback
	
	public MsgPacket ProcessingReceivedMessage(Byte [] Data){
			/// exemplair of _msgPacket
			var _msgPacket = new MsgPacket();
			var MyMemoryStream =  new System.IO.MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			
			MyMemoryStream.Write(Data,0,Data.Length);
			MyMemoryStream.Position = 0;
			_msgPacket = (MsgPacket) formatter.Deserialize(MyMemoryStream);
			
			BeginReceive();
			
			return _msgPacket;
	}
	

	
		
		
}// NewtWInterface

public class Server :  NetwInterface{
	
	public override int StartMain(IPEndPoint _ipMyPoint){
			
			MySocket = new Socket (_ipMyPoint.Address.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
			MySocket.Bind(_ipMyPoint);
			MySocket.Listen(3); // Wait only 1 connections
			// Waiting for connect & set other side socket	
			MySocket.BeginAccept(new AsyncCallback(AcceptCallback), MySocket);
			MessageServiceClass.MessageProcessing  ("Server begin to listen from address "+_ipMyPoint.Address.AddressFamily.ToString());
			return 0;	
	}
	
	private void AcceptCallback(IAsyncResult result)
    	{

        try
        {
            // Завершение операции Accept
            Socket s = (Socket)result.AsyncState;
            MySocket = s.EndAccept(result);
            
            MessageServiceClass.MessageProcessing  ("SERVER: Server Connected");
			ConnectionStatus = true;
			BeginReceive();
 
   
        }
        catch (SocketException exc)
        {
            
			SentDoneFlag = true;
            Debug.Log("Socket exception: " + exc.SocketErrorCode);
        }
        catch (Exception exc)
        {
            
			SentDoneFlag = true;
            Debug.Log("Exception: " + exc);
        }
		
		
    } // AcceptCallback
	
	
}// class Server :  NetwInterface 

/// <summary>
/// Client.
/// </summary>
/// <summary>
/// Client.
/// </summary>
public class  Client : NetwInterface{
	public override int StartMain(IPEndPoint _ipMyPoint,IPEndPoint _ipTargetPoint){
			ipMyPoint = _ipMyPoint;
			ipTargetPoint = _ipTargetPoint;
			MySocket = new Socket(ipMyPoint.Address.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
			MessageServiceClass.MessageProcessing ("Client begin to connect to"+ipTargetPoint);
			///////////////////////////////////////
			// Connect to a remote device.
        	try {
           	    	// Connect to the remote endpoint.
            		MySocket.BeginConnect(ipTargetPoint,new AsyncCallback(ConnectCallback), MySocket);
                } 
			catch (Exception e) 
				{
					SentDoneFlag = true;
            		Debug.Log (e.ToString());
        		}
		return 0;
    }// startMain

    private void ConnectCallback(IAsyncResult ar) {
        	try {
 	           		Socket client = (Socket) ar.AsyncState;
   	    	    	// Complete the connection.
            		client.EndConnect(ar);
					
            		MessageServiceClass.MessageProcessing("Client connected to "+client.RemoteEndPoint.ToString());
					ConnectionStatus = true;
					BeginReceive();
        		} 
			catch (Exception e) 
				{
					SentDoneFlag = true;
            		Debug.Log(e.ToString());
        		}
 	} // ConnectCallback

}// Client

/// <summary>
/// Demo server.
/// </summary>
public class DemoServer : NetwInterface{
	

	
	public override int StartMain(){
			IPEndPoint MyServerIPPoint = new IPEndPoint(IPAddress.Loopback,1100);	
			MySocket = new Socket (MyServerIPPoint.Address.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
			//MessageServiceClass.MessageProcessing  ("<=========Demo Server Try to bind "+MyServerIPPoint.Address.ToString());
			Debug.Log("<=========Demo Server Try to bind "+MyServerIPPoint.Address.ToString());
			try{
				MySocket.Bind(MyServerIPPoint);
			}
			catch(SocketException exc){
				MessageServiceClass.MessageProcessing("Socket exception: " + exc.SocketErrorCode.ToString());
			}
			MessageServiceClass.MessageProcessing  ("<=========Demo Server binded and Try to begin listen");
			MySocket.Listen(1); // Wait only 1 connections
			// Waiting for connect & set other side socket	
			
			MySocket.BeginAccept(new AsyncCallback(AcceptCallback), MySocket);
			MessageServiceClass.MessageProcessing  ("Demo Server begin to listen from address "+MyServerIPPoint.Address.AddressFamily.ToString());
			return 0;
	}
	 private void AcceptCallback(IAsyncResult result)
    	{

        try
        {
            // Завершение операции Accept
            Socket s = (Socket)result.AsyncState;
            MySocket = s.EndAccept(result);
            
            MessageServiceClass.MessageProcessing  ("SERVER: Demo Server Connected");
			ConnectionStatus = true;
 			BeginReceive();
   
        }
        catch (SocketException exc)
        {
            
			SentDoneFlag = true;
            MessageServiceClass.MessageProcessing("Socket exception: " + exc.SocketErrorCode.ToString());
			
        }
        catch (Exception exc)
        {
			SentDoneFlag = true;
           
            Debug.Log("Exception: " + exc);
        }
		
		
    } // AcceptCallback
	
	/// <summary>
	/// Receives the callback.
	/// </summary>
	/// <param name='result'>
	/// Result.
	/// </param>



} //demoserver class

[Serializable]
public class SerializableVector3{
		public float x,y,z;
		public  SerializableVector3(Vector3 _vector3){
			x=_vector3.x;y=_vector3.y;z=_vector3.z;
		}
}
[Serializable]
public class SeriaziableColor {
		public float r,g,b;
		public SeriaziableColor(Color _color){
			r = _color.r;
			g = _color.g;
			b = _color.b;
		}
}
[Serializable]
public class MsgPacket{
		public MsgType _msgtype;
		public float _timeStamp;
		public int IdObj;
		public float _objectSize;
		public int _textureID;
		public  SerializableVector3 _objectPosition;
		public SeriaziableColor _textureColor;
		public float _objSpeed;
		public string RawData;
		public int _packagenum;
}
