using UnityEngine;

[System.Serializable]
public enum eCurrentVertexColor {
  RED, GREEN, BLUE, MAGENTA
};

public class RTcube : RTmeshObject {
  public eCurrentVertexColor mMyVertexColor;
  Mesh mMesh;

  public override void OnEnable() {
    mMesh = GetComponent<MeshFilter>().sharedMesh;
    SetVertexColor();
    base.OnEnable();
  }
  public override void OnDisable() { base.OnDisable(); }

  void SetVertexColor() {
    mMesh.colors = new Color[4];
    switch (mMyVertexColor) {
      case eCurrentVertexColor.RED:
      for (int i = 0; i < mMesh.colors.Length; ++i) {
        mMesh.colors[i] = Color.red;
      }
      break;

      case eCurrentVertexColor.GREEN:
      for (int i = 0; i < mMesh.colors.Length; ++i) {
        mMesh.colors[i] = Color.green;
      }
      break;

      case eCurrentVertexColor.BLUE:
      for (int i = 0; i < mMesh.colors.Length; ++i) {
        mMesh.colors[i] = Color.blue;
      }
      break;

      case eCurrentVertexColor.MAGENTA:
      for (int i = 0; i < mMesh.colors.Length; ++i) {
        mMesh.colors[i] = Color.magenta;
      }
      break;

      default:
      return;
    }
  }
}
