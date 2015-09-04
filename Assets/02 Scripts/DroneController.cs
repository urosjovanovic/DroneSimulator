using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class DroneController : MonoBehaviour
{
    private Rotor[] rotors;
    private Transform droneModel;
    private Rigidbody droneRigidbody;
    private float verticalSpeed;
    private Vector3 droneCameraViewDirection;
    private float droneModelAngle;

    #region Inspector Accessible Params

    //Looking from the top of the drone
    public Transform FrontLeftRotor;
    public Transform FrontRightRotor;
    public Transform BackLeftRotor;
    public Transform BackRightRotor;

    //Optional params
    public Camera DroneCamera;
	public Camera TopDownCamera;
    public Transform CameraRollSystem;
    public Transform CameraPitchSystem;

    [Range(1, 10)]
    public float MaxSpeed = 6;
    [Range(0, 2)]
    public float VerticalStabilizationFactor = 1.0f;
    [Range(0, 2)]
    public float VerticalAccelerationFactor = 1.2f;
    [Range(0, 2)]
    public float HorizontalXStabilizationFactor = 1.0f;
    [Range(0, 2)]
    public float HorizontalYStabilizationFactor = 1.0f;
    [Range(0, 2)]
    public float HorizontalXAccFactor = 1.0f;
    [Range(0, 2)]
    public float HorizontalYAccFactor = 1.0f;

    #endregion

    #region Properties

    private bool HasCamera
    {
        get { return this.DroneCamera != null; }
    }

    private bool CameraRollSupported
    {
        get { return this.CameraRollSystem != null; }
    }

    private bool CameraPitchSupported
    {
        get { return this.CameraPitchSystem != null; }
    }

    #endregion

    #region RTSParams

    public bool RtsMode = false;

    private float verticalAxis = 0.0f;
    private float horizontalAxis = 0.0f;
    private float tiltAxis = 0.0f;
    private float sidewayTiltAxis = 0.0f;

    public Plane droneMovementPlane;
    public float planeY;
    private GameObject RTSCamera;

    private bool moving = false;
    private bool rotating = false;
    private bool inCoroutine = false;
    private float scrollFactor = 2f; //TODO: Ovo izbaciti odavde

    private float maxIdleVelocity;
    private float autoRotateSpeed;
    private float autoRotateAngleTreshold;
    private float autoElevateSpeed;
    private float autoMoveSpeed;

    public List<Vector3> DestinationPoints { get; set; }
    [Range(0, 1)]
    public float MaxIdleVelocity = 0.02f;
    [Range(0, 1)]
    public float AutoRotateSpeed = 0.5f;
    [Range(0, 1)]
    public float AutoRotateAngleTreshold = 0.25f;
    [Range(0, 1)]
    public float AutoElevateSpeed = 0.1f;
    [Range(0, 1)]
    public float AutoMoveSpeed = 0.2f;

	public bool TopDownMode = false;
	public bool frozen = false;

    #endregion

    // Use this for initialization
    void Start()
    {
        try
        {
            if (this.FrontLeftRotor == null || this.FrontRightRotor == null || this.BackLeftRotor == null || this.BackRightRotor == null)
                throw new ArgumentNullException("One or more rotor objects are missing");

            this.droneModel = this.gameObject.transform;
            this.droneRigidbody = this.droneModel.GetComponent<Rigidbody>();

            if (this.droneRigidbody == null)
                throw new ArgumentNullException("No rigidbody found.");

            this.rotors = new Rotor[4] { new Rotor(this.FrontLeftRotor), new Rotor(this.FrontRightRotor), new Rotor(this.BackLeftRotor), new Rotor(this.BackRightRotor) };

            this.droneRigidbody.centerOfMass = new Vector3(0f, -0.1f, 0f); //Fixes the random sideways tilting bug

            if (this.HasCamera)
            {
                this.droneCameraViewDirection = this.DroneCamera.transform.forward;
                this.droneModelAngle = this.droneModel.eulerAngles.y;
            }

            this.maxIdleVelocity = MaxIdleVelocity;
            this.autoRotateSpeed = AutoRotateSpeed;
            this.autoRotateAngleTreshold = AutoRotateAngleTreshold;
            this.autoElevateSpeed = AutoElevateSpeed;
            this.autoMoveSpeed = AutoMoveSpeed;

            this.RTSCamera = GameObject.Find("RTSCamera");
            this.DestinationPoints = new List<Vector3>();
        }
        catch (Exception ex)
        {
            Debug.LogError("DroneController Initialization failed: " + ex.Message);
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.LogFormat("In Coroutine: {0}, Moving: {1}, Rotating: {2}", inCoroutine, moving, rotating);

        if(RtsMode)
        {
            if (!moving)
            {
                verticalAxis = 0.0f;
                tiltAxis = 0.0f;
                sidewayTiltAxis = 0.0f;
            }

            if (!rotating)
            {
                horizontalAxis = 0.0f;
            }


            if (DestinationPoints.Count > 0 && !inCoroutine && droneRigidbody.velocity.magnitude < maxIdleVelocity && droneRigidbody.angularVelocity.magnitude < 0.1 && !frozen)
            {
                StartCoroutine("MoveTo", 0);
            }
        }

        foreach (var rotor in this.rotors)
            rotor.Transform.RotateAround(rotor.Transform.GetObjectCenter(), this.droneModel.up, 3600 * Time.deltaTime);

        float cameraAxis = Input.GetAxis("CameraAxis");
        float cameraAngle = Vector3.Angle(Vector3.up, this.droneCameraViewDirection);
        this.droneCameraViewDirection = Quaternion.Euler(0, this.droneModel.eulerAngles.y - this.droneModelAngle, 0) * this.droneCameraViewDirection;
        if (cameraAngle >= 90 && cameraAxis >= 0 || cameraAngle < 179 && cameraAxis <= 0)
            this.droneCameraViewDirection = Quaternion.AngleAxis(cameraAxis, -this.DroneCamera.transform.right) * this.droneCameraViewDirection;
        this.DroneCamera.transform.forward = this.droneCameraViewDirection;
        this.droneModelAngle = this.droneModel.eulerAngles.y;

        if (RtsMode && !TopDownMode)
            DrawLine();
        else if (RtsMode && TopDownMode)
        {
        }
    }

    // Run all physics based stuff here
    void FixedUpdate()
    {
        if (!RtsMode)
        {
            verticalAxis = Input.GetAxis("Vertical");
            horizontalAxis = Input.GetAxis("Horizontal");
            tiltAxis = Input.GetAxis("Vertical2");
            sidewayTiltAxis = Input.GetAxis("Horizontal2");
        }


        this.Elevate(verticalAxis * this.VerticalAccelerationFactor);
        this.Tilt(tiltAxis);
        this.SidewayTilt(sidewayTiltAxis);

        foreach (var rotor in this.rotors)
            this.droneRigidbody.AddForceAtPosition(Vector3.up * rotor.Force, rotor.Transform.position, ForceMode.Force);

        Vector3 relativeVelocity = this.droneRigidbody.GetLocalVelocity();

        if (tiltAxis >= 0 && relativeVelocity.z < 0 || tiltAxis <= 0 && relativeVelocity.z > 0)
            tiltAxis -= relativeVelocity.z * this.HorizontalXStabilizationFactor;
        this.droneRigidbody.AddForce(Vector3.Cross(this.droneModel.right, Vector3.up).normalized * tiltAxis * this.MaxSpeed * this.HorizontalXAccFactor);

        if (sidewayTiltAxis >= 0 && relativeVelocity.x < 0 || sidewayTiltAxis <= 0 && relativeVelocity.x > 0)
            sidewayTiltAxis -= relativeVelocity.x * this.HorizontalYStabilizationFactor;
        this.droneRigidbody.AddForce(Vector3.Cross(Vector3.up, this.droneModel.forward).normalized * sidewayTiltAxis * this.MaxSpeed * this.HorizontalYAccFactor);

        if ((horizontalAxis >= 0 && this.droneRigidbody.angularVelocity.y < 0 || horizontalAxis <= 0 && this.droneRigidbody.angularVelocity.y > 0))
            horizontalAxis -= this.droneRigidbody.angularVelocity.y;

        this.droneRigidbody.AddTorque(Vector3.up * horizontalAxis * 0.5f);

        if (this.droneRigidbody.velocity.magnitude > this.MaxSpeed)
            this.droneRigidbody.velocity = this.droneRigidbody.velocity.normalized * this.MaxSpeed;

        // Debug.Log(string.Format("X: {1:0.0} Y: {0:0.0} Z: {2:0.0}", this.droneRigidbody.velocity.y, relativeVelocity.x, relativeVelocity.z));
    }

    private void Elevate(float amount)
    {
        foreach (var rotor in this.rotors)
            rotor.Force = 2.4525f;
        foreach (var rotor in this.rotors)
        {
            rotor.Force += amount;
            if (amount >= 0 && this.droneRigidbody.velocity.y < 0 || amount <= 0 && this.droneRigidbody.velocity.y > 0)
                rotor.Force -= this.droneRigidbody.velocity.y * this.VerticalStabilizationFactor;
        }
    }

    private void Tilt(float amount)
    {
        const float mult = 1;
        this.rotors[0].Force -= amount * rotors[0].Force * mult;
        this.rotors[1].Force -= amount * rotors[1].Force * mult;
        this.rotors[2].Force += amount * rotors[2].Force * mult;
        this.rotors[3].Force += amount * rotors[3].Force * mult;
    }

    private void SidewayTilt(float amount)
    {
        const float mult = 1;
        this.rotors[1].Force -= amount * rotors[1].Force * mult;
        this.rotors[3].Force -= amount * rotors[3].Force * mult;
        this.rotors[0].Force += amount * rotors[0].Force * mult;
        this.rotors[2].Force += amount * rotors[2].Force * mult;
    }

    private void DrawLine()
    {
        var lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.SetWidth(0.01f, 0.01f);
        lineRenderer.SetPosition(0, gameObject.transform.position);

        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKey(KeyCode.LeftControl))
        {
            planeY += scrollWheel * scrollFactor;
            if (planeY < 0.2) planeY = 0.2f;

            droneMovementPlane = new Plane(Vector3.up, new Vector3(0, planeY, 0));
        }

        var ray = RTSCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        float rayDistance = 0f;

        if (droneMovementPlane.Raycast(ray, out rayDistance))
        {
            var point = ray.GetPoint(rayDistance);
            lineRenderer.SetPosition(1, point);
            if (Input.GetMouseButtonDown(1))
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.GetComponent<SphereCollider>().isTrigger = true;
                sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                sphere.transform.position = point;
                sphere.AddComponent<TriggerDestroy>();
                DestinationPoints.Add(point);
            }
        }
    }

    private IEnumerator MoveTo(int destinationPointIndex)
    {
        inCoroutine = true;
        yield return StartCoroutine("RotateToPoint", DestinationPoints[destinationPointIndex]);
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine("ElevateToPoint", DestinationPoints[destinationPointIndex]);
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine("MoveForward", DestinationPoints[destinationPointIndex]);

        if (Vector3.Distance(gameObject.transform.position, DestinationPoints[destinationPointIndex]) <= 0.5)
            DestinationPoints.RemoveAt(destinationPointIndex);

        inCoroutine = false;
        yield return null;
    }

    private IEnumerator RotateToPoint(Vector3 point)
    {
        rotating = true;
        var dronePos = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
        var pointPos = new Vector3(point.x, 0, point.z);
        var rotateDir = pointPos - dronePos;
        var angleBetween = Vector3.Angle(gameObject.transform.forward, rotateDir);

        // Smer rotacije drona
        Vector2 v1 = new Vector2(gameObject.transform.forward.x, gameObject.transform.forward.z);
        Vector2 v2 = new Vector2(rotateDir.x, rotateDir.z);
        Vector3 cross = Vector3.Cross(v1, v2);

        while (angleBetween > autoRotateAngleTreshold)
        {
            angleBetween = Vector3.Angle(gameObject.transform.forward, rotateDir);
            if (angleBetween < 10)
                autoRotateSpeed = Mathf.Clamp(autoRotateSpeed / 2, 0.1f, 1.0f);
            // rotate LEFT
            if (cross.z > 0)
                horizontalAxis = -autoRotateSpeed;
            // rotate RIGHT
            else
                horizontalAxis = autoRotateSpeed;
            yield return null;
            horizontalAxis = 0.0f;
        }

        rotating = false;
        autoRotateSpeed = AutoRotateSpeed;
        yield return null;
    }

    private IEnumerator ElevateToPoint(Vector3 point)
    {
        float targetY = point.y;
        float droneY = gameObject.transform.position.y;
        float deltaY = targetY - droneY;

        while (Math.Abs(deltaY) > 0.1 && autoElevateSpeed > 0)
        {
            float verticalVelocity = droneRigidbody.velocity.y;
            if (Math.Abs(verticalVelocity) > AutoElevateSpeed * 10)
                if (Math.Abs(deltaY) < (droneRigidbody.velocity.y > 0 ? 0.3 : 0.45))
                    autoElevateSpeed = 0;

            verticalAxis = Math.Sign(targetY - droneY) * autoElevateSpeed;

            yield return null;
            verticalAxis = 0.0f;
            droneY = gameObject.transform.position.y;
            deltaY = targetY - droneY;
        }

        autoElevateSpeed = AutoElevateSpeed;
        yield return null;
    }

    private IEnumerator MoveForward(Vector3 target)
    {
        moving = true;
        while (Vector3.Distance(gameObject.transform.position, target) > 0.2 && autoMoveSpeed > 0)
        {
            float angle = AngleTo(target);
            horizontalAxis = Math.Abs(angle) > 0.05 ? Math.Sign(angle) * 0.1f : 0.0f;
            if (Vector3.Distance(gameObject.transform.position, target) < droneRigidbody.velocity.magnitude / 8)
                autoMoveSpeed = 0;
            tiltAxis = autoMoveSpeed;
            yield return null;
            tiltAxis = 0.0f;
            horizontalAxis = 0.0f;
        }
        moving = false;
        autoMoveSpeed = AutoMoveSpeed;
        yield return null;
    }

    private void StabilizeDrone()
    {
        droneRigidbody.angularVelocity = Vector3.zero;
        droneRigidbody.velocity = Vector3.zero;
    }

    private float AngleTo(Vector3 target)
    {
        var dronePos = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
        var pointPos = new Vector3(target.x, 0, target.z);
        Vector3 rotateDir = pointPos - dronePos;
        Vector2 v1 = new Vector2(gameObject.transform.forward.x, gameObject.transform.forward.z);
        Vector2 v2 = new Vector2(rotateDir.x, rotateDir.z);
        Vector3 cross = Vector3.Cross(v1, v2);
        return -Math.Sign(cross.z) * Vector3.Angle(gameObject.transform.forward, rotateDir);
    }

    public void ClearAutoMovement()
    {
        this.DestinationPoints.Clear();
        StopCoroutine("MoveForward");
        StopCoroutine("RotateToPoint");
        StopCoroutine("MoveTo");
    }
}

public sealed class Rotor
{
    private readonly Transform transform;
    private float force;

    public Rotor(Transform transform)
    {
        this.transform = transform;
        this.force = 0;
    }

    public float Force
    {
        get { return this.force; }
        set { this.force = value; }
    }

    public Transform Transform
    {
        get { return this.transform; }
    }
}
