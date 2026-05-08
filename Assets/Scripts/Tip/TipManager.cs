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

        // 按元件类型显示不同的详细信息
        string detail;
        switch (element) {
            case PowerSource ps:
                detail = $"当前电强: {ps.workIntensity}";
                break;
            case Light light:
                detail = $"工作阈值: {light.workIntensity}";
                break;
            case SignalBooster booster:
                detail = $"增强电强: {booster.boostValue}";
                break;
            case SignalAmplifier amp:
                detail = $"增强电强: {amp.boostValue}";
                break;
            default:
                detail = $"强度: {(element.isActivate ? element.intensity : 0)}";
                break;
        }
        TipPanel.textIntensity.text = detail;

        TipPanel.textName.text = element.showName;
    }

    public void HideTip() {
        if (TipPanel != null)
            TipPanel.gameObject.SetActive(false);
    }
}