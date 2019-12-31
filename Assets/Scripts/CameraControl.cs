using UnityEngine;
public class CameraControl : MonoBehaviour {
  public float x_min_rotation;
  public float x_max_rotation;
  public float x_sensitivity = 10.0f;
  public float y_sensitivity = 10.0f;
  float rot_around_x, rot_around_y;
  bool is_cam_moved = false;
  public float move_speed = 10.0f;
  float original_move_speed;
  float forward, strafe;  

  void Start() {
    rot_around_x = transform.eulerAngles.x;
    rot_around_y = transform.eulerAngles.y;
    original_move_speed = move_speed;
  }

  void Update() {
    // rotate the camera.
    rot_around_x += Input.GetAxisRaw("Mouse Y") * x_sensitivity;
    rot_around_y += Input.GetAxisRaw("Mouse X") * y_sensitivity;
    rot_around_x = Mathf.Clamp(rot_around_x, x_min_rotation, x_max_rotation);
    transform.rotation = Quaternion.Euler(-rot_around_x, rot_around_y, 0);
    // move faster.
    if (Input.GetKey(KeyCode.LeftShift)) {
      move_speed = original_move_speed * 2.0f;
    }
    if (!Input.GetKey(KeyCode.LeftShift) &&
      original_move_speed != move_speed) {
      move_speed = original_move_speed;
    }      
    // move the camera.
    forward = Input.GetAxisRaw("Vertical") * move_speed * Time.deltaTime;
    strafe = Input.GetAxisRaw("Horizontal") * move_speed * Time.deltaTime;
    transform.Translate(strafe, 0, forward);
    // fly-upward the camera.
    if (Input.GetKey(KeyCode.E)) {
      transform.Translate(0, transform.up.y * move_speed * Time.deltaTime, 0);
    }
    // fly-downward the camera.
    if (Input.GetKey(KeyCode.Q)) {
      transform.Translate(0, -transform.up.y * move_speed * Time.deltaTime, 0);
    }
  }
};
