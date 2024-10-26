using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;
    [SerializeField]
    private List<SlotImage> Tempimages;

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;


    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<GameObject> StaticLine_Objects;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text LineBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;

    [Header("Games buttongroup UI")]
    [SerializeField]
    private GameObject Your_Win_GameObject;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField]
    private Button AutoSpinStop_Button;
    [SerializeField]
    private Sprite AutoSpinHover_Sprite;
    [SerializeField]
    private Sprite AutoSpin_Sprite;
    [SerializeField]
    private Image AutoSpin_Image;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button Betone_button;
    [SerializeField]
    private Button Double_button;

    int tweenHeight = 0;

    [SerializeField]
    private GameObject Image_Prefab;


    [SerializeField]
    private PayoutCalculation PayCalculator;

    [SerializeField]
    private List<Tweener> alltweens = new List<Tweener>();

    [SerializeField]
    private List<ImageAnimation> TempList;

    [SerializeField]
    private int IconSizeFactor = 100;
    [SerializeField] private int SpaceFactor = 0;

    private int numberOfSlots = 5;

    [SerializeField]
    int verticalVisibility = 3;

    [SerializeField]
    private SocketIOManager SocketManager;
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private BonusController bonusController;
    [SerializeField] private GambleController gambleController;

    Coroutine AutoSpinRoutine = null;
    Coroutine tweenroutine=null;
    Coroutine FreeSpinRoutine = null;

    //Tweeners
    private Tweener Win_Tween = null;

    protected int Lines = 9;

    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;
    private int BetCounter = 0;
    private double currentBalance = 0;
    internal double currentBet = 0;
    private double currentTotalBet = 0;



    private void Start()
    {
        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (Betone_button) Betone_button.onClick.RemoveAllListeners();
        if (Betone_button) Betone_button.onClick.AddListener(OnBetOne);

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        tweenHeight = (9 * IconSizeFactor) - 280;
        TriggerWinImageAnimation(false);
    }

    private void AutoSpin()
    {
        if (audioController) audioController.PlaySpinButtonAudio();
        if (!IsAutoSpin)
        {
            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
        }
    }

    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
        PayCalculator.LineList = SocketManager.initialData.LinesCount;
    }

    private void StopAutoSpin()
    {
        if (audioController) audioController.PlaySpinButtonAudio();
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {

        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {

            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));

        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            i++;
        }
        ToggleButtonGrp(true);
        IsFreeSpin = false;
    }

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            //if (AutoSpin_Button) AutoSpin_Button.interactable = false;
            //if (SlotStart_Button) SlotStart_Button.interactable = false;
        }
        //else
        //{
        //    if (AutoSpin_Button) AutoSpin_Button.interactable = true;
        //    if (SlotStart_Button) SlotStart_Button.interactable = true;
        //}
    }

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    void OnBetOne()
    {
        if (audioController) audioController.PlayButtonAudio();

        if (BetCounter < SocketManager.initialData.Bets.Count - 1)
        {
            BetCounter++;
        }
        else
        {
            BetCounter = 0;
        }
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
    //    {
    //        StartSlots();
    //    }
    //}

    //internal void PopulateInitalSlots(int number, List<int> myvalues)
    //{

    //    PopulateSlot(myvalues, number);
    //}

    internal void LayoutReset(int number)
    {
        if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }

    //private void PopulateSlot(List<int> values, int number)
    //{
    //    if (Slot_Objects[number]) Slot_Objects[number].SetActive(true);
    //    for (int i = 0; i < values.Count; i++)
    //    {
    //        if (values[i] < myImages.Length)
    //        {
    //            GameObject myImg;
    //            myImg = Instantiate(Image_Prefab, Slot_Transform[number]);
    //            images[number].slotImages.Add(myImg.transform.GetComponent<Image>());
    //            Debug.Log(string.Concat("<color=red><b>", i.ToString(), "</b></color>"));
    //            images[number].slotImages[i].sprite = myImages[values[i]];
    //        }
    //    }
    //    //for (int k = 0; k < 2; k++)
    //    //{
    //    //    GameObject mylastImg = Instantiate(Image_Prefab, Slot_Transform[number]);
    //    //    images[number].slotImages.Add(mylastImg.transform.GetComponent<Image>());
    //    //    images[number].slotImages[images[number].slotImages.Count - 1].sprite = myImages[values[k]];
    //    //}
    //    if (mainContainer_RT) LayoutRebuilder.ForceRebuildLayoutImmediate(mainContainer_RT);
    //    tweenHeight = (values.Count * IconSizeFactor) - 280;
    //    GenerateMatrix(number);
    //}

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();
        if (gambleController) gambleController.toggleDoubleButton(false);
        gambleController.GambleTweeningAnim(false);
        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }

        }
        PayCalculator.DontDestroyLines.Clear();

        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetStaticLine();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    private IEnumerator TweenRoutine()
    {
        gambleController.GambleTweeningAnim(false);
        TriggerWinImageAnimation(false);
        currentBet = SocketManager.initialData.Bets[BetCounter];

        if (currentBalance < currentTotalBet && !IsFreeSpin)
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        if (audioController) audioController.PlayWLAudio("spin");
        CheckSpinAudio = true;
        IsSpinning = true;
        ToggleButtonGrp(false);
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        if (!IsFreeSpin)
        {
            double bet = 0;
            double balance = 0;
            try
            {
                bet = double.Parse(TotalBet_text.text);
            }
            catch (Exception e)
            {
                Debug.Log("Error while conversion " + e.Message);
            }

            try
            {
                balance = double.Parse(Balance_text.text);
            }
            catch (Exception e)
            {
                Debug.Log("Error while conversion " + e.Message);
            }
            double initAmount = balance;

            balance = balance - bet;

            DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
            {
                if (Balance_text) Balance_text.text = initAmount.ToString("f2");
            });
        }
        SocketManager.AccumulateResult(BetCounter);
        print("before result");
        yield return new WaitUntil(() => SocketManager.isResultdone);

        for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        {
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {
                if (images[i].slotImages[images[i].slotImages.Count - 5 + j])
                    images[i].slotImages[images[i].slotImages.Count - 5 + j].sprite = myImages[resultnum[i]];
            }
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i);
        }

        yield return new WaitForSeconds(0.3f);

        CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot);
        KillAllTweens();
        currentBalance = SocketManager.playerdata.Balance;

        CheckPopups = true;

        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f2");
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        if (SocketManager.resultData.isBonus)
        {
            CheckBonusGame();
        }
        else
        {
            CheckWinPopups();
        }

        yield return new WaitUntil(() => !CheckPopups);
        if (!IsAutoSpin)
        {
            ActivateGamble();
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            ActivateGamble();
            yield return new WaitForSeconds(2f);
            IsSpinning = false;
        }
        //if (SocketManager.resultData.freeSpins > 0  && !IsFreeSpin)
        //{
        //    uiManager.FreeSpinProcess((int)SocketManager.resultData.freeSpins);
        //}
    }

    private void ActivateGamble()
    {
        if (SocketManager.playerdata.currentWining > 0 && SocketManager.playerdata.currentWining <= SocketManager.GambleLimit)
        {
            gambleController.GambleTweeningAnim(true);
            gambleController.toggleDoubleButton(true);
            IsSpinning = false;
        }
    }

    internal void DeactivateGamble()
    {
        StopAutoSpin();
    }

    internal void GambleCollect()
    {
        SocketManager.GambleCollectCall();
    }

    internal void CheckWinPopups()
    {
        if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        }
        else
        {
            CheckPopups = false;
        }
    }

    internal void CheckBonusGame()
    {
        bonusController.maxBreakCount = SocketManager.resultData.BonusResult.Count;
        bonusController.StartBonus(SocketManager.resultData.BonusResult);
    }

    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (Betone_button) Betone_button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        //if (Double_button) Double_button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;

    }

    internal void updateBalance()
    {
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f2");
    }

    private void StartGameAnimation(GameObject animObjects)
    {
        //if (animObjects.transform.GetComponent<ImageAnimation>().isActiveAndEnabled)
        //{

            animObjects.transform.GetChild(0).gameObject.SetActive(true);
            animObjects.transform.GetChild(1).gameObject.SetActive(true);
        //}

        ImageAnimation temp = animObjects.transform.GetChild(0).GetComponent<ImageAnimation>();

        temp.StartAnimation();
        TempList.Add(temp);
    }

    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
            if (TempList[i].transform.parent.childCount > 0)
            {
                TempList[i].transform.parent.GetChild(0).gameObject.SetActive(false);
                TempList[i].transform.parent.GetChild(1).gameObject.SetActive(false);
            }
        }
        TempList.Clear();
        TempList.TrimExcess();
    }

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        List<int> points_anim = null;
        if (LineId.Count>0)
        {
            if (audioController) audioController.PlayWLAudio("win");

            TriggerWinImageAnimation(true);

            for (int i = 0; i < LineId.Count; i++)
            {
                PayCalculator.DontDestroyLines.Add(LineId[i]);
                PayCalculator.GeneratePayoutLinesBackend(LineId[i]);
            }

            if (jackpot > 0)
            {
                for (int i = 0; i < Tempimages.Count; i++)
                {
                    for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
                    {
                        StartGameAnimation(Tempimages[i].slotImages[k].gameObject);
                    }
                }
            }
            else
            {
                for (int i = 0; i < points_AnimString.Count; i++)
                {
                    points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                    for (int k = 0; k < points_anim.Count; k++)
                    {
                        print(points_anim.Count);
                        if (points_anim[k] >= 10)
                        {
                            StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
                        }
                        else
                        {
                            StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
                        }
                    }
                }
            }
        }
        else
        {

            if (audioController) audioController.StopWLAaudio();
        }

        CheckSpinAudio = false;
    }

    private void GenerateMatrix(int value)
    {
        for (int j = 0; j < 3; j++)
        {
            Tempimages[value].slotImages.Add(images[value].slotImages[images[value].slotImages.Count - 5 + j]);
        }
    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }

    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        int tweenpos = (reqpos * (IconSizeFactor + SpaceFactor)) - (IconSizeFactor + (2 * SpaceFactor));
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (SpaceFactor > 0 ? SpaceFactor / 4 : 0), 0.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
    }

    private void TriggerWinImageAnimation(bool _config_tf)
    {
        if(_config_tf)
        {
            Your_Win_GameObject.GetComponent<ImageAnimation>().StartAnimation();
        }
        else
        {
            Your_Win_GameObject.GetComponent<ImageAnimation>().StopAnimation();
        }
    }

    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();
    }
    #endregion
}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}


