using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DifficultyHandler_scr : MonoBehaviour
{
    private int difficulty;
    private string[] diffNames = new[] {"Easy", "Normal", "Hard"};
    private TextMeshProUGUI TMPro;

    private void Start()
    {
        TMPro = GetComponentInChildren<TextMeshProUGUI>();
        difficulty = PlayerPrefs.GetInt("difficulty");
        TMPro.text = diffNames[difficulty];
    }

    public void changeDiff()
    {
        difficulty = (difficulty + 1) % 3;
        TMPro.text = diffNames[difficulty];
        PlayerPrefs.SetInt("difficulty", difficulty);
    }
}
