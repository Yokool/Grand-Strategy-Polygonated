using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    [SerializeField]
    private GameObject camera;

    private Vector3 lastMousePosition = Vector3.zero;
    private bool started = false;
    [SerializeField]
    private float draggingCoefficient;

    private void Update()
    {

        // Drag Start
        if (!started && Input.GetMouseButtonDown(0))
        {
            DragStarted();
            return;
        }
        else
        {
            // Drag End
            if (started)
            {
                DragEnded();
                return;
            }

            if (!Input.GetMouseButton(0))
            {
                return;
            }

            // During Drag
            DuringDrag();

        }

    }

    private void DuringDrag()
    {
        Vector3 currentMousePosition = Input.mousePosition;

        Vector3 difference = lastMousePosition - currentMousePosition;
        difference *= draggingCoefficient;

        lastMousePosition = currentMousePosition;

        camera.transform.position = camera.transform.position + difference;

    }

    private void DragStarted()
    {
        lastMousePosition = Input.mousePosition;
        started = true;
    }

    private void DragEnded()
    {
        started = false;
    }

}
