using UnityEngine;

public interface IInteractable
{
    void Interact();
}

public class ActivatablePowerSource : MonoBehaviour, IInteractable
{
    [SerializeField] private int outputStrength = 10;
    [SerializeField] private Transform strengthBar;

    private GridCell cell;
    private bool isActivated;

    void Start()
    {
        cell = GetComponent<GridCell>();
    }

    public void Interact()
    {
        isActivated = !isActivated;
        int newStrength = isActivated ? outputStrength : 0;

        if (cell.signalStrength != newStrength)
        {
            cell.signalStrength = newStrength;
            UpdateBar(newStrength);
            PropagateSignal();
        }
    }

    private void UpdateBar(int strength)
    {
        strengthBar.localScale = new Vector3(strength / 10f, 1, 1);
    }

    private void PropagateSignal()
    {
        // 触发信号传播逻辑
    }
}
