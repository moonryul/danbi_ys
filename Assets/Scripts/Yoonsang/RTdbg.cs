using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public struct RTdbg {
  public UnityEngine.Vector4[] RetrivedColBuf;

  public static void Log(StringBuilder format, UnityEngine.Object obj) {
    Debug.Log(format, obj);
  }

};
