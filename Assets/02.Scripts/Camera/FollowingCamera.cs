using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public Transform player;
    public float dist = 10f;
    public float height = 5f;
    public float smoothRotate = 5f;

    private Transform cam;

    private void Start()
    {
        cam = GetComponent<Transform>();
    }

    private void LateUpdate()
    {
        float currentYAngle = Mathf.LerpAngle(cam.eulerAngles.y, player.eulerAngles.y, smoothRotate * Time.deltaTime);

        Quaternion rot = Quaternion.Euler(0, currentYAngle, 0);

        cam.position = player.position - (rot * Vector3.forward * dist) + (Vector3.up * height);
        cam.LookAt(player);
    }

}
