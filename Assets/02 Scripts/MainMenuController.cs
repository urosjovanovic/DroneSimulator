using UnityEngine;

public class MainMenuController : MonoBehaviour
{
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