using System.Text;
using UnityEngine;

public static class Utils {
  #region Method Extensions to UnityEngine.Object.
  public static bool Null(this Object obj) {
    return ReferenceEquals(obj, null);
  }

  public static bool FakeNull(this Object obj) {
    return ReferenceEquals(obj, null) && !obj;
  }

  public static bool Assigned(this Object obj) {
    return obj;
  }
  #endregion

  public static bool Null(object obj) {
    return ReferenceEquals(obj, null);
  }

  public class MyIO_ {
    [System.Diagnostics.Conditional("UNITY_EDITOR"),
    System.Diagnostics.Conditional("TRACE_ON")]
    public static void Log(string contents, object context) {
      //
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR"),
      System.Diagnostics.Conditional("TRACE_ON")]
    public static void Log(string contents) {
      //
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR"),
      System.Diagnostics.Conditional("TRACE_ON")]
    public static void LogVec(ref Vector3 vec) {
      Debug.Log(vec.ToString("F7"));
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR"),
      System.Diagnostics.Conditional("TRACE_ON")]
    public static void LogVec(ref Vector4 vec) {
      Debug.Log(vec.ToString("F7"));
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR"),
      System.Diagnostics.Conditional("TRACE_ON")]
    public static void LogMat(ref Matrix4x4 mat) {
      // https://docs.microsoft.com/ko-kr/dotnet/csharp/tuples
      (int rowLen, int colLen) dimension = (rowLen: 4, colLen: 4); // (named) Tuple-Projection Initializer.
      StringBuilder arrStrB = default; // default(System.Text.StringBuilder).

      for (int i = 0; i < dimension.rowLen; ++i) {
        for (int j = 0; j < dimension.colLen; ++j) {
          arrStrB.Append(string.Format("{0} {1} {2} {3}", mat[i, 0], mat[i, 1], mat[i, 2], mat[i, 3]));
        }
        arrStrB.Append(System.Environment.NewLine);
        Debug.Log(arrStrB.ToString());
      }
    }
  };
};
