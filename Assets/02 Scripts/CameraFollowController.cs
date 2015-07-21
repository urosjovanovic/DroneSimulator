using UnityEngine;
using System.Collections;

public sealed class CameraFollowController : MonoBehaviour 
{
    public Transform Target;
    [Range(0.5f,5)]
    public float Distance = 1.5f;

	// Use this for initialization
	void Start () 
    {
	    if(this.Target == null)
        {
            Debug.LogError("Target object is missing.");
            this.enabled = false;
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        this.transform.position = this.Target.position - Vector3.Cross(this.Target.right, Vector3.up) * this.Distance + Vector3.up * this.Distance / 2;
        this.transform.LookAt(this.Target.position);
	}
}
