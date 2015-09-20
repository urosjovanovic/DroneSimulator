using UnityEngine;
using UnityEngine.UI;

public class MainMenuArcadeController : MonoBehaviour
{
    private GameObject mainMenuPanel;
    private Text gameOverText;
    private Text elapsedTimeText;
    private Text timeText;
    private Button resumeButton;

    // Use this for initialization
    private void Start()
    {
        this.mainMenuPanel = this.transform.FindChild("MainMenuPanel").gameObject;
        this.elapsedTimeText = this.mainMenuPanel.transform.FindChild("ElapsedTimeText").GetComponent<Text>();
        this.elapsedTimeText.gameObject.SetActive(false);
        this.timeText = this.transform.FindChild("GameTimeText").GetComponent<Text>();
        this.gameOverText = this.mainMenuPanel.transform.FindChild("GameOverText").GetComponent<Text>();
        this.gameOverText.gameObject.SetActive(false);
        this.resumeButton = this.mainMenuPanel.transform.FindChild("Resume").GetComponent<Button>();
        this.mainMenuPanel.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1 - Time.timeScale;
            this.mainMenuPanel.SetActive(!this.mainMenuPanel.activeSelf);
        }
    }

    public void GameOver()
    {
        this.mainMenuPanel.SetActive(true);
        this.gameOverText.gameObject.SetActive(true);
        this.resumeButton.gameObject.SetActive(false);
        this.elapsedTimeText.gameObject.SetActive(true);
        this.elapsedTimeText.text = string.Format("Elapsed Time {0:00}:{1:00}", (Time.timeSinceLevelLoad / 60) % 60, Time.timeSinceLevelLoad % 60);

        this.timeText.text = "Time's up!";
        this.timeText.color = Color.red;
    }

    public void StartArcadeMode()
    {
        Time.timeScale = 1;
        Application.LoadLevel("MazeRunnerScene");
    }

    public void ResumeArcadeMode()
    {
        this.mainMenuPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void StartSimulationMode()
    {
        Time.timeScale = 1;
        Application.LoadLevel("TestArea");
    }

    public void Quit()
    {
        Application.Quit();
    }
}