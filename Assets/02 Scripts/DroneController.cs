using UnityEngine;
using System;
using System.Collections;

public sealed class DroneController : MonoBehaviour 
{
    private Rotor[] rotors;
    private Vector3 cameraViewDirection;
    private Transform droneModel;

    //Looking from the top of the drone
    public Transform FrontLeftRotor;
    public Transform FrontRightRotor;
    public Transform BackLeftRotor;
    public Transform BackRightRotor;

    //Optional params
    public Camera DroneCamera;
    public Transform CameraRollSystem;
    public Transform CameraPitchSystem;

	// Use this for initialization
	void Start () 
    {
        try
        {
            if (this.FrontLeftRotor == null || this.FrontRightRotor == null || this.BackLeftRotor == null || this.BackRightRotor == null)
                throw new ArgumentNullException("One or more rotor objects are missing");
            this.droneModel = this.gameObject.transform;
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

        //Simulating camera gyro
        //if (this.CameraRollSupported)
        //    this.CameraRollSystem.up = Vector3.up;
        //if (this.CameraPitchSupported)
        //    this.CameraPitchSystem.forward = this.cameraViewDirection;

        var rigidBody = this.droneModel.GetComponent<Rigidbody>();
        foreach(var rotor in this.rotors)
            rigidBody.AddForceAtPosition(Vector3.up*rotor.Force, rotor.Transform.position);
	}

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
}

public sealed class Rotor
{
    private readonly Transform transform;
    private float force;

    public Rotor(Transform transform)
    {
        this.transform = transform;
        this.force = 2.05f;
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
