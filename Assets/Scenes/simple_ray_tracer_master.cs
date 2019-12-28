using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ray_tracing_obj_list_t = System.Collections.Generic.List<ray_tracing_object>;
using Sphere_list_t = System.Collections.Generic.List<ray_tracing_sphere>;

public class simple_ray_tracer_master : MonoBehaviour {
  public ComputeShader RayTracingShader;
  Camera _camera;
  public RenderTexture _target;
  public Texture _Skybox;
  public Material _Video;
  uint _current_sample = 0;
  Material _add_material;

  static bool _mesh_object_need_rebuilding = false;
  static ray_tracing_obj_list_t _ray_tracing_objects = new ray_tracing_obj_list_t();

  public Light _directional_light;

  Material _shader;
  public Vector2 _offset_Radius = new Vector2(3.0f, 8.0f);
  public static readonly uint _spheres_max = 20;
  public float _sphere_placement_radius = 100.0f;
  public ComputeBuffer _sphere_buffer;

  internal struct ray_tracing_mesh_object { // total byte offset : 4 * 16 + 4 + 4 = 72.
    public Matrix4x4 local_to_world_matrix; // sizeof(float) * 16
    public int indices_offset; // 4
    public int indices_count; // 4
    public int use_vertex_color; // 4
  };

  static List<ray_tracing_mesh_object> _mesh_objects = new List<ray_tracing_mesh_object>();
  static List<Vector3> _vertices = new List<Vector3>(); // byte offset = 
  static List<int> _indices = new List<int>();
  static List<Vector4> _vertex_colors = new List<Vector4>();
  ComputeBuffer _mesh_object_buffer;
  ComputeBuffer _vertex_buffer;
  ComputeBuffer _index_buffer;
  ComputeBuffer _vertex_color_buffer;

  void OnEnable() {
    SetupSphere();
  }

  void Awake() {
    _camera = GetComponent<Camera>();
    //_shader = GetComponent<MeshRenderer>().material;
  }

  void OnDisable() {
    if (!ReferenceEquals(_sphere_buffer, null)) {
      _sphere_buffer.Release();
    }

    if (!ReferenceEquals(_mesh_object_buffer, null)) {
      _mesh_object_buffer.Release();
    }

    if (!ReferenceEquals(_vertex_buffer, null)) {
      _vertex_buffer.Release();
    }

    if (!ReferenceEquals(_index_buffer, null)) {
      _index_buffer.Release();
    }
  }

  void OnRenderImage(RenderTexture source, RenderTexture destination) {
    Rebuild_MeshObject_Buffers();
    SetShaderParameters();
    Render(destination);
  }

  void Update() {
    if (transform.hasChanged) {
      _current_sample = 0;
      transform.hasChanged = false;
    }
    SetShaderParameters_at_runtime();
    //RayTracingShader.SetTexture(0, "_SkyboxTexture", _Video.mainTexture);
  }

  void Render(RenderTexture destination) {
    // Make sure we have a current render target
    InitRenderTexture();

    // Set the target and dispatch the compute shader
    RayTracingShader.SetTexture(0, "_Result", _target);
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
    } else {
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
    RayTracingShader.SetBuffer(0, "_Spheres", _sphere_buffer);
    var light_dir = _directional_light.transform.forward;
    RayTracingShader.SetVector("_DirectionalLight",
                              new Vector4(light_dir.x, light_dir.y, light_dir.z, _directional_light.intensity));
    SetComputeBuffer("_Spheres", _sphere_buffer);
    SetComputeBuffer("_MeshObjects", _mesh_object_buffer);
    SetComputeBuffer("_Vertices", _vertex_buffer);
    SetComputeBuffer("_Indices", _index_buffer);
    SetComputeBuffer("_Colors", _vertex_color_buffer);
  }

  void SetShaderParameters_at_runtime() {
    RayTracingShader.SetVector("_Time",
    new Vector4(Time.time * 10,
                Time.time * 20,
                Time.time * 50,
                Time.time * 100));
  }

