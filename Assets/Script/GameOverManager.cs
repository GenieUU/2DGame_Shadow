using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TimerManager timerManager;
    public ScoreManager scoreManager;

    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI finalScoreText;

    public void TriggerGameOver()
    {
        // 시간, 점수 업데이트
        if (timerManager != null)
        {
            timerManager.StopTimer();
            string timeString = timerManager.GetFormattedTime();
            finalTimeText.text = "Time: " + timeString;
        }

        if (scoreManager != null)
        {
            int score = scoreManager.GetScore();
            finalScoreText.text = "Score: " + score.ToString();
        }

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }
}