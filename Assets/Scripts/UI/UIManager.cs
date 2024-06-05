using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{

    [Header("Menu UI")]
    [SerializeField]
    private Button Home_button;
    [SerializeField]
    private Button Menu_Button;
    [SerializeField]
    private GameObject Menu_Object;
    [SerializeField]
    private RectTransform Menu_RT;

    [SerializeField]
    private Button About_Button;
    [SerializeField]
    private GameObject About_Object;
    [SerializeField]
    private RectTransform About_RT;

    [SerializeField]
    private Button Settings_Button;
    [SerializeField]
    private GameObject Settings_Object;
    [SerializeField]
    private RectTransform Settings_RT;

    [SerializeField]
    private Button Exit_Button;
    [SerializeField]
    private GameObject Exit_Object;
    [SerializeField]
    private RectTransform Exit_RT;

    [SerializeField]
    private Button Paytable_Button;
    [SerializeField]
    private GameObject Paytable_Object;
    [SerializeField]
    private RectTransform Paytable_RT;

    [SerializeField] private Button home;
    [Header("Popus UI")]
    [SerializeField]
    private GameObject MainPopup_Object;

    [Header("About Popup")]
    [SerializeField]
    private GameObject AboutPopup_Object;
    [SerializeField]
    private Button AboutExit_Button;

    [Header("Paytable Popup")]
    [SerializeField]
    private Button Info_button;
    [SerializeField]
    private GameObject PaytablePopup_Object;
    [SerializeField]
    private Button PaytableExit_Button;
    [SerializeField]
    private TMP_Text[] SymbolsText;

    [Header("Settings Popup")]
    [SerializeField]
    private GameObject SettingsPopup_Object;
    [SerializeField]
    private Button SettingsExit_Button;
    [SerializeField]
    private Button Sound_Button;
    [SerializeField]
    private Button Music_Button;
    [SerializeField]
    private GameObject MusicOn_Object;
    [SerializeField]
    private GameObject MusicOff_Object;
    [SerializeField]
    private GameObject SoundOn_Object;
    [SerializeField]
    private GameObject SoundOff_Object;

    [Header("Megawin Popup")]
    [SerializeField] private GameObject megawin;
    [SerializeField] private TMP_Text megawin_text;

    [Header("Freespin Popup")]
    [SerializeField] private GameObject freeSpin;
    [SerializeField] private TMP_Text freeSpin_text;
    [SerializeField] private Button freeSpin_button;



    [SerializeField] private AudioController audioController;
    [SerializeField] private SlotBehaviour slotBehaviour;

    private bool isMusic = true;
    private bool isSound = true;

    private int FreeSpins;

    private void Start()
    {


        if (Info_button) Info_button.onClick.RemoveAllListeners();
        if (Info_button) Info_button.onClick.AddListener(delegate { OpenPopup(PaytablePopup_Object); });

        if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
        if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate { ClosePopup(PaytablePopup_Object); });

        if (MusicOn_Object) MusicOn_Object.SetActive(true);
        if (MusicOff_Object) MusicOff_Object.SetActive(false);

        if (SoundOn_Object) SoundOn_Object.SetActive(true);
        if (SoundOff_Object) SoundOff_Object.SetActive(false);

        if (audioController) audioController.ToggleMute(false);


        isMusic = true;
        isSound = true;

        if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onClick.AddListener(ToggleSound);

        if (Music_Button) Music_Button.onClick.RemoveAllListeners();
        if (Music_Button) Music_Button.onClick.AddListener(ToggleMusic);

        if (home) home.onClick.RemoveAllListeners();
        if (home) home.onClick.AddListener(CallOnExitFunction);

        if (freeSpin_button) freeSpin_button.onClick.AddListener(delegate { StartFreeSpins(FreeSpins); });

    }

    void ReturnToHome() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    internal void InitialiseUIData(string SupportUrl, string AbtImgUrl, string TermsUrl, string PrivacyUrl, Paylines symbolsText)
    {
        PopulateSymbolsPayout(symbolsText);
    }

    private void PopulateSymbolsPayout(Paylines paylines)
    {
        for (int i = 0; i < paylines.symbols.Count; i++)
        {
            if (i < SymbolsText.Length)
            {
                string text = null;
                if (paylines.symbols[i].multiplier._5x != 0)
                {
                    text += "<color=#FFCD02><b>5</b></color>" + " "+paylines.symbols[i].multiplier._5x;
                }
                if (paylines.symbols[i].multiplier._4x != 0)
                {
                    text += "\n" + "<color=#FFCD02><b>4</b></color>" + " " + paylines.symbols[i].multiplier._4x;
                }
                if (paylines.symbols[i].multiplier._3x != 0)
                {
                    text += "\n" + "<color=#FFCD02><b>3</b></color>" + " " + paylines.symbols[i].multiplier._3x;
                }
                if (paylines.symbols[i].multiplier._2x != 0)
                {
                    text += "\n" + "<color=#FFCD02><b>2</b></color>" + " " + paylines.symbols[i].multiplier._2x;
                }
                if (SymbolsText[i]) SymbolsText[i].text = text;
            }
        }
    }

    private void CallOnExitFunction()
    {
        print("close");
        slotBehaviour.CallCloseSocket();
        Application.ExternalCall("window.parent.postMessage", "onExit", "*");
    }

    internal void PopulateWin(double amount)
    {
        double initAmount = 0;
        double originalAmount = amount;
        if (megawin) megawin.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        DOTween.To(() => initAmount, (val) => initAmount = val, amount, 1f).OnUpdate(() =>
        {
            if (megawin_text) megawin_text.text = initAmount.ToString("0.000");
        });

        DOVirtual.DelayedCall(3.5f, () =>
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
            if (megawin) megawin.SetActive(false);
            if (megawin_text) megawin_text.text="0";
            slotBehaviour.CheckPopups = false;

        });
    }

    private void StartFreeSpins(int spins)
    {
        if (MainPopup_Object) MainPopup_Object.SetActive(false);
        if (freeSpin) freeSpin.SetActive(false);
        slotBehaviour.FreeSpin(spins);
    }

    internal void FreeSpinProcess(int spins)
    {
        FreeSpins = spins;
        if (freeSpin) freeSpin.SetActive(true);
        if (freeSpin_text) freeSpin_text.text += "\n"+spins.ToString();
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void OpenMenu()
    {
        if (Menu_Object) Menu_Object.SetActive(false);
        if (Exit_Object) Exit_Object.SetActive(true);
        if (About_Object) About_Object.SetActive(true);
        if (Paytable_Object) Paytable_Object.SetActive(true);
        if (Settings_Object) Settings_Object.SetActive(true);

        DOTween.To(() => About_RT.anchoredPosition, (val) => About_RT.anchoredPosition = val, new Vector2(About_RT.anchoredPosition.x, About_RT.anchoredPosition.y + 150), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(About_RT);
        });

        DOTween.To(() => Paytable_RT.anchoredPosition, (val) => Paytable_RT.anchoredPosition = val, new Vector2(Paytable_RT.anchoredPosition.x, Paytable_RT.anchoredPosition.y + 300), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Paytable_RT);
        });

        DOTween.To(() => Settings_RT.anchoredPosition, (val) => Settings_RT.anchoredPosition = val, new Vector2(Settings_RT.anchoredPosition.x, Settings_RT.anchoredPosition.y + 450), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_RT);
        });
    }

    private void CloseMenu()
    {

        DOTween.To(() => About_RT.anchoredPosition, (val) => About_RT.anchoredPosition = val, new Vector2(About_RT.anchoredPosition.x, About_RT.anchoredPosition.y - 150), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(About_RT);
        });

        DOTween.To(() => Paytable_RT.anchoredPosition, (val) => Paytable_RT.anchoredPosition = val, new Vector2(Paytable_RT.anchoredPosition.x, Paytable_RT.anchoredPosition.y - 300), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Paytable_RT);
        });

        DOTween.To(() => Settings_RT.anchoredPosition, (val) => Settings_RT.anchoredPosition = val, new Vector2(Settings_RT.anchoredPosition.x, Settings_RT.anchoredPosition.y - 450), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_RT);
        });

        DOVirtual.DelayedCall(0.1f, () =>
         {
             if (Menu_Object) Menu_Object.SetActive(true);
             if (Exit_Object) Exit_Object.SetActive(false);
             if (About_Object) About_Object.SetActive(false);
             if (Paytable_Object) Paytable_Object.SetActive(false);
             if (Settings_Object) Settings_Object.SetActive(false);
         });
    }

    private void OpenPopup(GameObject Popup)
    {
        if (audioController)audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {
        if (audioController)audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (MainPopup_Object) MainPopup_Object.SetActive(false);
    }

    private void ToggleMusic()
    {
        isMusic = !isMusic;
        if(isMusic)
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(true);
            if (MusicOff_Object) MusicOff_Object.SetActive(false);
            if(audioController) audioController.ToggleMute(false, "bg");
        }
        else
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(false);
            if (MusicOff_Object) MusicOff_Object.SetActive(true);
            if (audioController) audioController.ToggleMute(true, "bg");

        }
    }

    private void ToggleSound()
    {
        isSound = !isSound;
        if(isSound)
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(true);
            if (SoundOff_Object) SoundOff_Object.SetActive(false);
            if (audioController) audioController.ToggleMute(false, "wl");
            if (audioController) audioController.ToggleMute(false, "button");

        }
        else
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(false);
            if (SoundOff_Object) SoundOff_Object.SetActive(true);
            if (audioController) audioController.ToggleMute(true, "wl");
            if (audioController) audioController.ToggleMute(true, "button");
        }
    }
}
