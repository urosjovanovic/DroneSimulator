using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TopDownCameraController : MonoBehaviour
{
    private float mouseX = 0;
    private float mouseY = 0;
    private float zoomSpeed = 1;
    private Camera cameraComponent;
    

    public float panSpeed = 1;
    public DroneController droneController;
    public CameraController cameraController;
    public RoutingController routingControler;
    public RectTransform coordinatePanel;

    // Use this for initialization
    private void Start()
    {
        this.cameraComponent = this.gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    private void Update()
    {
        float mouseDeltaX = mouseX - Input.mousePosition.x;
        float mouseDeltaY = mouseY - Input.mousePosition.y;

        if (Input.GetMouseButton(0))
        {
            gameObject.transform.Translate(Vector3.right * mouseDeltaX * panSpeed * Time.deltaTime);
            gameObject.transform.Translate(Vector3.up * mouseDeltaY * panSpeed * Time.deltaTime);
        }

        mouseX = Input.mousePosition.x;
        mouseY = Input.mousePosition.y;

        this.cameraComponent.orthographicSize = Mathf.Clamp(this.cameraComponent.orthographicSize + Input.GetAxis("Mouse ScrollWheel") * this.zoomSpeed, 0.1f, 100);

        if (droneController.TopDownMode)
            DrawTopDown();
    }

    private void DrawTopDown()
    {
        var ray = gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        float rayDistance = 0f;

        if (droneController.droneMovementPlane.Raycast(ray, out rayDistance))
        {
            var point = ray.GetPoint(rayDistance);
            if (Input.GetMouseButtonDown(1) && !coordinatePanel.gameObject.activeSelf)
                this.routingControler.DrawPointInput(point);
        }
    }
}
