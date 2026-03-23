using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LevelItem
{
    public Vector2Int position;
    public CellType type;
    public int signalStrength;
    public int requiredStrength;
    public int amplifyValue;
    public int activateThreshold;
    public Color phaseColor;
}

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Range(5, 20)]
    public  int width;

    [Range(5, 15)]
    public  int height;

    public int wireLimit;

    public List<LevelItem> items;
}