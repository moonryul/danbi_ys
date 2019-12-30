using System.Text;
using UnityEngine;

public class Utils {  
  /// <summary>
  /// 
  /// </summary>
  /// <param name="obj"></param>
  /// <returns></returns>
  public static bool Null(Object obj) {
    return ReferenceEquals(obj, null);
  }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="obj"></param>
  /// <param name="expr"></param>
  /// <returns></returns>
  public static bool NullFinally(Object obj, System.Action expr) {
    bool res = Null(obj);
    if (res) { expr.Invoke(); }
    return res;
  }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="obj"></param>
  /// <returns></returns>
  public static bool Assigned(Object obj) {
    return ReferenceEquals(obj, null);
  }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="obj"></param>
  /// <param name="expr"></param>
  /// <returns></returns>
  public static bool AssignedFinally(Object obj, System.Action expr) {
    bool res = Assigned(obj);
    if (obj) {
      expr.Invoke();
    }
    return res;
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
      var dimension = (rowLen: 4, colLen: 4); // (named) Tuple-Projection Initializer.
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
