using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private float scaleFactor = 1.1f;
    private float scaleDuration = 0.2f;

    private Vector3 originalScale;
    private RectTransform buttonTransform;

    void Start()
    {
        buttonTransform = GetComponent<RectTransform>();
        originalScale = buttonTransform.localScale;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonTransform.DOScale(originalScale * scaleFactor, scaleDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonTransform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);
    }
}
