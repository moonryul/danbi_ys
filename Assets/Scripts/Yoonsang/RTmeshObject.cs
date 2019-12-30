using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class RTmeshObject : MonoBehaviour {
  virtual public void OnEnable() {
    RTmaster.register(this);
  }

  virtual public void OnDisable() {
    RTmaster.unregister(this);
  }
}
