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

    [Header("地板（定位）")]
    [Tooltip("若勾选「仅使用场景烘焙地板」，运行 LoadLevel 时不再动态生成地板（避免与已烘焙到场景的地板重复）。烘焙请用菜单 Tools/Grid/将地板烘焙到当前场景。")]
    public bool spawnRuntimeFloor = true;

    [Tooltip("地板预制体。用于运行时生成；也用于编辑模式下烘焙到场景。烘焙后若关闭 spawnRuntimeFloor，请保存场景以持久保留地板。")]
    public GameObject floorPrefab;

    public int wireLimit;

    public List<LevelItem> items;
}