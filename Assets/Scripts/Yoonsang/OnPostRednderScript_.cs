﻿#region Preprocessors.
// Basically the preprocessor <<UNITY_EDITOR>> is always defined on the Unity editor.
#define TRACE_ON
#endregion

#region Includes
using UnityEngine;
using System;
#endregion

public class OnPostRednderScript_ : MonoBehaviour
{
  /// <summary>
  /// 
  /// </summary>
  public GameObject mPyramidObj
  {
    get; set;
  }
  /// <summary>
  /// 
  /// </summary>
  Pyramid_ mPyramid;
  /// <summary>
  /// Hard reference to Camera.main.
  /// </summary>
  public Camera mMainCam
  {
    get; set;
  }

  void Awake()
  {
    if (mPyramidObj.FakeNull())
    {
      mPyramidObj = (!GameObject.Find("Prewarp/Pyramid").FakeNull() ? GameObject.Find("Prewarp/Pyramid") : default);
    }
    mPyramid = mPyramidObj.GetComponent<Pyramid_>();
    if (mPyramid.FakeNull())
    {
      Debug.LogError("Pyramid GameObject don't contains Script <<Pyramid_>>");
    }

    if (mPyramid.ThisUVMapRT.FakeNull())
      return;
    if (!mPyramid.ThisIntersectBuf.IsValid())
      return;
  }

  void OnDestroy()
  {
    if (mPyramid.ThisIntersectBuf != null)
    {
      mPyramid.ThisIntersectBuf.Dispose();
    }
  }

  void OnPostRender()
  {
    #region Early returns.

    /* <<Null comparision in Unity C# scripting>>
     * It requires to understand how actual <<null>> works since Unity C# scripting is quite different from a native C# scripting.
     * https://overworks.github.io/unity/2019/07/16/null-of-unity-object.html
     * https://overworks.github.io/unity/2019/07/22/null-of-unity-object-part-2.html 
     *   According to this article, UnityEngine.Object and System.Object are technically different type,
     *
     *   because <<UnityEngine.Object>> is the wrapper of the native instance written in C++ and it's disposed when the scene has changed or after calling <<object.Destroy()>>.
     *   however C# object(UnityEngine.Object) is still remaining until the GC finished collecting Garbages.
     *   and this is called <<Fake null status>>.
     *   Although There're operator overloadings of == and != inside the UnityEngine.Object, It doesn't cover this fake-null problem.
     *   Additionally, if you keep on comparing UnityEngine.Object with object, it leads to the boxing which performance requires.
     *   
     *   Therefore, we need to compare UnityEngine.Object by using <<object.ReferenceEquals(UnityEngine.Object, null)>> or <<(object)UnityEngine.Object == null >>.
    */
    if (mPyramid.ThisMat.FakeNull())
      return;

    if (mPyramid.ThisUVMapRT.FakeNull())
      return;

    if (!mPyramid.ThisIntersectBuf.IsValid())
      return;

    mPyramid.ThisIntersectBuf.GetData(mPyramid.mIntersectionBufDataArr);

    Vector4 pixel = default;
    string text = default;

    RenderTexture prev = RenderTexture.active;
    mPyramid.ThisTex2D.ReadPixels(new Rect(0, 0, mMainCam.pixelWidth, mMainCam.pixelHeight), 0, 0);

    #endregion
  }
};