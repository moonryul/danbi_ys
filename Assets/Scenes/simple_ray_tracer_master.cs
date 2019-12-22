using System;
using UnityEngine;
using ray_tracing_obj_list_t = System.Collections.Generic.List<ray_tracing_object>;
public class simple_ray_tracer_master : MonoBehaviour {
  public ComputeShader RayTracingShader;
  Camera _camera;
  public RenderTexture _target;
  public Texture _Skybox;
  uint _current_sample = 0;
  Material _add_material;

  static bool _mesh_object_need_rebuilding = false;
  static ray_tracing_obj_list_t _ray_tracing_objects = new ray_tracing_obj_list_t();
  public static void register(ray_tracing_object obj)
  {
    _ray_tracing_objects.Add(obj);
    _mesh_object_need_rebuilding = true;
  }

  public static void unregister(ray_tracing_object obj)
  {
    _ray_tracing_objects.Remove(obj);
    _mesh_object_need_rebuilding = true;
  }

  Material _shader;

  void Awake() {
    _camera = GetComponent<Camera>();
    //_shader = GetComponent<MeshRenderer>().material;
  }

  void OnRenderImage(RenderTexture source, RenderTexture destination) {
    SetShaderParameters();
    Render(destination);
  }

  void Update() {
    if (transform.hasChanged) {
      _current_sample = 0;
      transform.hasChanged = false;
    }
  }

  void Render(RenderTexture destination) {
    // Make sure we have a current render target
    InitRenderTexture();

    // Set the target and dispatch the compute shader
    RayTracingShader.SetTexture(0, "Result", _target);
     // Check the ratio of Screen.Width and Screen.Height is 16 by 9.
    //  if (1) {
    //   // ASSERT
    //  }

    //  if (1) {
    //    // ASSERt
    //  }

    int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
    int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
    RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

    // Blit the result texture to the screen
    if (ReferenceEquals(_add_material, null)) {
      _add_material = new Material(Shader.Find("Hidden/AddShader"));
      Debug.Log("add material is created!");
    }

    _add_material.SetFloat("_Sample", _current_sample);
    Graphics.Blit(_target, destination);
    if (_current_sample < 100) {
      ++_current_sample;
    }
    else {
      return;
    }
    //Debug.Log($"{_current_sample}");
  }

  void InitRenderTexture() {
    if (_target == null || _target.width != Screen.width || _target.height != Screen.height) {
      // Release render texture if we already have one
      if (_target != null) {
        _target.Release();
      }

      // Get a render target for Ray Tracing
      _target = new RenderTexture(Screen.width, Screen.height, 0,
          RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      _target.enableRandomWrite = true;
      _target.Create();
    }
  }

  void SetShaderParameters() {
    RayTracingShader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
    RayTracingShader.SetTexture(0, "_SkyboxTexture", _Skybox);
    RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
    RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);

    // _shader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
    // _shader.SetTexture("_SkyboxTexture", _Skybox);
    // _shader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
    // _shader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
  }

  void SetShaderParameters_at_runtime() {
    RayTracingShader.SetVector("_Time", new Vector4(Time.time * 100, Time.time * 1000, Time.time * 10000, Time.time * 20000));
  }
}
