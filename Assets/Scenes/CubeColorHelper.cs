using UnityEngine;

public class CubeColorHelper : MonoBehaviour {
  Material mCurMat;
  Camera mMainCamRef;

  void Start() {
    mCurMat = GetComponent<MeshRenderer>().sharedMaterial;
    mMainCamRef = Camera.main;
    SetShaderParameter();
  }

  void SetShaderParameter() {
    if (ReferenceEquals(mCurMat, null)) {
      Debug.LogError("Material is invalid !", this);
    }

    mCurMat.SetInt("_ScreenDimensionX", mMainCamRef.pixelWidth);
    mCurMat.SetInt("_ScreenDimensionY", mMainCamRef.pixelHeight);
    //mCurMat.SetColor("_VertexCol")
  }
};
