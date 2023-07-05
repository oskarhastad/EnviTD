using UnityEngine;
using System.Collections;

public class DragCamera : MonoBehaviour
{
    float cameraHeight;
    float initialCameraHeigth;
    float scrollSpeed = 1f;
    Vector3 dragOrigin;
    bool cameraDragging = false;

    void Start()
    {
        cameraHeight = transform.position.y;
        initialCameraHeigth = cameraHeight;
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(2))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = cameraHeight;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit))
            {
                dragOrigin = hit.point;
            }
            cameraDragging = true;
        }
        if (Input.GetMouseButtonUp(2))
        {
            cameraDragging = false;
        }

        if (cameraDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = cameraHeight;
            RaycastHit hit;
            Vector3 delta = new Vector3();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit))
            {
                delta = hit.point - dragOrigin;
            }
            delta.y = 0;
            transform.Translate(delta * -1f, Space.World);

        }



        if (Input.mouseScrollDelta.y != 0)
        {
            // Scroll to zoom
            cameraHeight += -Input.mouseScrollDelta.y * scrollSpeed;
            transform.position = new Vector3(transform.position.x, cameraHeight, transform.position.z);

            // Also rotate camera based on zoom
            // transform.rotation = Quaternion.AngleAxis(cameraHeight * (75 / initialCameraHeigth), Vector3.right);
        }

    }


}
