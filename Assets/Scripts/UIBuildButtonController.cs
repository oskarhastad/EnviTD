using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class UIBuildButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject tooltip;

    void Awake()
    {
        tooltip.SetActive(false);

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.SetActive(false);
    }
}
