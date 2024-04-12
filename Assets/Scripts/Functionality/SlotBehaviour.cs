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
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> Tempimages;     //class to store the result matrix

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<GameObject> StaticLine_Objects;

    [Header("Line Button Texts")]
    [SerializeField]
    private List<TMP_Text> StaticLine_Texts;

    private Dictionary<int, string> x_string = new Dictionary<int, string>();
    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;

    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] Bonus_Sprite;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text Lines_text;
    [SerializeField]
    private TMP_Text TotalWin_text;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField]
    private Sprite AutoSpinHover_Sprite;
    [SerializeField]
    private Sprite AutoSpin_Sprite;
    [SerializeField]
    private Image AutoSpin_Image;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button BetPlus_Button;
    [SerializeField]
    private Button BetMinus_Button;
    [SerializeField]
    private Button Line_Button;
    [SerializeField]
    private List<GameObject> activeLineImage;
    [SerializeField]
    private List<GameObject> inactiveLineimage;

    //[SerializeField]
    //private Button LinePlus_Button;
    //[SerializeField]
    //private Button LineMinus_Button;

    [Header("Audio Management")]
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _spinSound;
    [SerializeField]
    private AudioClip _lossSound;
    [SerializeField]
    private AudioClip[] _winSounds;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;
    [SerializeField]
    private GameObject Bonus_Prefab;    //icons prefab

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();


    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing

    private int numberOfSlots = 5;          //number of columns

    [SerializeField]
    int verticalVisibility = 3;

    [SerializeField]
    private SocketIOManager SocketManager;
    Coroutine AutoSpinRoutine = null;
    bool IsAutoSpin = false;

    [Header("Static Line Management")]
    [SerializeField]
    private List<int> LineList;
    [SerializeField]
    private List<GameObject> LineObjetcs;

    internal int CurrentLines;
    private int LineIndex;


    private void Start()
    {
        CurrentLines = LineList[LineList.Count - 1];
        LineIndex = LineList.Count - 1;
        inactiveLineimage[LineIndex].SetActive(false);
        activeLineImage[LineIndex].SetActive(true);
        if (Lines_text) Lines_text.text = CurrentLines.ToString();

        IsAutoSpin = false;
        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        //if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        //if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
        //if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        //if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        //if (LinePlus_Button) LinePlus_Button.onClick.RemoveAllListeners();
        //if (LinePlus_Button) LinePlus_Button.onClick.AddListener(delegate { ChangeLine(true); });
        //if (LineMinus_Button) LineMinus_Button.onClick.RemoveAllListeners();
        //if (LineMinus_Button) LineMinus_Button.onClick.AddListener(delegate { ChangeLine(false); });

        if (Line_Button) Line_Button.onClick.RemoveAllListeners();
        if (Line_Button) Line_Button.onClick.AddListener(delegate { ChangeLine(); });


        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);
    }

    private void AutoSpin()
    {
        IsAutoSpin = !IsAutoSpin;
        if (IsAutoSpin)
        {
            if (AutoSpin_Image) AutoSpin_Image.sprite = AutoSpinHover_Sprite;
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
        }
        else
        {
            if (AutoSpin_Image) AutoSpin_Image.sprite = AutoSpin_Sprite;
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (true)
        {
            StartSlots(true);
            yield return new WaitForSeconds(10);
        }
    }

    //Fetch Lines from backend
    internal void FetchLines(string x_value, string y_value, int LineID, int count)
    {
        x_string.Add(LineID, x_value);
        y_string.Add(LineID, y_value);
        StaticLine_Texts[count].text = LineID.ToString();
        StaticLine_Objects[count].SetActive(true);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(int index = -1)
    {

        //DestroyStaticLine();
        if (index >= 0)
        {
            LineObjetcs[index - 1].SetActive(true);
            print(LineObjetcs[index - 1].name);
            return;
        }
        for (int i = 0; i < CurrentLines; i++)
        {
            LineObjetcs[i].SetActive(true);


        }


    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {

        for (int i = 0; i < LineObjetcs.Count; i++)
        {
            LineObjetcs[i].SetActive(false);
        }

    }

    private void MaxBet()
    {
        if (TotalBet_text) TotalBet_text.text = "99999";
    }


    private void ChangeLine()
    {
        LineIndex++;

        if (LineIndex >= LineList.Count)
            LineIndex = 0;

        CurrentLines = LineList[LineIndex];


        if (Lines_text) Lines_text.text = CurrentLines.ToString();
        GenerateStaticLine();

        for (int i = 0; i < LineList.Count; i++)
        {
            activeLineImage[i].SetActive(false);
            inactiveLineimage[i].SetActive(true);
        }

        int j = LineList.IndexOf(CurrentLines);
        StartCoroutine(Set_button_state(activeLineImage[j], inactiveLineimage[j], 0.1f));

    }

    IEnumerator Set_button_state(GameObject button_to_active, GameObject button_to_inactive, float time)
    {
        button_to_inactive.SetActive(false);
        yield return new WaitForSeconds(time);
        button_to_active.SetActive(true);

    }


    private void ChangeBet(bool IncDec)
    {
        double currentbet = 0;
        try
        {
            currentbet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("parse error " + e);
        }
        if (IncDec)
        {
            if (currentbet < 99999)
            {
                currentbet += 100;
            }
            else
            {
                currentbet = 99999;
            }

            if (currentbet > 99999)
            {
                currentbet = 99999;
            }
        }
        else
        {
            if (currentbet > 0)
            {
                currentbet -= 100;
            }
            else
            {
                currentbet = 0;
            }

            if (currentbet < 0)
            {
                currentbet = 0;
            }
        }

        if (TotalBet_text) TotalBet_text.text = currentbet.ToString();
    }


    //just for testing purposes delete on production
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
        {
            StartSlots();
        }
    }

    //populate the slots with the values recieved from backend
    internal void PopulateInitalSlots(int number, List<int> myvalues)
    {
        PopulateSlot(myvalues, number);
    }

    //reset the layout after populating the slots
    internal void LayoutReset(int number)
    {
        if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }

    private void PopulateSlot(List<int> values, int number)
    {

        if (Slot_Objects[number]) Slot_Objects[number].SetActive(true);
        for (int i = 0; i < values.Count; i++)
        {
            GameObject myImg;
            if (values[i] == 0)
            {
                myImg = Instantiate(Bonus_Prefab, Slot_Transform[number]);
                images[number].slotImages.Add(myImg.GetComponent<Image>());
                images[number].slotImages[i].sprite = myImages[values[i]];
                PopulateAnimationSprites(images[number].slotImages[i].GetComponent<ImageAnimation>(), values[i]);
            }
            else {
                myImg = Instantiate(Image_Prefab, Slot_Transform[number]);
                images[number].slotImages.Add(myImg.GetComponent<Image>());
                images[number].slotImages[i].sprite = myImages[values[i]];
            }


        }
        for (int k = 0; k < 2; k++)
        {
            if (values[k] == 0)
            {
                GameObject mylastImg = Instantiate(Bonus_Prefab, Slot_Transform[number]);
                images[number].slotImages.Add(mylastImg.GetComponent<Image>());
                images[number].slotImages[images[number].slotImages.Count - 1].sprite = myImages[values[k]];
                PopulateAnimationSprites(images[number].slotImages[images[number].slotImages.Count - 1].gameObject.GetComponent<ImageAnimation>(), values[k]);
            }
            else {

                GameObject mylastImg = Instantiate(Image_Prefab, Slot_Transform[number]);
                images[number].slotImages.Add(mylastImg.GetComponent<Image>());
                images[number].slotImages[images[number].slotImages.Count - 1].sprite = myImages[values[k]];
            }
           
        }
        if (mainContainer_RT) LayoutRebuilder.ForceRebuildLayoutImmediate(mainContainer_RT);
        tweenHeight = (values.Count * IconSizeFactor) - 280;
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
            for (int i = 0; i < Bonus_Sprite.Length; i++)
            {
                animScript.textureArray.Add(Bonus_Sprite[i]);
            }

    }
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (_audioSource) _audioSource.clip = _spinSound;
        if (_audioSource) _audioSource.loop = true;
        if (_audioSource) _audioSource.Play();
        DestroyStaticLine();
        if (!autoSpin)
        {
            if (AutoSpin_Image) AutoSpin_Image.sprite = AutoSpin_Sprite;
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
        }

        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        StartCoroutine(TweenRoutine());
        for (int i = 0; i < Tempimages.Count; i++)
        {
            Tempimages[i].slotImages.Clear();
            Tempimages[i].slotImages.TrimExcess();
        }
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        SocketManager.AccumulateResult();
        yield return new WaitForSeconds(0.5f);
        List<int> resultnum = SocketManager.tempresult.StopList?.Split(',')?.Select(Int32.Parse)?.ToList();

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(resultnum[i] + 3, Slot_Transform[i], i);
        }

        yield return new WaitForSeconds(0.3f);
        GenerateMatrix(SocketManager.tempresult.StopList);
        CheckPayoutLineBackend(SocketManager.tempresult.resultLine, SocketManager.tempresult.x_animResult, SocketManager.tempresult.y_animResult);
        KillAllTweens();
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        //ImageAnimation temp = animObjects.Gtec.GetComponent<ImageAnimation>();
        int a = animObjects.transform.childCount;

        if (a>0)
        {
            ImageAnimation temp = animObjects.transform.GetChild(0).GetComponent<ImageAnimation>();
            animObjects.transform.GetChild(0).gameObject.SetActive(true);
            animObjects.transform.GetChild(1).gameObject.SetActive(true);
            temp.StartAnimation();
            TempList.Add(temp);
        }
        else {

            animObjects.GetComponent<ImageAnimation>().StartAnimation();
        }

    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
    }

    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> x_AnimString, List<string> y_AnimString)
    {
        List<int> x_points = null;
        List<int> y_points = null;
        List<int> x_anim = null;
        List<int> y_anim = null;
        if (LineId.Count > 0)
        {
            int choice = UnityEngine.Random.Range(0, 2);
            if (_audioSource) _audioSource.Stop();
            if (_audioSource) _audioSource.loop = false;
            if (_audioSource) _audioSource.clip = _winSounds[choice];
            if (_audioSource) _audioSource.Play();

            for (int i = 0; i < LineId.Count; i++)
            {

                GenerateStaticLine(LineId[i]);

                //x_points = x_string[LineId[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
                //y_points = y_string[LineId[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
                //PayCalculator.GeneratePayoutLinesBackend(x_points, y_points, x_points.Count);
            }

            for (int i = 0; i < x_AnimString.Count; i++)
            {
                x_anim = x_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();
                y_anim = y_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                for (int k = 0; k < x_anim.Count; k++)
                {
                    StartGameAnimation(Tempimages[x_anim[k]].slotImages[y_anim[k]].gameObject);
                }
            }
        }
        else
        {
            if (_audioSource) _audioSource.Stop();
            if (_audioSource) _audioSource.loop = false;
            if (_audioSource) _audioSource.clip = _lossSound;
            if (_audioSource) _audioSource.Play();
        }
    }

    //generate the result matrix
    private void GenerateMatrix(string stopList)
    {
        List<int> numbers = stopList?.Split(',')?.Select(Int32.Parse)?.ToList();

        for (int i = 0; i < numbers.Count; i++)
        {
            for (int s = 0; s < verticalVisibility; s++)
            {
                Tempimages[i].slotImages.Add(images[i].slotImages[(images[i].slotImages.Count - (numbers[i] + 3)) + s]);
            }
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
        int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100, 0.5f).SetEase(Ease.OutElastic);

        yield return new WaitForSeconds(0.2f);
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

