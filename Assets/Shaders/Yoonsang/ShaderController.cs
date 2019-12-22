using UnityEngine;
public class ShaderController : MonoBehaviour {
    Material Mat;
    public enum Target {
        CONE, CYLINDER
    };

    public Target target;
    void Start() {
      Mat = GetComponent<MeshRenderer>().sharedMaterial;    
    }

    void Update() {
      switch (target) {
        case Target.CONE:
        Mat.DisableKeyword("CYLINDER_ON");
        Mat.EnableKeyword("CONE_ON");
        break;

        case Target.CYLINDER:
        Mat.EnableKeyword("CYLINDER_ON");
        Mat.DisableKeyword("CONE_ON");
        break;
      }
    }
}
