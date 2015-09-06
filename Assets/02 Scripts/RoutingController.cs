using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoutingController : MonoBehaviour
{
    private bool autoMode;
    private DroneController droneController;
    private CameraController cameraController;
    private Canvas inGameUI;
    private Canvas routeUI;
    private Transform coordinatePanel;
    private Queue<GameObject> routingPoints;
    private LineRenderer lineRenderer;
    private GameObject inputField;

    #region Properties

    public Queue<GameObject> RoutingPoints
    {
        get { return this.routingPoints; }
    }

    #endregion

    // Use this for initialization
    private void Start()
    {
        this.droneController = this.gameObject.GetComponent<DroneController>();
        this.cameraController = GameObject.Find("Dummy").GetComponent<CameraController>();
        this.inGameUI = GameObject.Find("IngameUI").GetComponent<Canvas>();
        this.routeUI = GameObject.Find("RouteUI").GetComponent<Canvas>();
        this.inputField = GameObject.Find("InputField");

        this.coordinatePanel = this.routeUI.transform.FindChild("CoordinateInputPanel");

        this.lineRenderer = this.gameObject.GetComponent<LineRenderer>();
        this.lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        this.lineRenderer.SetColors(Color.red, Color.red);
        this.lineRenderer.SetWidth(0.05f, 0.05f);

        this.routingPoints = new Queue<GameObject>();
    }

    // Update is called once per frame
    private void Update()
    {
        if(this.droneController.TopDownMode || this.droneController.RtsMode)
        {
            this.lineRenderer.enabled = true;
            this.lineRenderer.SetVertexCount(this.routingPoints.Count + 1);
            this.lineRenderer.SetPosition(0, this.transform.position);
            int i = 1;
            foreach (var point in this.routingPoints)
                this.lineRenderer.SetPosition(i++, point.transform.position);
        }
        else
            this.lineRenderer.enabled = false;

        if (this.droneController.RtsMode && this.droneController.TopDownMode && this.routingPoints.Count == 0)
            this.OnAutoModeButtonClick();
    }

    public void OnAutoModeButtonClick()
    {
        this.autoMode = !this.autoMode;

        this.inGameUI.enabled = !this.autoMode;
        GameObject.Find("AutoModeText").GetComponent<Text>().text = this.autoMode ? "Exit" : "Route Planning";
        this.routeUI.transform.Find("StartButton").gameObject.SetActive(this.autoMode);

        if (this.autoMode)
        {
            this.lineRenderer.enabled = true;
            this.droneController.TopDownMode = true;
            this.droneController.frozen = false;
            this.droneController.planeY = this.transform.position.y;
            this.droneController.droneMovementPlane = new Plane(Vector3.up, new Vector3(0, this.droneController.planeY, 0));
        }
        else
        {
            this.droneController.TopDownMode = false;
            this.droneController.RtsMode = false;
            this.droneController.frozen = true;

            while (this.routingPoints.Count > 0)
                Destroy(this.routingPoints.Dequeue());
            this.droneController.ClearAutoMovement();
        }

        this.cameraController.SwitchTopDownCamera(this.autoMode, this.transform.position);
    }

    public void OnStartButtonClick()
    {
        if (this.routingPoints.Count > 0)
        {
            this.autoMode = true;
            this.inGameUI.enabled = true;
            this.routeUI.transform.Find("StartButton").gameObject.SetActive(false);
            this.droneController.RtsMode = true;
            this.droneController.frozen = false;

            this.cameraController.SwitchTopDownCamera(false, this.transform.position);
        }
    }

    public void DrawPointInput(Vector3 point)
    {
        this.coordinatePanel.gameObject.SetActive(true);
        var xCoord = this.coordinatePanel.FindChild("XCoordinate");
        var yCoord = this.coordinatePanel.FindChild("YCoordinate");
        var zCoord = this.coordinatePanel.FindChild("ZCoordinate");

        if (xCoord != null && zCoord != null)
        {
            xCoord.gameObject.GetComponent<InputField>().text = point.x + "";
            yCoord.gameObject.GetComponent<InputField>().text = point.y + "";
            zCoord.gameObject.GetComponent<InputField>().text = point.z + "";
        }
    }

    public void ConfirmButtonClick()
    {
        var xCoord = this.coordinatePanel.FindChild("XCoordinate");
        var yCoord = this.coordinatePanel.FindChild("YCoordinate");
        var zCoord = this.coordinatePanel.FindChild("ZCoordinate");

        if (xCoord && yCoord && zCoord)
        {
            float xPos, yPos, zPos;
            bool xCheck = float.TryParse(xCoord.gameObject.GetComponent<InputField>().text, out xPos);
            bool yCheck = float.TryParse(yCoord.gameObject.GetComponent<InputField>().text, out yPos);
            bool zCheck = float.TryParse(zCoord.gameObject.GetComponent<InputField>().text, out zPos);

            if (xCheck && yCheck && zCheck)
            {
                var destinationPoint = new Vector3(xPos, yPos, zPos);
                this.MakeNewSphere(destinationPoint);
                this.coordinatePanel.gameObject.SetActive(false);
            }
        }
    }

    private void MakeNewSphere(Vector3 position)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        sphere.GetComponent<SphereCollider>().isTrigger = true;
        sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        sphere.transform.position = position;
        sphere.AddComponent<TriggerDestroy>();
        this.routingPoints.Enqueue(sphere);

        /*var newInputField = Instantiate(Resources.Load<GameObject>("RouteLabel"));
        newInputField.transform.position = this.cameraController.TopDownCamera.WorldToScreenPoint(position) + new Vector3(50, 15, 0);
        newInputField.transform.SetParent(this.routeUI.transform);*/
    }
}