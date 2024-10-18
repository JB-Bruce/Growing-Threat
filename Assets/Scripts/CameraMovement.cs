using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float cameraSpeed;
    public float cameraSprintSpeed;
    public Camera cam;

    public float minZoom;
    public float maxZoom;

    void Update()
    {
        Vector2 move = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) move += Vector2.up;
        if (Input.GetKey(KeyCode.S)) move += Vector2.down;
        if (Input.GetKey(KeyCode.D)) move += Vector2.right;
        if (Input.GetKey(KeyCode.A)) move += Vector2.left;

        var lastMove = move.normalized * cameraSpeed * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? cameraSprintSpeed : 1);
        transform.position += new Vector3(lastMove.x, lastMove.y, 0);
        Vector2 dm = Input.mouseScrollDelta;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - dm.y, minZoom, maxZoom);
    }
}
