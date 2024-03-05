using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
//using UnityEditor; // Required for accessing Editor functionality
#endif

public class FlyCamera : MonoBehaviour
{
  public float speed = 5.0f;
  public float sensitivity = 2.0f;
  private float yaw = 0.0f;
  private float pitch = 0.0f;

  void Start()
  {
    // Lock cursor in the game window for a better experience
    Cursor.lockState = CursorLockMode.Locked;
  }

  void Update()
  {
    // Exit on ESC key
    if (Input.GetKeyDown(KeyCode.Escape))
    {
#if UNITY_EDITOR
      // If running in the Unity Editor
      UnityEditor.EditorApplication.isPlaying = false;
#else
            // If running in a built game
            Application.Quit();
#endif
    }
    // Camera rotation with mouse
    yaw += sensitivity * Input.GetAxis("Mouse X");
    pitch -= sensitivity * Input.GetAxis("Mouse Y");
    pitch = Mathf.Clamp(pitch, -89f, 89f); // Prevents the gimbal lock
    transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

    // Camera movement with keyboard
    float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
    float moveZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;
    // Corrected vertical movement with Q and E
    float moveY = (Input.GetKey(KeyCode.E) ? speed : 0) - (Input.GetKey(KeyCode.Q) ? speed : 0);
    moveY *= Time.deltaTime;


    Vector3 move = transform.right * moveX + transform.forward * moveZ + transform.up * moveY;
    transform.position += move;
  }
}
