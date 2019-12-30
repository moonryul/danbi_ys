using System.Diagnostics;
using UnityEngine;

public class NullTester : MonoBehaviour {
  Transform tf;  

  void Start() {
    var sw = new Stopwatch();
    tf = transform;
    //if (tf.Null()) {
    //  Debug.Log($"tf is real null");
    //} else {
    //  Debug.Log($"tf isn't real null");
    //}

    //if (tf.FakeNull()) {
    //  Debug.Log($"tf is fake null");
    //} else {
    //  Debug.Log($"tf isn't fake null");
    //}

    //if (tf.Assigned()) {
    //  Debug.Log($"tf is assigned");
    //} else {
    //  Debug.Log($"tf isn't assigned");
    //}
    bool res1 = false, res2 = false, res3 = false;

    sw.Start();
    for (int i = 0; i < 10000; ++i) {
      res1 = ReferenceEquals(tf, null);
    }

    sw.Stop();
    UnityEngine.Debug.Log($"Ref Equals -> {sw.Elapsed}");

    sw.Start();
    for (int i = 0; i < 10000; ++i) {
      res2 = !tf;
    }

    sw.Stop();
    UnityEngine.Debug.Log($"bool operator -> {sw.Elapsed}");

    sw.Start();
    for (int i = 0; i < 10000; ++i) {
      res3 = tf == null;
    }

    sw.Stop();
    UnityEngine.Debug.Log($"null operator -> {sw.Elapsed}");
  }
}
