using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipManager : ManagerBase<TipManager> {
    [SerializeField] TipPanel TipPanel;
    [SerializeField] Camera uiCamera;

    RectTransform _tipRect;

    void Start() {
        if (TipPanel != null)
            _tipRect = TipPanel.GetComponent<RectTransform>();
        if (uiCamera == null)
            uiCamera = Camera.main;
    }

    public void ShowTip(ElectricElementBase element) {
        if (element == null || TipPanel == null || _tipRect == null) return;

        TipPanel.gameObject.SetActive(true);

        // 将 element 的世界坐标转换为屏幕坐标
        Vector3 worldPos = element.transform.position;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);

        // 将屏幕坐标转换为本地坐标（相对于父级 Canvas）
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _tipRect.parent as RectTransform,
            screenPos,
            uiCamera,
            out localPos
        );

        _tipRect.localPosition = localPos;

        TipPanel.textIntensity.text = $"强度: {element.intensity}";

        TipPanel.textName.text = element.showName;
    }

    public void HideTip() {
        if (TipPanel != null)
            TipPanel.gameObject.SetActive(false);
    }
}