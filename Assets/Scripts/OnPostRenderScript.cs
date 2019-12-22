
#define UNITY_EDITOR
#define TRACE_ON

using System;
using UnityEngine;



//In Java as well as in c# there is no separate method declaration.    
//The declaration of the method is done with its implementation.You also do not need to keep track of 
//    file includes    so that the classes know about eachother as long as they are in the same namespace.

//RequireComponent automatically adds the required component to the 
// the gameObject to which the Script component will be added.

// Debug Shaders:
// https://forum.unity.com/threads/how-to-print-shaders-var-please.26052/
// https://docs.unity3d.com/ScriptReference/Material.GetMatrix.html

// GameObjects and Components: Only Components(MonoBehaviours) need to be attached to GameObjects, and if fact only they CAN be.
//The great majority of scripts in games are not Components, but data objects and utility classes for performing operations on data objects, 
//    like with any other program.You can create a normal non-MonoBehaviour script by just right clicking in the Project files tab and saying 
//    Create C# Script, naming the script, opening it in an IDE (double clicking it works), and deleting the part where it derives from MonoBehaviour.
//    If it doesn't derive from MonoBehaviour, then it's not a component, and you don't need to / can't attach it to GameObject. 



//[RequireComponent (typeof (MeshCollider))]
//[RequireComponent (typeof (MeshFilter))]
//[RequireComponent (typeof (MeshRenderer))]

//This breaks the whole idea that your script should be a reusable component with only a single responsibility.
//https://akbiggs.silvrback.com/please-stop-using-gameobject-find

public class OnPostRenderScript : MonoBehaviour
{
  //    For the most part, scripting is also about modifying Component properties to manipulate GameObjects
  //.The difference, though, is that a script can vary a property value gradually over time or in response to input from the user. 
  //        By changing, creating and destroying objects at the right time, any kind of gameplay can be implemented.

  //   You can now drag an object from the scene or Hierarchy panel onto this variable to assign it.
  //  The GetComponent function and Component access variables are available for this object

  // Field variales are set to the default values; but not local variables of methods

  public GameObject mPyramidObj;
  Pyramid mPyramid;

  //This is Main Camera in the Scene
  public Camera mMainCamera; // Camera is a behavior component
                             //Renderer mPyramidRenderer;
                             //Material mPyramidMaterial;

  //ComputeBuffer mIntersectionBuffer;
  //RenderTexture mUVMapRenderTexture;

  //Texture2D mTexture2D;

  // ComputeBuffer(int count, int stride, ComputeBufferType type);

  //struct IntersectionBufferStruct {
  //    public Vector4 mPosInObj;
  //    public Vector4 mPosInClip;
  //    public Vector4 mPosInCam;
  //    public Vector4 mNormalInObj;
  //    public Vector4 mNormalInCam;
  //    public Vector4 mInterPos;       
  //};

  //IntersectionBufferStruct[] mIntersectionBufferData;

  // OnPostRender message is sent to scripts attached to the camera game object. Pyramid script is attached to the camera
  void OnPostRender()
  {

    #region Early returns.
    // check if the material is already constructed in pyramid.cs
    if (ReferenceEquals(mPyramid.mPyramidMaterial, null))
    {
      // throw new Exception("mPyramid.mPyramidMaterial is not set in Pyramid script; Stop the process");
      return; // wait for mPyramid.mPyramidMaterial  to be set by Pymamid Script
    }
    if (ReferenceEquals(mPyramid.mUVMapRenderTexture, null))
    {
      // throw new Exception("mUVMapRenderTexture is not set in Pyramid script; Stop the process");
      return; // wait for mUVMapRenderTexture  to be set by Pymamid Script
    }
    //mIntersectionBuffer = mPyramid.mIntersectionBuffer;
    if (ReferenceEquals(mPyramid.mIntersectionBuffer, null))
    {
      //throw new Exception(" mIntersectionBuffer is not set in Pyramid script; Stop the process");
      return; // wait formIntersectionBuffer to be set by Pymamid Script
    }
    #endregion

    mPyramid.mIntersectionBuffer.GetData(mPyramid.mIntersectionBufferData);

    //  float[] v = new float[Camera.main.pixelWidth * Camera.main.pixelHeight * 3];
    Vector4 pixel = new Vector4();
    string text = "";

    Debug.Log("\n\nIntersection Debug");
    Debug.Log("Intersection Debug");

    for (int i = 0; i < mPyramid.mIntersectionBufferData.Length; i++)
    {
      text = "vertex index =" + i + "(" + i / 3 + ")" + ":";
      Debug.Log(text);

      pixel = mPyramid.mIntersectionBufferData[i].mPosInObj;

      //Debug.Log("Pos In Obj:");
      //MyIO.DebugLogVector(pixel);

      pixel = mPyramid.mIntersectionBufferData[i].mPosInClip;

      //Debug.Log("Pos In Clip:");
      MyIO.DebugLogVector(pixel);


      pixel = mPyramid.mIntersectionBufferData[i].mPosInCam;


      //Debug.Log("Pos In Camera:");
      //MyIO.DebugLogVector(pixel);


      pixel = mPyramid.mIntersectionBufferData[i].mNormalInObj;

      //Debug.Log("Normal In Obj:");
      //MyIO.DebugLogVector(pixel);


      pixel = mPyramid.mIntersectionBufferData[i].mNormalInCam;

      //Debug.Log("Normal In Cam:");
      //MyIO.DebugLogVector(pixel);



      pixel = mPyramid.mIntersectionBufferData[i].mInterPos;

      //Debug.Log("intersection point:");
      //MyIO.DebugLogVector(pixel);
    }

    //Debug.Log("UVMap  Debug");
    //Debug.Log("UVMap Debug");


    RenderTexture previous = RenderTexture.active; // The original active RenderTexture is the framebuffer

    // Set the current RenderTexture to the temporary one we created,
    //RenderTexture.active = tmp;

    RenderTexture.active = mPyramid.mUVMapRenderTexture;
    // set the active render texture to  mUVMapRenderTexture
    // Copy the pixels from the active RenderTexture to the new Texture
    mPyramid.mTexture2D.ReadPixels(new Rect(0, 0, mMainCamera.pixelWidth, mMainCamera.pixelHeight), 0, 0);
    // ReadPixels(Rect source, int destX, int destY);

    //mTexture2D.Apply(); // apply all the previous setPixel and setPixels





  } // OnPostRender()

