using UnityEngine;
using System;
using System.Collections;

public sealed class DroneController : MonoBehaviour 
{
    private Rotor[] rotors;
    private Vector3 cameraViewDirection;
    private Transform droneModel;
    private Rigidbody droneRigidbody;
    private float verticalSpeed;

    #region Inspector Accessible Params

    //Looking from the top of the drone
    public Transform FrontLeftRotor;
    public Transform FrontRightRotor;
    public Transform BackLeftRotor;
    public Transform BackRightRotor;

    //Optional params
    public Camera DroneCamera;
    public Transform CameraRollSystem;
    public Transform CameraPitchSystem;

    [Range(3,6)]
    public float MaxVerticalSpeed;

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

	// Use this for initialization
	void Start () 
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
            if (this.HasCamera)
                this.cameraViewDirection = this.DroneCamera.transform.forward;
            else
                this.cameraViewDirection = this.CameraPitchSystem.forward;
        }
        catch(Exception ex)
        {
            Debug.LogError("DroneController Initialization failed: " + ex.Message);
            this.enabled = false;
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        foreach (var rotor in this.rotors)
            rotor.Transform.RotateAround(rotor.Transform.GetObjectCenter(), this.droneModel.up, 3600*Time.deltaTime);

        //TODO: Simulating camera gyro
        //if (this.CameraRollSupported)
        //    this.CameraRollSystem.up = Vector3.up;
        //if (this.CameraPitchSupported)
        //    this.CameraPitchSystem.forward = this.cameraViewDirection;
	}

    // Run all physics based stuff here
    void FixedUpdate ()
    {
        float verticalAxis = Input.GetAxis("Vertical");
        float force = 2.4525f + (verticalAxis);
        foreach (var rotor in this.rotors)
        {
            rotor.Force = Math.Abs(force);
            this.droneRigidbody.AddForceAtPosition(Math.Sign(force)*Vector3.up * rotor.Force, rotor.Transform.position, ForceMode.Force);
        }
        if (this.droneRigidbody.velocity.magnitude > this.MaxVerticalSpeed)
            this.droneRigidbody.velocity = this.droneRigidbody.velocity.normalized * this.MaxVerticalSpeed;
        Debug.Log(this.droneRigidbody.velocity);
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
        get { return force; }
        set { force = value; }
    }

    public Transform Transform
    {
        get { return transform; }
    }
}
