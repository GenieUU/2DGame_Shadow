using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameClearManager : MonoBehaviour
{
    public GameObject gameClearPanel;

    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI rankingText;


    public TimerManager timerManager;
    public ScoreManager scoreManager;

    public Animator goalDoorAnimator;
    public float doorOpenDelay = 1.5f; // 문 열리는 시간 만큼 설정

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HandleGoalSequence(other.gameObject));
        }
    }

    IEnumerator HandleGoalSequence(GameObject player)
    {
        // 문 열기 애니메이션 재생
        if (goalDoorAnimator != null)
        {
            goalDoorAnimator.SetTrigger("Open");
        }

        // 애니메이션 재생 시간만큼 대기
        yield return new WaitForSeconds(doorOpenDelay);

        // GameClear 처리
        ShowGameClear(player);
    }

    public void ShowGameClear(GameObject player)
    {
        // Timer 멈춤
        if (timerManager != null)
        {
            timerManager.StopTimer();
            finalTimeText.text = "Time: " + timerManager.GetFormattedTime();
        }

        // Score 표시
        if (scoreManager != null)
        {
            int score = scoreManager.GetScore();
            finalScoreText.text = "Score: " + score.ToString();
        }

        // 플레이어 조작 제한
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
            player.layer = 13;
        }

        // GameClear UI 표시
        gameClearPanel.SetActive(true);
        Time.timeScale = 0f;

        if (timerManager != null)
        {
            timerManager.StopTimer();
            float clearTime = timerManager.GetElapsedTime();
            finalTimeText.text = "Time: " + timerManager.GetFormattedTime();

            SaveAndDisplayRanking(clearTime);
            ShowRanking();
        }

    }

    // Restart 버튼
    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene"); // 시작 씬으로 이동
    }

    // 랭킹기능 추가
    void SaveAndDisplayRanking(float clearTime)
    {
        // 기존 랭킹 불러오기
        List<float> rankings = new List<float>();

        for (int i = 0; i < 3; i++)
        {
            float time = PlayerPrefs.GetFloat("Ranking" + i, -1f);
            if (time >= 0f)
            {
                rankings.Add(time);
            }
        }

        // 새로운 시간 추가 후 정렬
        rankings.Add(clearTime);
        rankings.Sort(); // 낮을수록 높은 순위 (빠른 시간)

        // 상위 3개까지만 저장
        for (int i = 0; i < 3; i++)
        {
            if (i < rankings.Count)
                PlayerPrefs.SetFloat("Ranking" + i, rankings[i]);
            else
                PlayerPrefs.DeleteKey("Ranking" + i); // 남은 건 삭제
        }

        PlayerPrefs.Save();
    }

    void ShowRanking()
    {
        string result = "";

        for (int i = 0; i < 3; i++)
        {
            float time = PlayerPrefs.GetFloat("Ranking" + i, -1f);
            if (time >= 0)
            {
                int min = Mathf.FloorToInt(time / 60f);
                int sec = Mathf.FloorToInt(time % 60f);
                result += $"{i + 1}.  {min:00}:{sec:00}\n"; //

            }
            else
            {
                result += $"{i + 1}. : --:--\n";
            }
        }
        if (rankingText != null)
        {
            rankingText.text = result;
        }
    }
   
    // 랭킹 리셋용
    [UnityEditor.MenuItem("Tools/Reset Ranking (PlayerPrefs)")]
    private static void ResetRankingEditor()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.DeleteKey("Ranking" + i);
        }

        PlayerPrefs.Save();
        Debug.Log(" 랭킹 초기화 완료 (에디터 메뉴에서 실행됨)");
    }
}