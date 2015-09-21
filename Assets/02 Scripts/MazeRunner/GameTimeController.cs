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
    private Timer timer;
    private Timer timeUpdateTimer;
    private bool gamePaused;
    private MainMenuArcadeController mainMenuController;

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

    // Use this for initialization
    private void Start()
    {
        Time.timeScale = 1;
        this.remainingSeconds = 60;
        this.inGameUI = GameObject.Find("InGameUI").transform;
        this.mainMenuController = this.inGameUI.GetComponent<MainMenuArcadeController>();
        this.timeText = this.inGameUI.GetChild(0).GetComponent<Text>();
        this.timeUpdateText = this.inGameUI.GetChild(1).GetComponent<Text>();
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
            this.mainMenuController.GameOver();
            this.enabled = false;
        }
        else
        {
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
