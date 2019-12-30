using UnityEngine;
public interface RTproceduralShapes{
  Vector3 Position { get; set; }
  float Radius { get; set; }
  Vector3 Albedo { get; set; }
  Vector3 Specular { get; set; }
  Vector3 Emission { get; set; }

  void Init();
}
