using UnityEngine;
using System.Collections;

public class MazeRunnerCameraController : MonoBehaviour
{
    private Transform target;

    // Use this for initialization
    private void Start()
    {
        this.target = GameObject.Find("DJIPhantom").transform;
    }

    // Update is called once per frame
    private void Update()
    {
        this.transform.position = this.target.position - new Vector3(2f, -0.25f, 0);
        this.transform.LookAt(this.target);
    }
}
