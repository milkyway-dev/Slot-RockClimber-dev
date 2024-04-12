using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardFlip : MonoBehaviour
{
    [SerializeField]
    private Sprite cardImage;
    [SerializeField]
    private Button Card_Button;

    private RectTransform Card_transform;
  
    bool once = false;

    private void Start()
    {
        Card_transform = Card_Button.GetComponent<RectTransform>();
        if (Card_Button) Card_Button.onClick.RemoveAllListeners();
        if (Card_Button) Card_Button.onClick.AddListener(FlipMyObject);
    }



    private void FlipMyObject()
    {
        if (!once)
        {
            Card_transform.localEulerAngles = new Vector3(0, 180, 0);
            once = true;
            Card_transform.DORotate(new Vector3(0, 0, 0), 1, RotateMode.FastBeyond360);
            DOVirtual.DelayedCall(0.3f, () =>
            {
                if (Card_Button) {
                    Card_Button.image.sprite = cardImage;
                    Card_Button.interactable = false;
                } 
            });
        }
    }
}
