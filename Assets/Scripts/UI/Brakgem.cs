using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Brakgem : MonoBehaviour
{
    [SerializeField]
    private Button idle;
    [SerializeField]
    private Button active;

    void Start()
    {
        if (idle) idle.onClick.RemoveAllListeners();
        if (idle) idle.onClick.AddListener(break_gem);


    }

    void break_gem(){
        idle.transform.gameObject.SetActive(false);
        active.transform.gameObject.SetActive(true);
    }
    
}
