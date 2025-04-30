using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public Transform playerTransform;
    public TextMeshProUGUI scoreText;

    private float startX;
    private int score;

    void Start()
    {
        startX = playerTransform.position.x;
    }

    void Update()
    {
        float distance = playerTransform.position.x - startX;
        score = Mathf.FloorToInt(distance / 5f); // 5 유닛 이동할 때 점수 1

        score = Mathf.Max(score, 0); // 음수 방지
        scoreText.text = "Score: " + score.ToString();
    }

    public int GetScore()
    {
        return score;
    }
}