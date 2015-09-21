using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    private GameObject drone;

    private void Start()
    {
        this.drone = GameObject.Find("DJIPhantom");
    }

    private void Update()
    {
        if(this.drone != null)
            this.drone.transform.Rotate(Vector3.up, 10.0f * Time.deltaTime, Space.World);
    }

    public void StartArcadeMode()
    {
        Application.LoadLevel("MazeRunnerScene");
    }

    public void StartSimulationMode()
    {
        Application.LoadLevel("TestArea");
    }

    public void Quit()
    {
        Application.Quit();
    }
}