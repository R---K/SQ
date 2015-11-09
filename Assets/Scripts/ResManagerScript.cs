using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public class ResManagerScript : MonoBehaviour {
	
	public static ResManagerScript instance;
	public static bool ImBusy;
	
	// Bundles
	public string ResPath = "BundleRes/02Bundle.unity3d";
    public int version;
	public GameObject CircleGameObjPref;
	public Texture2D [] BundleResourceTextures2d = new Texture2D[4];
	public string [] BundleTexturesNames = {"32","64","128","256"};
	AssetBundle assetBundle;
	

	public Texture2D [] WorkTextures2d = new Texture2D[8];
	public Texture2D [] WorkCacheTextures2d = new Texture2D[8];
	static List<Texture2D> Texture2DPointerList = new List<Texture2D>();
	
	void Awake(){
			instance = this;
			ImBusy = false;
	}
	


    public IEnumerator LoadBundle(){
			ImBusy = true;
        	string url = "file:///"+Path.GetFullPath(ResPath);
			MessageServiceClass.MessageProcessing("Loadin bundle from "+url);
			WWW www = WWW.LoadFromCacheOrDownload(url, version);
		    yield return www;
			MessageServiceClass.MessageProcessing("Bundle resources are loaded from "+ url);
			
			assetBundle = www.assetBundle;
            CircleGameObjPref = assetBundle.Load("circle") as GameObject;
			for (int i = 0;i <4; i++){
				BundleResourceTextures2d[i] = assetBundle.Load (BundleTexturesNames[i]) as Texture2D;
		
			}// int i = 0;i <4; i++
		
           	assetBundle.Unload(false);
			ImBusy = false;
        }
 
    void Start(){
			StartCoroutine(LoadBundle());
			
			
			
		
    }

	void Update (){
		if (Input.GetKeyDown("t")){
				MessageServiceClass.MessageProcessing ("Deleting texture");
				Destroy( WorkTextures2d[0]);
		}
	}

	/// <summary>
	/// Materials the create. Create specifical 4 Material of specified _color
	/// </summary>
	/// <param name='_color'>
	/// _color. 
	/// _local - local textures,!_local other playerTextures
	/// </param>
	/// 
	public IEnumerator MaterialCreate(Color _color,bool _local){
			float _timer;
			if (ImBusy){
				MessageServiceClass.MessageProcessing("Resource manager is busy");
			}
			while (ImBusy)
				yield return new WaitForSeconds(.1f);
			ImBusy = true;
			// preparing textures
			// from 1 to 4
			int TextureIndexShift; // 0 - local, 4- other player
			if(_local)
				TextureIndexShift = 0;
			  else
				TextureIndexShift = 4;
			_timer = Time.time;
		
			//create
			for (int i = 0; i < 4;i++){
			if (_local)
				WorkCacheTextures2d[i] = new Texture2D(BundleResourceTextures2d[i].width,BundleResourceTextures2d[i].height);
			
			else
				WorkCacheTextures2d[i+4] = new Texture2D(BundleResourceTextures2d[i].width,BundleResourceTextures2d[i].height);
			}
				
			MessageServiceClass.MessageProcessing("Creating new texture pack");
			for (int i = 0; i < 4;i++){
			
				
				
				// fills texture specified color & sets resolution stamp 
				for (int y = 0; y < WorkCacheTextures2d[i].height;y++)
					{
					for(int x = 0; x < WorkCacheTextures2d[i].width;x++)
						{
								// construct every pixel in new texture
								// creating gradient
								float _colorgradient = (float)y/(float)WorkCacheTextures2d[i].height*.6f;
								Color _targetColor = new Color(_colorgradient,_colorgradient,_colorgradient,1)+_color;
								
								Color _colorPixel = BundleResourceTextures2d[i].GetPixel(x,y);
				
								if( _colorPixel.g < .4f && _colorPixel.b < .4f) // is far from white color
									{
										WorkCacheTextures2d[i+TextureIndexShift].SetPixel(x,y,_colorPixel);
									}
								  else
									{
										WorkCacheTextures2d[i+TextureIndexShift].SetPixel(x,y,_targetColor);
									}
					
								// here we need to return
						}
					
					if (Time.time - _timer > .03f)// exit point
						{
							_timer = Time.time;
							yield return new WaitForSeconds(.02f);
							
						}
					}
				WorkCacheTextures2d[i+TextureIndexShift].Apply();
			}
			ImBusy = false;
			if (_local)
				MessageServiceClass.MessageProcessing("Texture pack was created");
			  else
				MessageServiceClass.MessageProcessing("Other player Texture pack was created");
			
	}
	/// <summary>
	/// Textures the swap. Here textures from cache sets to main work textures
	/// _local - local textures,!_local other playerTextures
	/// </summary>
	public void  TextureSwap(bool _local){

		int TextureIndexShift; // 0 - local, 4- other player
			if(_local)
				TextureIndexShift = 0;
			  else
				TextureIndexShift = 4;
			for(int i = TextureIndexShift;i<4+TextureIndexShift;i++){
				WorkTextures2d[i] = WorkCacheTextures2d[i];
				// add current texture to texture list
				Texture2DPointerList.Add(WorkTextures2d[i]);
				// if count of list textures > 24 lets clear first texure - 
				if (Texture2DPointerList.Count > 24)
					{
						Destroy(Texture2DPointerList[0]);
						Texture2DPointerList.RemoveAt(0);
					}
			}
			
			
	}// textureswap
}



