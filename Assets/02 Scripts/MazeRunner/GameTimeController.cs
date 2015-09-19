using UnityEngine;
using System.Timers;
using System.Collections;
using UnityEngine.UI;

public class GameTimeController : MonoBehaviour
{
    private int remainingSeconds;
    private Transform inGameUI;
    private Text timeText;
    private Text timeUpdateText;
    private Text gameOverText;
    private Text elapsedTimeText;
    private Button reloadButton;
    private Timer timer;
    private Timer timeUpdateTimer;
    private bool gamePaused;

    public void UpdateTimeBy(int value)
    {
        if (this.gamePaused)
            return;

        this.remainingSeconds += value;
        if (this.remainingSeconds < 0)
            this.remainingSeconds = 0;

        if (value != -1)
        {
            this.timeUpdateText.text = string.Format("{0}{1}s", value > 0 ? "+" : "", value);
            this.timeUpdateText.color = value > 0 ? Color.green : Color.red;
            this.timeUpdateTimer.Interval = 2000;
            this.timeUpdateTimer.Start();
        }

    }

    public void ReloadLevel()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    // Use this for initialization
    private void Start()
    {
        Time.timeScale = 1;
        this.remainingSeconds = 60;
        this.inGameUI = GameObject.Find("InGameUI").transform;
        this.timeText = this.inGameUI.GetChild(0).GetComponent<Text>();
        this.timeUpdateText = this.inGameUI.GetChild(1).GetComponent<Text>();
        this.gameOverText = this.inGameUI.GetChild(2).GetComponent<Text>();
        this.reloadButton = this.inGameUI.GetChild(3).GetComponent<Button>();
        this.elapsedTimeText = this.inGameUI.GetChild(4).GetComponent<Text>();
        this.timer = new Timer(1000);
        this.timer.Elapsed += timer_Elapsed;
        this.timer.Start();
        this.timeUpdateTimer = new Timer(100);
        this.timeUpdateTimer.Elapsed += timeUpdateTimer_Elapsed;
    }

    // Update is called once per frame
    private void Update()
    {
        if (this.remainingSeconds == 0)
        {
            Time.timeScale = 0;
            this.gameOverText.gameObject.SetActive(true);
            this.reloadButton.gameObject.SetActive(true);
            this.timeText.text = "Time's up!";
            this.timeText.color = Color.red;
            this.elapsedTimeText.gameObject.SetActive(true);
            this.elapsedTimeText.text = string.Format("Elapsed Time {0:00}:{1:00}", (Time.timeSinceLevelLoad / 60) % 60, Time.timeSinceLevelLoad % 60);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Time.timeScale = 1 - Time.timeScale;
            this.timeText.text = string.Format("{0:00}:{1:00}", (this.remainingSeconds / 60) % 60, this.remainingSeconds % 60);
            this.timeUpdateText.gameObject.SetActive(this.timeUpdateTimer.Interval > 100);
        }

        this.gamePaused = Time.timeScale == 0;
    }

    private void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        this.UpdateTimeBy(-1);
    }

    private void timeUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        this.timeUpdateTimer.Stop();
        this.timeUpdateTimer.Interval = 100;
    }
}
