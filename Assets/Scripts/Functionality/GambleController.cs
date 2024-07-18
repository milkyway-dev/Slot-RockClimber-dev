using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GambleController : MonoBehaviour
{
    [SerializeField] private GameObject gamble_game;
    [SerializeField] private Button doubleButton;
    [SerializeField] private SocketIOManager socketManager;
    [SerializeField] private AudioController audioController;
    [SerializeField] internal List<CardFlip> allcards = new List<CardFlip>();
    [SerializeField] private TMP_Text winamount;
    [SerializeField] private SlotBehaviour slotController;
    [SerializeField] private Sprite[] cardSpriteList;
    [SerializeField] private Sprite cardCover;
    [SerializeField] internal List<Sprite> tempSpriteList = new List<Sprite>();
    [SerializeField] private Image DealerCard;

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image slider;

    [SerializeField] private ImageAnimation tabloAnimation;


    internal bool gambleStart = false;

    private void Start()
    {
        if (doubleButton) doubleButton.onClick.RemoveAllListeners();
        if (doubleButton) doubleButton.onClick.AddListener(StartGamblegame);
        toggleDoubleButton(false);
    }

    internal void toggleDoubleButton(bool toggle) {

        doubleButton.interactable = toggle;
    }

    void StartGamblegame()
    {
        winamount.text = "0";
        if (audioController) audioController.PlayButtonAudio();
        if (gamble_game) gamble_game.SetActive(true);
        PickRandomCard();
        loadingScreen.SetActive(true);
        StartCoroutine(loadingRoutine());
        StartCoroutine(GambleCoroutine());
    }

    IEnumerator GambleCoroutine()
    {
        for (int i = 0; i < allcards.Count; i++)
        {
            allcards[i].once = false;
        }

        socketManager.OnGamble();

        yield return new WaitUntil(() => socketManager.isResultdone);
        gambleStart = true;
        slotController.updateBalance();
        int maxIndex = tempSpriteList.Count - 1;
        int minIndex = 0;
        if (socketManager.gambleData.totalWinningAmount > 0)
        {
            DealerCard.sprite = tempSpriteList[minIndex];
            foreach (var item in allcards)
            {
                item.cardImage = tempSpriteList[maxIndex];
            }
        }
        else
        {
            DealerCard.sprite = tempSpriteList[maxIndex];
            foreach (var item in allcards)
            {
                item.cardImage = tempSpriteList[minIndex];
            }

        }

    }

    internal void FlipAllCard()
    {
        List<CardFlip> tempCardList = new List<CardFlip>();
        foreach (var item in allcards)
        {
            if (item.once)
                continue;
            else
            {
                item.Card_Button.interactable = false;
                tempCardList.Add(item);
            }
        }
        if(socketManager.gambleData.totalWinningAmount>0)
        winamount.text = "YOU WIN"+ "\n"+socketManager.gambleData.totalWinningAmount.ToString();
        else
            winamount.text = "YOU LOSE" + "\n" + "0";

        StartCoroutine(Collectroutine());

    }


    IEnumerator Collectroutine()
    {
        yield return new WaitForSeconds(2f);
        gambleStart = false;
        socketManager.OnCollect();
        yield return new WaitUntil(() => socketManager.isResultdone);
        slotController.updateBalance();
        if (gamble_game) gamble_game.SetActive(false);
        tempSpriteList.Clear();
        allcards.ForEach((element) =>
        {
            element.Card_Button.image.sprite = cardCover;
            element.Reset();

        });
        tabloAnimation.StopAnimation();
        toggleDoubleButton(false);

    }

    void OnGameOver()
    {
        StartCoroutine(Collectroutine());

    }

    void PickRandomCard()
    {
        int maxlength = cardSpriteList.Length / 2;

        int lowCardIndex = Random.Range(0, maxlength);

        int maxCardIndex = Random.Range(maxlength, cardSpriteList.Length);

        tempSpriteList.Add(cardSpriteList[lowCardIndex]);
        tempSpriteList.Add(cardSpriteList[maxCardIndex]);
    }

    IEnumerator loadingRoutine()
    {

        float fillAmount = 0;
        while (fillAmount<0.9) {
            fillAmount += Time.deltaTime;
            slider.fillAmount = fillAmount;
            if (fillAmount == 0.9) yield break;
            yield return null;
        }
        yield return new WaitUntil(() => gambleStart);
        slider.fillAmount = 1;
        yield return new WaitForSeconds(1f);
        loadingScreen.SetActive(false);
        tabloAnimation.StartAnimation();
    }

    internal void CardFlipSound()
    {
        if (audioController) audioController.PlayBonusAudio("card");
    }

}
