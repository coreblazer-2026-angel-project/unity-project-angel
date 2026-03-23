using UnityEngine;

public class Wire : MonoBehaviour, ISignalReceiver
{
    [SerializeField] private Color poweredColor = Color.yellow;
    [SerializeField] private Color unpoweredColor = Color.gray;

    private SpriteRenderer spriteRenderer;
    private Collider2D wireCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        wireCollider = GetComponent<Collider2D>();
        spriteRenderer.color = unpoweredColor;
    }

    public void OnSignalChanged(int strength)
    {
        bool isPowered = strength > 0;
        spriteRenderer.color = isPowered ? poweredColor : unpoweredColor;

        if (wireCollider != null)
        {
            wireCollider.enabled = !isPowered;
        }
    }
}
