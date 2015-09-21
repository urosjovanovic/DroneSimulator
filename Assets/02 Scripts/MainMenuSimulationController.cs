using UnityEngine;

public class MainMenuSimulationController : MonoBehaviour
{
    public bool GamePaused;
    private GameObject mainMenuPanel;

    // Use this for initialization
    private void Start()
    {
        this.mainMenuPanel = this.transform.FindChild("MainMenuPanel").gameObject;
        this.mainMenuPanel.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            this.GamePaused = !this.GamePaused;
            Time.timeScale = 1 - Time.timeScale;
            this.mainMenuPanel.SetActive(this.GamePaused);
        }
    }

    public void ResumeSimulationMode()
    {
        this.GamePaused = false;
        this.mainMenuPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        Application.LoadLevel("MainMenuScene");
    }

    public void Quit()
    {
        Application.Quit();
    }
}