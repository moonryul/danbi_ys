using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ray_tracing_object : MonoBehaviour {
  virtual public void OnEnable() {
    simple_ray_tracer_master.register(this);
  }

  virtual public void OnDisable() {
    simple_ray_tracer_master.unregister(this);
  }
}