  //garbage collector extra 
  //OnDestroy occurs when a Scene or game ends.Stopping the Play mode when running from inside the Editor will end the application.
  //    As this end happens an OnDestroy will be executed.Also, if a Scene is closed and a new Scene is loaded the OnDestroy call 
  //    will be made.   When built as a standalone application OnDestroy calls are made when Scenes end.
  //    A Scene ending typically means a new Scene is loaded.

  void OnDestroy()
  {
    if (mPyramid.mIntersectionBuffer != null)
    {
      mPyramid.mIntersectionBuffer.Dispose();
    }
  }


  // SingleTon class
  //https://www.gamasutra.com/blogs/JohnWarner/20130910/194559/The_top_5_things_Ive_learned_as_a_Unity_developer.php

  //https://stackoverflow.com/questions/35160797/is-it-possible-to-create-a-global-class-instance-in-unity
  //https://debuglog.tistory.com/35

  void Awake()
  {
    Debug.Log("Intersection Debug Init");

    // mWriter = new StreamWriter(mPath, true);
    // mReader = new StreamReader(mPath, true);


    // check if mPyradmidObj is bound to its object. If not, you can find it using 
    //GameObject.Find("name"); or    GameObject.FindWithTag("..."). These gameobjects should have been active to be found.
    //GameObject.Find is the slowest because Unity must go through all GameObject in the scene.
    //The deal is, you don't exactly use find like

    //GameOjbect.Find("SomeWeirdGameObject");
    //You use it like GameObject.Find("Path/path/path/someObject");

    //https://akbiggs.silvrback.com/please-stop-using-gameobject-find
    //GameObject.FindWithTag is better because Unity only needs to go through GameObjects with that tag.
    // GetComponent<T>() is much better because Unity already has a GameObject to deal with and doesn't need to check the entire scene.
    //Tags must be declared in the tag manager before using them.
    //    A UnityException will be thrown if the tag does not exist or an empty string or null is passed as the tag.
    // But this finding approach is not recommended, because the dependencies are hidden from programmers and users.
    //Instead of using object names that are hidden away in our script to resolve our dependencies, let's show our dependencies in the inspector instead. 
    //This way, we know exactly what a script needs when we attach it. To do this, let's make our fields public.
    //https://akbiggs.silvrback.com/please-stop-using-gameobject-find
    //https://www.reddit.com/r/gamedev/comments/3t74w4/unity_devs_should_stop_using_gameobjectfind/

    //"Unity devs should stop using all string-based methods."

    //Almost every string-based part of Unity is both error prone and inefficient.

    //Edit: That said there is a time and place for the find methods, 
    //    and I don't know that spaghetti wiring prefabs and references is much better.


    if (ReferenceEquals(mPyramidObj, null))
    {
      Debug.Log("Pyramid Game objectt  is not set in the inspector; We will try to find it");

      mPyramidObj = GameObject.Find("Prewarp/Pyramid");

      // //https://answers.unity.com/questions/8500/how-can-i-get-the-full-path-to-a-gameobject.html
      //GameObject.Find("/wall/door2/frame")

      if (mPyramidObj is null)
      {
        //Debug.LogError("GameObject Preward/Pyramid does not exist");
        //When you run your code in the Unity editor, the editor catches the thrown errors and does a Debug.LogError() instead of crashing Unity or the app.
        throw new Exception("GameObject Preward/Pyramid does not exist");

      }

    }



    mPyramid = mPyramidObj.GetComponent<Pyramid>();

    if (ReferenceEquals(mPyramid, null)) // Is material null?
    {
      throw new Exception("script mPyramid is not attached to the Pyramid gameobject; Stop the process");


    }


    //mUVMapRenderTexture = mPyramid.mUVMapRenderTexture;

    if (ReferenceEquals(mPyramid.mUVMapRenderTexture, null)) // Is material null?
    {
      // throw new Exception("mUVMapRenderTexture is not set in Pyramid script; Stop the process");
      return; // wait for mUVMapRenderTexture  to be set by Pymamid Script


    }

    //mIntersectionBuffer = mPyramid.mIntersectionBuffer;


    if (ReferenceEquals(mPyramid.mIntersectionBuffer, null)) // Is material null?
    {
      //throw new Exception(" mIntersectionBuffer is not set in Pyramid script; Stop the process");
      return; // wait formIntersectionBuffer to be set by Pymamid Script


    }




  } // Start()

  // Update is called once per frame
  void Update()
  {

  }

}  // class OnPostRenderScript
