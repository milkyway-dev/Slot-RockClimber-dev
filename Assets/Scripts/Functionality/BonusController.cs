using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BonusController : MonoBehaviour
{
    [SerializeField] private GameObject bonus_game;
    [SerializeField] private SlotBehaviour slotManager;
    [SerializeField] internal GameObject raycastPanel;
    [SerializeField] private List<BonusBreakGem> gems;
    [SerializeField] private AudioController _audioManager;

    [SerializeField] private List<int> gemValues;

    [SerializeField] internal int currentBreakCount = 0;
    [SerializeField] internal int maxBreakCount = 0;
 

    internal double OnBreakGem()
    {
        currentBreakCount++;
        return gemValues[currentBreakCount - 1];
    }

    internal void GameOver()
    {
        currentBreakCount = 0;
        maxBreakCount = 0;
        Invoke("OnGameOver", 1.5f);
    }

    void OnGameOver()
    {
        slotManager.CheckPopups = false;
        _audioManager.SwitchBGSound(false);
        if (bonus_game) bonus_game.SetActive(false);
    }

    internal void StartBonus(List<int> values)
    {
       
        gemValues.Clear();
        gemValues.TrimExcess();
        gemValues = values;

        for (int i = 0; i < gemValues.Count; i++)
        {
            if (gemValues[i] == -1)
            {
                gemValues.RemoveAt(i);
                gemValues.Add(-1);
            }
        }

        _audioManager.SwitchBGSound(true);
        if (bonus_game) bonus_game.SetActive(true);
    }

    internal void PlayWinSound()
    {
        if (_audioManager) _audioManager.PlayBonusAudio("win");
    }

    internal void PlayLoseSound()
    {
        if (_audioManager) _audioManager.PlayBonusAudio("lose");
    }
}
