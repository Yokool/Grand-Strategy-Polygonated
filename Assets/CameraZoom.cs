using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField]
    private Camera camera;


    private int zoom = 0;
    [SerializeField]
    private int maxZoom;

    [SerializeField]
    private float zoomIncrement;

    void Start()
    {
        
    }

    void Update()
    {

        Vector2 mouseScrollDelta = Input.mouseScrollDelta;

        // Zooming In
        if(mouseScrollDelta.y > 0)
        {
            
            if(zoom >= maxZoom)
            {
                zoom = maxZoom;
                return;
            }
            ++zoom;

            camera.orthographicSize -= zoomIncrement;
        }
        // Zooming out
        else if(mouseScrollDelta.y < 0)
        {
            if (zoom <= 0)
            {
                zoom = 0;
                return;
            }
            --zoom;
            camera.orthographicSize += zoomIncrement;
        }

    }
}
