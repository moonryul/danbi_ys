using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ray_tracing_object : MonoBehaviour
{
    void OnEnable() {
      simple_ray_tracer_master.register(this);    
    }

    void OnDisable() {
        simple_ray_tracer_master.unregister(this);
    }
}
