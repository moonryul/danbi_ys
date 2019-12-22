using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter), typeof(MeshRenderer))]
public class Cylinder_ : MonoBehaviour {
  [System.Serializable]
  public struct CylinderParam {
    public float height;
    public float radius;
    public int nbSides;
    public int nbHeightSeg;
  };

  int nbVerticesCap;
  float bottomRadius;
  float topRadius;

  [Header("Cylinder Paramteres"), Space(20)]
  public CylinderParam mCylinderParam = new CylinderParam() {
    height = 0.1f,
    radius = 0.25f,
    nbSides = 18,
    nbHeightSeg = 1
  };

  void OnValidate() {
    Rebuild(); 
  }

  void Rebuild() {
    MeshFilter mf = GetComponent<MeshFilter>();
    if (mf.FakeNull()) {
      Debug.LogError("MeshFilter not found!");
      return;
    }

    Mesh mesh = mf.sharedMesh;
    if (mesh.FakeNull()) {
      mf.mesh = new Mesh();
      mesh = mf.sharedMesh;
    }
    mesh.Clear();

    #region Vertices
    nbVerticesCap = mCylinderParam.nbSides + 1;
    topRadius = bottomRadius = mCylinderParam.radius;

    // bottom + top + sides.
    Vector3[] vertices = new Vector3[
      nbVerticesCap + nbVerticesCap + mCylinderParam.nbSides * mCylinderParam.nbHeightSeg * 2 + 2];
    int vert = 0;
    const float _2pi = Mathf.PI * 2.0f;
    // Top cap.
    vertices[vert++] = Vector3.zero;
    while (vert <= mCylinderParam.nbSides) {
      float r = (float)vert / mCylinderParam.nbSides * _2pi;
      vertices[vert] = new Vector3(Mathf.Cos(r) * bottomRadius, 0.0f, Mathf.Sin(r) * bottomRadius);
      ++vert;
    }
    // Sides.
    int v = 0;
    while (vert <= vertices.Length - 4) {
      float r = (float)v / mCylinderParam.nbSides * _2pi;
      vertices[vert] = new Vector3(Mathf.Cos(r) * topRadius, mCylinderParam.height, Mathf.Sin(r) * topRadius);
      vertices[vert + 1] = new Vector3(Mathf.Cos(r) * bottomRadius, 0.0f, Mathf.Sin(r) * bottomRadius);
      vert += 2;
      ++v;
    }
    vertices[vert] = vertices[mCylinderParam.nbSides * 2 + 2];
    vertices[vert + 1] = vertices[mCylinderParam.nbSides * 2 + 3];
    #endregion

    #region Normalise
    Vector3[] normals = new Vector3[vertices.Length];
    vert = 0;

    // Bottom Cap
    while (vert <= mCylinderParam.nbSides) {
      normals[vert++] = Vector3.down;
    }
    // Top Cap
    while (vert <= mCylinderParam.nbSides * 2 + 1) {
      normals[vert++] = Vector3.up;
    }
    // Sides
    v = 0;
    while (vert <= vertices.Length - 4) {
      float r = (float)v / mCylinderParam.nbSides * _2pi;
      normals[vert] = new Vector3(Mathf.Cos(r), 0.0f, Mathf.Sin(r));
      normals[vert + 1] = normals[vert];
      vert += 2;
      ++v;
    }
    normals[vert] = normals[mCylinderParam.nbSides * 2 + 2];
    normals[vert + 1] = normals[mCylinderParam.nbSides * 2 + 3];
    #endregion

    #region UVs
    Vector2[] uvs = new Vector2[vertices.Length];
    // Bottom Cap
    int u = 0;
    uvs[u++] = new Vector2(0.5f, 0.5f);
    while (u <= mCylinderParam.nbSides) {
      float r = (float)u / mCylinderParam.nbSides * _2pi;
      uvs[u] = new Vector2(Mathf.Cos(r) * 0.5f + 0.5f, Mathf.Sin(r) * 0.5f + 0.5f);
      ++u;
    }
    // Top Cap
    uvs[u++] = new Vector2(0.5f, 0.5f);
    while (u <= mCylinderParam.nbSides * 2 + 1) {
      float r = (float)u / mCylinderParam.nbSides * _2pi;
      uvs[u] = new Vector2(Mathf.Cos(r) * 0.5f + 0.5f, Mathf.Sin(r) * 0.5f + 0.5f);
      ++u;
    }
    // Sides
    int uSides = 0;
    while (u <= uvs.Length - 4) {
      float t = (float)uSides / mCylinderParam.nbSides;
      uvs[u] = new Vector2(t, 1.0f);
      uvs[u + 1] = new Vector2(t, 0.0f);
      u += 2;
      ++uSides;
    }
    uvs[u] = new Vector2(1.0f, 1.0f);
    uvs[u + 1] = new Vector2(1.0f, 0.0f);
    #endregion

    #region Triangles
    int nbTriangles = mCylinderParam.nbSides + mCylinderParam.nbSides + mCylinderParam.nbSides * 2;
    int[] triangles = new int[nbTriangles * 3 + 3];
    // Bottom Cap
    int tri = 0, i = 0;
    while (tri < mCylinderParam.nbSides - 1) {
      triangles[i] = 0;
      triangles[i + 1] = tri + 1;
      triangles[i + 2] = tri + 2;
      ++tri;
      i += 3;
    }
    triangles[i] = 0;
    triangles[i + 1] = tri + 1;
    triangles[i + 2] = 1;
    i += 3;
    // Top Cap
    while (tri < mCylinderParam.nbSides * 2) {
      triangles[i] = tri + 2;
      triangles[i + 1] = tri + 1;
      triangles[i + 2] = nbVerticesCap;
      ++tri;
      i += 3;
    }
    triangles[i] = nbVerticesCap + 1;
    triangles[i + 1] = tri + 1;
    triangles[i + 2] = nbVerticesCap;
    ++tri;
    i += 3;
    ++tri;
    // Sides.
    while (tri <= nbTriangles) {
      triangles[i] = tri + 2;
      triangles[i + 1] = tri + 1;
      triangles[i + 2] = tri;
      ++tri;
      i += 3;
      triangles[i] = tri + 1;
      triangles[i + 1] = tri + 2;
      triangles[i + 2] = tri;
      ++tri;
      i += 3;
    }
    #endregion

    mesh.vertices = vertices;
    mesh.normals = normals;
    mesh.uv = uvs;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();
  }
};
