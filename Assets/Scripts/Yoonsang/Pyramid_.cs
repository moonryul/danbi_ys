#define TRACE_ON

using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public class Pyramid_ : MonoBehaviour {
  /// <summary>
  /// 
  /// </summary>
  readonly string mResultPath = "Assets/Resources/debug.txt";

  /// <summary>
  /// 
  /// </summary>
  StreamReader mStreamReader;

  /// <summary>
  /// 
  /// </summary>
  StreamWriter mStreamWriter;

  /// <summary>
  /// Hard ref to Camera.main.
  /// </summary>
  [SerializeField] Camera mMainCam;
  public Camera MainCam {
    get {
      if (!mMainCam.Assigned()) {
        mMainCam = Camera.main;
      }
      return mMainCam;
    }
  }

  /// <summary>
  /// Hard ref to the renderer of Pyramid.
  /// </summary>
  [SerializeField] Renderer mThisRenderer;
  public Renderer ThisRenderer {
    get {
      if (!mThisRenderer.Assigned()) {
        Debug.LogError("mPyramidRenderer hasn't been assigned yet!", this);
      }

      return mThisRenderer;
    }
  }

  /// <summary>
  /// Material of this (Pyramid).
  /// </summary>
  [SerializeField] Material mThisMat;
  public Material ThisMat {
    get {
      if (!mThisMat.Assigned()) {
        Debug.LogError("ThisMat hasn't been assigned yet!", this);
      }
      return mThisMat;
    }
  }

  /// <summary>
  /// Mesh of Pyramid.
  /// </summary>
  Mesh mMesh;

  /// <summary>
  /// Hard ref to Cylinder.
  /// </summary>
  [SerializeField] Cylinder_ mCylinder;
  public Cylinder_ Cylinder {
    get {
      if (!mCylinder.Assigned()) {
        Debug.LogError("Cylinder hasn't been assigned yet!", this);
      }

      return mCylinder;
    }
  }

  [SerializeField] ComputeBuffer mIntersectBuf;
  public ComputeBuffer ThisIntersectBuf {
    get {
      if (ReferenceEquals(mIntersectBuf, null)) {
        Debug.LogError("Intersection Buffer didn't created");
      }
      return mIntersectBuf;
    }
  }

  [SerializeField] RenderTexture mUVMapRT;
  public RenderTexture ThisUVMapRT {
    get {
      if (mUVMapRT.FakeNull()) {
        Debug.LogError("UVMap RenderTexture didn't created");
      }
      return mUVMapRT;
    }
  }

  [SerializeField] Texture2D mTex2D;
  public Texture2D ThisTex2D {
    get {
      if (mTex2D.FakeNull()) {
        Debug.LogError("Final texture didn't created");
      }
      return mTex2D;
    }
  }


  public struct IntersectionBufInfo {
    public Vector4 PosInObjectSpace;
    public Vector4 PosInClipSpace;
    public Vector4 POsInCameraSpace;
    public Vector4 NormalInObjectSpace;
    public Vector4 NormalInCameraSpace;
    public Vector4 IntersectedPos;
  }

  public IntersectionBufInfo[] mIntersectionBufDataArr;

  [System.Serializable]
  public struct PyramidParam {
    public float Height;
    public float Width;
    public float Depth;
  };
  [Header("Pyramid Parameters"), Space(20)]
  public PyramidParam mPyramidParam = new PyramidParam {
    Height = 0.1f,
    Width = 0.2f,
    Depth = 0.1f
  };

  void OnValidate() {
    Rebuild();
  }

  void Awake() {
    mStreamWriter = new StreamWriter(mResultPath, true);
    MainCam.enabled = true;
    mThisRenderer = GetComponent<MeshRenderer>();
    mThisMat = mThisRenderer.material;

    mThisMat.SetFloat("_CylinderHeight", mCylinder.mCylinderParam.height);
    mThisMat.SetFloat("_CylinderRadius", mCylinder.mCylinderParam.radius);

    Matrix4x4 cylinderLocalToWorldMat = mCylinder.transform.localToWorldMatrix;
    Matrix4x4 fwd = mMainCam.worldToCameraMatrix;
    Vector4 cylinderOriginInCameraSpace = mMainCam.worldToCameraMatrix * cylinderLocalToWorldMat * new Vector4(0, 0, 0, 1.0f);
    mThisMat.SetVector("_CylinderOriginInCamera", cylinderOriginInCameraSpace);

    // ComputeBufferType.Default -> it maps to StructuredBuffer<T> or RWStructuredBuffer<T>.
    mIntersectBuf = new ComputeBuffer(mMesh.triangles.Length, Marshal.SizeOf(typeof(IntersectionBufInfo)), ComputeBufferType.Default);
    // Set the ComputeBuffer for shader debugging.
    // but a RWStructuredBuffer, requires SetRandomWriteTarget to work at all in a non-compute-shader which is magically working internally inside the Unity API.
    mIntersectionBufDataArr = new IntersectionBufInfo[mMesh.triangles.Length];
    Graphics.ClearRandomWriteTargets();
    // index 1 -> register(u1).
    // Uses " Unordered Access Views (UAV)" in Using DX11GL3Features.
    // These "Random write" targets are set similarly to how multiple render targets are set.
    // Initial value is filling buffer with red pixels.
    Graphics.SetRandomWriteTarget(1, mIntersectBuf);
    mThisMat.SetBuffer("_IntersectionBuffer", mIntersectBuf);

    // Create an render texture to store the result of pixel shader.
    mUVMapRT = new RenderTexture(mMainCam.pixelWidth, mMainCam.pixelHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
    mUVMapRT.enableRandomWrite = true;
    mUVMapRT.Create();
    mTex2D = new Texture2D(mMainCam.pixelWidth, mMainCam.pixelHeight);
    Graphics.SetRandomWriteTarget(2, mUVMapRT);
    mThisMat.SetTexture("_UVMapBuffer", mUVMapRT);
  }

  private void OnDestroy() {
    if (!Utils.Null(mIntersectBuf)) {
      mIntersectBuf.Dispose();
      mIntersectBuf = null;
    }
  }

  Vector3[] CalcNormals() {
    Vector3[] normals = new Vector3[mMesh.vertices.Length];
    int len = mMesh.triangles.Length;
    for (int i = 0; i < len; ++i) {
      if (( i + 1 ) % 3 == 0) {
        int i0 = mMesh.triangles[i - 2];
        int i1 = mMesh.triangles[i - 1];
        int i2 = mMesh.triangles[i];

        Vector3 ab = mMesh.vertices[i1] - mMesh.vertices[i0];
        Vector3 bc = mMesh.vertices[i2] - mMesh.vertices[i1];
        Vector3 newNormal = Vector3.Cross(ab, bc).normalized;
        normals[i - 2] = normals[i - 1] = normals[i] = newNormal;
      }
    }
    return normals;
  }

  public void Rebuild() {
    MeshFilter mf = GetComponent<MeshFilter>();
    if (mf.FakeNull()) {
      Debug.LogError("MeshFilter failed to assign", this);
      return;
    }
    mMesh = mf.sharedMesh;

    if (mMesh.FakeNull()) {
      mf.sharedMesh = new Mesh();
      mMesh = mf.sharedMesh;
    }
    mMesh.Clear();

    Vector3 p0 = new Vector3(-0.5f * mPyramidParam.Width, 0.0f, -0.5f * mPyramidParam.Depth); // left/rear corner of the base
    Vector3 p1 = new Vector3(0.5f * mPyramidParam.Width, 0.0f, -0.5f * mPyramidParam.Depth);  // right/rear corner
    Vector3 p2 = new Vector3(0.5f * mPyramidParam.Width, 0.0f, 0.5f * mPyramidParam.Depth);   // right/front corner
    Vector3 p3 = new Vector3(-0.5f * mPyramidParam.Width, 0.0f, 0.5f * mPyramidParam.Depth);  // left/front corner
    Vector3 p4 = new Vector3(0.0f, mPyramidParam.Height, 0.0f);                               // apex

    mMesh.vertices = new Vector3[] {
      p0, p1, p2,
      p0, p2, p3,
      p0, p1, p4,
      p1, p2, p4,
      p2, p3, p4,
      p3, p0, p4
    };

    mMesh.triangles = new int[] {
      0, 1, 2,
      3, 4, 5,
      8, 7, 6,
      11, 10, 9,
      14, 13, 12,
      17, 16, 15
    };

    mMesh.RecalculateNormals();
    Vector3[] newNormals = CalcNormals();

    Debug.Log("mesh.RecalNormals() versus myCalNormals():");
    for (int i = 0; i < mMesh.vertices.Length; i++) {
      Debug.Log("vertex index =" + i + ":");

      Debug.Log("vertex = ");
      Utils.MyIO_.LogVec(ref mMesh.vertices[mMesh.triangles[i]]);

      Debug.Log("normal = ");
      Utils.MyIO_.LogVec(ref mMesh.normals[i]);
      //Utils.MyIO_.LogVec(myNormals[i]);
    }

    mMesh.RecalculateBounds();
  }


};
