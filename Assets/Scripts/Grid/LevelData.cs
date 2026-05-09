using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LevelItem
{
    public int elementId = -1;          // 元件唯一编号
    public Vector2Int position;
    public CellType type;
    public int signalStrength;
    public int requiredStrength;
    public int amplifyValue;
    public int activateThreshold;
    public Color phaseColor;
    public List<int> connections;       // 显式连接的元件编号列表
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

    [Header("关卡数据源")]
    [Tooltip("关卡 CSV 文件（TextAsset），运行时从此解析 items")]
    public TextAsset csvData;

    [Tooltip("若勾选，运行时不再解析 CSV，直接使用下方 items 列表")]
    public bool useInlineItems = false;

    [Header("背景")]
    [Tooltip("关卡背景 Sprite（由 LevelManager 在加载时设置到场景里指定的 SpriteRenderer 上）。留空则保留上一关的背景。")]
    public Sprite background;

    [HideInInspector]
    public List<LevelItem> items;

    /// <summary>解析 CSV 数据到 items 列表。
    /// 只要 csvData 不为 null，就强制重新解析（忽略 useInlineItems），
    /// 保证每次加载关卡时拿到的是 CSV 中最新的数据。
    /// useInlineItems = true 时仅作为"完全没有 CSV，纯靠 Inspector 配置 items"的退路。</summary>
    public void ParseCSV() {
        if (csvData == null) return;            // 没 CSV 就保留 inline items
        if (useInlineItems) return;             // 用户明确禁用 CSV
        items = LevelCSVParser.Parse(csvData.text);
    }
}