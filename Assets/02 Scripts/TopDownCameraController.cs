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

        this.cameraComponent.orthographicSize = Mathf.Clamp(this.cameraComponent.orthographicSize + Input.GetAxis("Mouse ScrollWheel") * this.zoomSpeed, 0.1f, 10);

        if (droneController.RtsMode && droneController.TopDownMode)
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
                DrawPointInput(point);
        }
    }

    private void DrawPointInput(Vector3 point)
    {
        coordinatePanel.gameObject.SetActive(true);
        var xCoord = coordinatePanel.FindChild("XCoordinate");
        var yCoord = coordinatePanel.FindChild("YCoordinate");
        var zCoord = coordinatePanel.FindChild("ZCoordinate");

        if (xCoord != null && zCoord != null)
        {
            xCoord.gameObject.GetComponent<InputField>().text = point.x + "";
            yCoord.gameObject.GetComponent<InputField>().text = point.y + "";
            zCoord.gameObject.GetComponent<InputField>().text = point.z + "";
        }
    }

    private void ConfirmButtonClick()
    {
        var xCoord = coordinatePanel.FindChild("XCoordinate");
        var yCoord = coordinatePanel.FindChild("YCoordinate");
        var zCoord = coordinatePanel.FindChild("ZCoordinate");

        if (xCoord && yCoord && zCoord)
        {
            float xPos, yPos, zPos;
            bool xCheck = float.TryParse(xCoord.gameObject.GetComponent<InputField>().text, out xPos);
            bool yCheck = float.TryParse(yCoord.gameObject.GetComponent<InputField>().text, out yPos);
            bool zCheck = float.TryParse(zCoord.gameObject.GetComponent<InputField>().text, out zPos);

            if (xCheck && yCheck && zCheck)
            {
                var destinationPoint = new Vector3(xPos, yPos, zPos);
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.GetComponent<SphereCollider>().isTrigger = true;
                sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                sphere.transform.position = destinationPoint;
                sphere.AddComponent<TriggerDestroy>();
                droneController.DestinationPoints.Add(destinationPoint);
                coordinatePanel.gameObject.SetActive(false);
            }
        }
    }
}
