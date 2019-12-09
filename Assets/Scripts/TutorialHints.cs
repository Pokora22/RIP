using System;
using System.Collections;
using System.Collections.Generic;
using Sisus;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TutorialHints : MonoBehaviour
{
    private int hintIndex;

    [SerializeField] private GameObject[] hintPanels;
    private bool[] hintPlayed;
    
    public enum HINT {START, SUMMON, SEND, TIME_LIMIT, HURT, INVENTORY}

    private void Start()
    {
        hintPlayed = new bool[hintPanels.Length];
    }

    public void ShowHint(HINT hint)
    {
        int hintIndex = (int) hint;
        if (hintPlayed[hintIndex])
            return;
        
        DisableOtherHints();        
        
        hintPanels[hintIndex].SetActive(true);
        StartCoroutine(delayedAction(() => {hintPanels[hintIndex].SetActive(false); },
            hintPanels[hintIndex].GetComponentInChildren<TextMeshProUGUI>().text.Length / 15));
    }

    private void DisableOtherHints()
    {
        foreach (GameObject o in hintPanels)        
            o.SetActive(false);        
    }
    
    private IEnumerator delayedAction(UnityAction action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke(); // execute a delegate
    }
    
}
