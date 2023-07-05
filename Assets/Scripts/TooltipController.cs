using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{

    [SerializeField] TMP_Text text;
    [SerializeField] TowerSO tower;

    void Start()
    {
        text.text = $"{tower.price}g";
    }

}
