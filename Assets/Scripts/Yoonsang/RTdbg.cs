using UnityEngine;

public class RTdbg {
  public Vector3[] RetrivedColBuf;
  public System.Diagnostics.Stopwatch DbgStopwatch;

  public RTdbg() {
    DbgStopwatch = new System.Diagnostics.Stopwatch();
  }

  public static void Log(string str, UnityEngine.Object indicated = default) {
    UnityEngine.Debug.Log(str, indicated);
  }

  public static void Log(string fmt, UnityEngine.Object indicated, params object[] args) {
    UnityEngine.Debug.LogFormat(indicated, fmt, args);
  }

  public static void LogW(string str, UnityEngine.Object indicated = default) {
    UnityEngine.Debug.LogWarning(str, indicated);
  }

  public static void LogW(string fmt, UnityEngine.Object indicated, params object[] args) {
    UnityEngine.Debug.LogWarningFormat(indicated, fmt, args);
  }

  public static void LogE(string str, UnityEngine.Object indicated = default) {
    UnityEngine.Debug.LogError(str, indicated);
  }

  public static void LogE(string str, UnityEngine.Object indicated, params object[] args) {
    UnityEngine.Debug.LogErrorFormat(indicated, str, args);
  }
};
