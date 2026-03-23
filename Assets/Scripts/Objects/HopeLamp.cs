using UnityEngine;

public class HopeLamp : MonoBehaviour, ISignalReceiver
{
    [SerializeField] private int requiredStrength = 5;
    [SerializeField] private Color litColor = Color.yellow;
    [SerializeField] private Color unlitColor = Color.gray;

    private SpriteRenderer spriteRenderer;
    private bool isLit;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = unlitColor;
    }

    public void OnSignalChanged(int strength)
    {
        bool shouldBeLit = strength >= requiredStrength;

        if (shouldBeLit && !isLit)
        {
            isLit = true;
            spriteRenderer.color = litColor;
            PlayWinEffect();
        }
        else if (!shouldBeLit && isLit)
        {
            isLit = false;
            spriteRenderer.color = unlitColor;
        }
    }

    private void PlayWinEffect()
    {
        // 点亮特效
    }
}