  void SetupSphere() {
    var spheres = new Sphere_list_t();
    // add a number of random spheres.
    for (int i = 0; i < _spheres_max; ++i) {
      var sphere = new ray_tracing_sphere();
      // radius and radius.
      sphere._radius = _offset_Radius.x + UnityEngine.Random.value * ( _offset_Radius.y - _offset_Radius.x );
      var random_pos = UnityEngine.Random.insideUnitCircle * _sphere_placement_radius;
      sphere._position = new Vector3(random_pos.x, sphere._radius, random_pos.y);

      // Reject spheres that are intersecting others.
      foreach (var other in spheres) {
        float min_dist = sphere._radius + other._radius;
        if (Vector3.SqrMagnitude(sphere._position - other._position) < min_dist * min_dist) {
          goto skip_sphere;
        }
      }
      // Albedo and specular color.
      var col = UnityEngine.Random.ColorHSV();
      bool metal = UnityEngine.Random.value < 0.5f;
      sphere._albedo = metal ? Vector3.zero : new Vector3(col.r, col.g, col.b);
      sphere._specular = metal ? new Vector3(col.r, col.g, col.b) : Vector3.one * 0.04f;
      // Add the sphere to the list.
      spheres.Add(sphere);

    skip_sphere:
      continue;
    }

    // Assign to compute shader.
    _sphere_buffer = new ComputeBuffer(spheres.Count, 40);
    _sphere_buffer.SetData<ray_tracing_sphere>(spheres);
  }

  public static void register(ray_tracing_object obj) {
    _ray_tracing_objects.Add(obj);
    _mesh_object_need_rebuilding = true;
  }

  public static void unregister(ray_tracing_object obj) {
    _ray_tracing_objects.Remove(obj);
    _mesh_object_need_rebuilding = true;
  }

  void Rebuild_MeshObject_Buffers() {
    if (!_mesh_object_need_rebuilding) {
      return;
    }

    _mesh_object_need_rebuilding = false;
    _current_sample = 0;
    // Clear all lists.
    _mesh_objects.Clear();
    _vertices.Clear();
    _indices.Clear();
    _vertex_colors.Clear();
    // Loop over all objects and gather their data into a single list of vertices, indices and mesh_objects.
    // vertex color is 
    foreach (var go in _ray_tracing_objects) {
      var mesh = go.GetComponent<MeshFilter>().sharedMesh;
      // Add vertex data.
      int first_vtx = _vertices.Count;
      // _vertices -> list<Vector4> , mesh.vertices -> Vector4[].
      // AddRange() ->
      // Adds the elements of the specified collection to the end of the '_vertices(System.Collections.Generic.List)'.
      _vertices.AddRange(mesh.vertices);
      // Add index data - if the vertex buffer wasn't empty before, the indices need to be offset.
      int first_idx = _indices.Count;
      int[] indices = mesh.GetIndices(0);
      _indices.AddRange(indices.Select(i => i + first_vtx));
      bool is_cube = go is ray_tracing_cube;

      // Add the object itself.
      _mesh_objects.Add(new ray_tracing_mesh_object() {
        local_to_world_matrix = go.transform.localToWorldMatrix,
        indices_offset = first_idx,
        indices_count = indices.Length,
        // set 'true' if the element 'go' is convertible of 'ray_tracing_cube'.
        use_vertex_color = is_cube ? 1 : 0
      });

      // if element 'go' is convertible of 'ray_tracing_cube'
      // add the vertex color into the '_vertex_colors(list<Vector4>)'.
      if (is_cube) {
        // Add the vertex color.        
        _vertex_colors.AddRange(mesh.colors.Select(i => new Vector4(i.r, i.g, i.b, i.a)));
      }
    }

    CreateOrSetComputeBuffer(ref _mesh_object_buffer, _mesh_objects, 76);
    CreateOrSetComputeBuffer(ref _vertex_buffer, _vertices, 12);
    CreateOrSetComputeBuffer(ref _index_buffer, _indices, 4);
    CreateOrSetComputeBuffer(ref _vertex_color_buffer, _vertex_colors, 16);
  }

  static void CreateOrSetComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride)
    where T : struct {
    // check if we already have a compute buffer.
    if (!ReferenceEquals(buffer, null)) {
      // If no data or buffer doesn't match the given condition, release it.
      if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride) {
        buffer.Release();
        buffer = null;
      }
    }

    if (data.Count != 0) {
      // If the buffer has been released or wasn't there to begin with, create it.
      if (ReferenceEquals(buffer, null)) {
        buffer = new ComputeBuffer(data.Count, stride);
      }
      // Set data on the buffer.
      buffer.SetData(data);
    }
  }

  void SetComputeBuffer(string name, ComputeBuffer buffer) {
    if (!ReferenceEquals(buffer, null)) {
      RayTracingShader.SetBuffer(0, name, buffer);
    }
  }

};
