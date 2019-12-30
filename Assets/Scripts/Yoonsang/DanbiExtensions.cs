using UnityEngine;
public static class DanbiExtensions {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Null(this Object obj) {
      return ReferenceEquals(obj, null);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="expr"></param>
    /// <returns></returns>
    public static bool NullFinally(this Object obj, System.Action expr) {
      bool res = Null(obj);
      if (res) {
        expr.Invoke();
      }
      return res;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Assigned(this Object obj) {
      return ReferenceEquals(obj, null);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="expr"></param>
    /// <returns></returns>
    public static bool AssignedFinally(this Object obj, System.Action expr) {
      bool res = Assigned(obj);
      if (res) {
        expr.Invoke();
      }
      return res;
    }
  };