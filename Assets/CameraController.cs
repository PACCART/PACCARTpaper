using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{

    public float moveSpeed = 0.01f;
    public  float scrollSpeed = 5f;

    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            transform.position += moveSpeed * new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"),0);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            GetComponent<Camera>().orthographicSize += scrollSpeed  * -Input.GetAxis("Mouse ScrollWheel");
            transform.localScale += scrollSpeed * new Vector3(0, -Input.GetAxis("Mouse ScrollWheel"), 0);
            //transform.position += scrollSpeed * new Vector3(0, -Input.GetAxis("Mouse ScrollWheel"), 0);
        }
    }

}