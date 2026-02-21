using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public Transform pivot;          // 回転中心
    public float rotateSpeed = 0.3f; // 回転感度
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private Vector3 offset;
    private float yaw;
    private float pitch;

    void Start()
    {
        if (pivot == null) return;

        offset = transform.position - pivot.position;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        if (pivot == null || !GameManager.instance.inputManager.isCameraMoving) return;

        if (Input.GetMouseButton(0))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            yaw   += dx * rotateSpeed * 100f;
            pitch -= dy * rotateSpeed * 100f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        offset = rot * Vector3.back * offset.magnitude;

        transform.position = pivot.position + offset;
        transform.LookAt(pivot.position);
    }
}
