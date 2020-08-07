using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotater : MonoBehaviour
{
    private bool isRotating;

    void Start()
    {
        isRotating = false;
    }

    /// <summary>
    /// Rotate the camera.
    /// </summary>
    /// <param name="angle"> The angle to rotate the camera. </param>
    /// <param name="timeFactor"> The time factor on the duration of the rotation. </param>
    /// <returns></returns>
    IEnumerator RotateCamera(Vector3 angle, float timeFactor)
    {
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(transform.eulerAngles + angle);
        for (var t = 0f; t <= 1; t += Time.deltaTime / timeFactor)
        {
            transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
        transform.rotation = toAngle;
        isRotating = false;
    }
    void Update()
    {
        if (!isRotating)
        {
            // Rotate the camera left.
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                isRotating = true;
                StartCoroutine(RotateCamera(Vector3.up * 90, 0.8f));
            }

            // Rotate the camera right.
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                isRotating = true;
                StartCoroutine(RotateCamera(Vector3.up * -90, 0.8f));
            }
        }
    }
}

