using UnityEngine;

public interface ISignalReceiver
{
    void OnSignalChanged(int strength);
}

public class PowerSource : MonoBehaviour, ISignalReceiver
{
    [SerializeField] private int outputStrength = 10;
    [SerializeField] private Transform strengthBar;

    private GridCell cell;

    void Start()
    {
        cell = GetComponent<GridCell>();
        cell.signalStrength = outputStrength;
        UpdateBar(outputStrength);
    }

    public void OnSignalChanged(int strength)
    {
        UpdateBar(strength);
    }

    private void UpdateBar(int strength)
    {
        strengthBar.localScale = new Vector3(strength / 10f, 1, 1);
    }
}
