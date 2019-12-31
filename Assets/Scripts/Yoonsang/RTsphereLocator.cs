using System.Collections.Generic;
using UnityEngine;

public class RTsphereLocator : MonoBehaviour {
  [Header("Offset Radius for Sphere ")]
  public Vector2 OffsetRadius = new Vector2(3.0f, 8.0f);
  public uint SphereMaxAmount = 20;
  public float SpherePlacementRadiusOffset = 100.0f;
  public ComputeBuffer SpheresComputeBuf;

  void OnDisable() {
    if (!SpheresComputeBuf.Null()) {
      SpheresComputeBuf.Release();
      SpheresComputeBuf = null;
    }
  }

  public void LocateSphereRandomly() {
    var spheresList = new List<RTsphere>();
    for (int i = 0; i < SphereMaxAmount; ++i) {
      var sphere = new RTsphere();
      // Setup the radius and the offsets.
      sphere.Radius = OffsetRadius.x + UnityEngine.Random.value
        * ( OffsetRadius.y - OffsetRadius.x );
      Vector2 randPos = UnityEngine.Random.insideUnitSphere * SpherePlacementRadiusOffset;
      sphere.Position = new Vector3(randPos.x, sphere.Radius, randPos.y);
      // Reject spheres that are intersecting others.
      foreach (var other in spheresList) {
        float minDist = sphere.Radius + other.Radius;
        if (Vector3.SqrMagnitude(sphere.Position - other.Position) < minDist * minDist) {
          // if the distance is less than the radius of the two comparer,
          // continue to the next condition.
          goto SKIP_INITIALIZING_SPHERE;
        }
      }
      // Setup the Albedo, the Specular and the emission color.
      var col = UnityEngine.Random.ColorHSV();
      var randCol = new Vector3(col.r, col.g, col.b);
      // Calculate if the material is metal by random (by 50 % chances).
      bool isMetal = UnityEngine.Random.value < 0.5f;
      sphere.Albedo = isMetal ? Vector3.zero : randCol;
      sphere.Specular = isMetal ? randCol : Vector3.one * 0.04f;
      // Add the sphere into the list.
      spheresList.Add(sphere);

    // continue to the next condition.
    SKIP_INITIALIZING_SPHERE:
      continue;
    }
    // Assign to compute shader.
    SpheresComputeBuf = new ComputeBuffer(spheresList.Count, 52);
    SpheresComputeBuf.SetData<RTsphere>(spheresList);
  }
};
