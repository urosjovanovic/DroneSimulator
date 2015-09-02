using UnityEngine;

public sealed class CameraController : MonoBehaviour
{
    private int activeCameraIndex;
    private bool showPreviewCam;
    private int previewCamPadding;
    private Camera[] cameras;

    // Inspector variables
    [Range(0.1f, 1.0f)]
    public float
        PreviewCameraWidth = 0.16f;
    public Camera ThirdPersonCamera;
    public Camera DroneCamera;
    public Camera FirstPersonCamera;
    public Camera RTSCamera;
    public Camera TopDownCamera;

    #region Properties

    private Camera ActiveCam
    {
        get { return this.cameras[this.activeCameraIndex]; }
    }

    private Camera PreviewCam { get; set; }

    #endregion

    // Use this for initialization
    private void Start()
    {
        this.cameras = new[]
        {
            this.ThirdPersonCamera,
            this.DroneCamera,
            this.FirstPersonCamera
        };
        for (int i = 0; i < this.cameras.Length; i++)
        {
            this.cameras[i].depth = this.cameras.Length - 1 - i;
            this.cameras[i].rect = new Rect(0, 0, 1, 1);
        }
        this.previewCamPadding = 12;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetButtonDown("ChangeCamera"))
        {
            this.activeCameraIndex = (this.activeCameraIndex + 1) % this.cameras.Length;
            this.ShowActiveCam();
            if (this.showPreviewCam)
                this.ShowPreviewCam();
        }

        if (Input.GetButtonDown("PreviewCamToggle"))
        {
            this.showPreviewCam = !this.showPreviewCam;
            this.ShowActiveCam();
            if (this.showPreviewCam)
                this.ShowPreviewCam();
        }

        //if (Input.GetButtonDown("RTSCamToggle"))
        //{
        //    this.SwitchRtsCamera();
        //}
        if(Input.GetButtonDown("TopDownCamToggle"))
        {
            this.SwitchTopDownCamera();
        }
    }

    private void OnGUI()
    {
        if (this.showPreviewCam)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.black);
            texture.Apply();
            GUI.skin.box.normal.background = texture;
            Rect rect = this.PreviewCam.pixelRect;
            GUI.Box(new Rect(rect.xMin - 1, Screen.height - rect.yMax - 6, 1, rect.height + 11), GUIContent.none);
            GUI.Box(new Rect(rect.xMin - 1 + rect.width, Screen.height - rect.yMax - 6, 1, rect.height + 11), GUIContent.none);
            GUI.Box(new Rect(rect.xMin - 1, Screen.height - rect.yMax - 1, rect.width, 1), GUIContent.none);
            GUI.Box(new Rect(rect.xMin - 1, Screen.height - rect.yMax - 1 + rect.height, rect.width, 1), GUIContent.none);
        }
    }

    private void ShowActiveCam()
    {
        foreach (var camera in this.cameras)
            camera.depth = -1;
        this.ActiveCam.depth = 0;
        this.ActiveCam.rect = new Rect(0, 0, 1, 1);
    }

    private void ShowPreviewCam()
    {
        float paddingRight = this.previewCamPadding * 100.0f / this.ActiveCam.pixelWidth / 100;
        float paddingBottom = this.previewCamPadding * 100.0f / this.ActiveCam.pixelHeight / 100;
        var previewCamRect = new Rect(1 - this.PreviewCameraWidth - paddingRight, paddingBottom, this.PreviewCameraWidth, this.PreviewCameraWidth * this.ActiveCam.aspect * 3 / 4);
        if (this.ActiveCam == this.DroneCamera)
        {
            this.ThirdPersonCamera.depth = 1;
            this.ThirdPersonCamera.rect = previewCamRect;
            this.PreviewCam = this.ThirdPersonCamera;
        }
        else
        {
            this.DroneCamera.depth = 1;
            this.DroneCamera.rect = previewCamRect;
            this.PreviewCam = this.DroneCamera;
        }
    }

    public void SwitchRtsCamera()
    {
        var drone = GameObject.Find("DJIPhantom");
        var dc = drone.GetComponent<DroneController>();

        if (this.RTSCamera.enabled == false)
        {
            this.RTSCamera.transform.position = drone.transform.position + new Vector3(2, 2, 2);
            this.RTSCamera.transform.LookAt(drone.transform.position);
            this.RTSCamera.GetComponent<RTSCameraController>().enabled = true;
            dc.RtsMode = true;
            dc.planeY = drone.gameObject.transform.position.y;
            dc.droneMovementPlane = new Plane(Vector3.up, new Vector3(0, dc.planeY, 0));
            drone.GetComponent<LineRenderer>().enabled = true;
            dc.ClearAutoMovement();
            this.RTSCamera.enabled = true;
            this.RTSCamera.depth = 0;
        }
        else
        {
            drone.GetComponent<LineRenderer>().enabled = false;
            this.RTSCamera.GetComponent<RTSCameraController>().enabled = false;
            this.RTSCamera.enabled = false;
            this.activeCameraIndex = 0;
            dc.RtsMode = false;
            this.cameras[this.activeCameraIndex].enabled = true;
        }
    }

    public void SwitchTopDownCamera()
    {
        var drone = GameObject.Find("DJIPhantom");
        var dc = drone.GetComponent<DroneController>();

        if (this.TopDownCamera.enabled == false)
        {
            foreach (var camera in this.cameras)
                camera.depth = -1;
            this.TopDownCamera.depth = 0;
            this.TopDownCamera.enabled = true;
            this.TopDownCamera.transform.position = drone.transform.position + new Vector3(0, 100, 0);
            this.TopDownCamera.transform.LookAt(drone.transform.position);
            this.TopDownCamera.GetComponent<TopDownCameraController>().enabled = true;
            drone.GetComponent<LineRenderer>().enabled = true;
            dc.RtsMode = true;
            dc.TopDownMode = true;
            dc.planeY = drone.gameObject.transform.position.y;
            dc.droneMovementPlane = new Plane(Vector3.up, new Vector3(0, dc.planeY, 0));
            dc.ClearAutoMovement();
        }
        else
        {
            foreach (var camera in this.cameras)
                camera.depth = -1;
            this.TopDownCamera.depth = -1;
            this.TopDownCamera.enabled = false;
            this.ShowActiveCam();
            this.TopDownCamera.GetComponent<TopDownCameraController>().enabled = false;
            drone.GetComponent<LineRenderer>().enabled = false;
            dc.RtsMode = false;
            dc.TopDownMode = false;
            dc.frozen = false;
        }
    }
}