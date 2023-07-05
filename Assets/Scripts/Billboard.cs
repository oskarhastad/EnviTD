using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    Transform cam;
    void Start()
    {
        cam = GameObject.FindWithTag("MainCamera").transform;
    }
    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
        Vector3 addZ = new Vector3(0,0.2f,0.18f);
        transform.position = transform.parent.position + addZ;
    }
}
