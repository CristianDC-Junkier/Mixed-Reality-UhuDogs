using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text buttonText;

    public Color normalColor;
    public Color hoverColor;
    public Color clickColor;

    private void OnEnable()
    {
        buttonText.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        buttonText.color = clickColor;
    }
}
