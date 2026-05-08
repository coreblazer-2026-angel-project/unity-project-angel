using System;
using System.Collections.Generic;

/// <summary>
/// CellType 中文名称映射，用于 CSV 导入时识别中文元件名称。
/// </summary>
public static class CellTypeNames {

    /// <summary>中文名称 -> CellType 映射</summary>
    static readonly Dictionary<string, CellType> _nameToType = new(StringComparer.OrdinalIgnoreCase) {
        // 基础
        ["空"] = CellType.Empty,
        ["墙"] = CellType.Wall,

        // 电源
        ["电源"] = CellType.PowerSource,
        ["固定电源"] = CellType.PowerSource,

        // 电线
        ["电线"] = CellType.Wire,
        ["线"] = CellType.Wire,

        // 灯/目标
        ["灯"] = CellType.HopeLamp,
        ["希望灯"] = CellType.HopeLamp,
        ["目标"] = CellType.HopeLamp,

        // 可激活电源
        ["可激活电源"] = CellType.ActivatablePower,
        ["开关电源"] = CellType.ActivatablePower,

        // 按压电源
        ["按压电源"] = CellType.PressSource,
        ["按钮电源"] = CellType.PressSource,
        ["按钮"] = CellType.PressSource,

        // 信号放大器
        ["放大器"] = CellType.SignalAmplifier,
        ["信号放大器"] = CellType.SignalAmplifier,
        ["增强器"] = CellType.SignalAmplifier,

        // 信号合并器（四种方向映射到四种 CellType）
        ["合并器(上)"] = CellType.SignalMerger,
        ["合并器(下)"] = CellType.SignalMergerDown,
        ["合并器(左)"] = CellType.SignalMergerLeft,
        ["合并器(右)"] = CellType.SignalMergerRight,

        // 信号增幅器
        ["增幅器"] = CellType.SignalBooster,
        ["信号增幅器"] = CellType.SignalBooster,
        ["能量注入器"] = CellType.SignalBooster,

        // 不可放置区
        ["不可放置"] = CellType.NoPlaceZone,
        ["不可放置区"] = CellType.NoPlaceZone,
        ["禁区"] = CellType.NoPlaceZone,

        // 十字交叉
        ["十字"] = CellType.CrossConnector,
        ["交叉器"] = CellType.CrossConnector,
        ["十字交叉"] = CellType.CrossConnector,

        // 相位（预留）
        ["相位块"] = CellType.PhaseBlock,
        ["相位触发器"] = CellType.PhaseTrigger,
    };

    /// <summary>CellType -> 中文显示名称（用于反向查询/显示）</summary>
    static readonly Dictionary<CellType, string> _typeToName = new() {
        [CellType.Empty] = "空",
        [CellType.Wall] = "墙",
        [CellType.PowerSource] = "电源",
        [CellType.Wire] = "电线",
        [CellType.HopeLamp] = "灯",
        [CellType.ActivatablePower] = "可激活电源",
        [CellType.PressSource] = "按压电源",
        [CellType.SignalAmplifier] = "放大器",
        [CellType.SignalMerger] = "合并器(上)",
        [CellType.SignalMergerDown] = "合并器(下)",
        [CellType.SignalMergerLeft] = "合并器(左)",
        [CellType.SignalMergerRight] = "合并器(右)",
        [CellType.SignalBooster] = "增幅器",
        [CellType.NoPlaceZone] = "不可放置区",
        [CellType.CrossConnector] = "十字交叉",
        [CellType.PhaseBlock] = "相位块",
        [CellType.PhaseTrigger] = "相位触发器",
    };

    /// <summary>尝试将中文/英文名称转换为 CellType</summary>
    public static bool TryParse(string name, out CellType result) {
        // 先尝试中文映射
        if (_nameToType.TryGetValue(name, out result))
            return true;

        // 再尝试英文枚举名
        if (System.Enum.TryParse<CellType>(name, true, out result))
            return true;

        return false;
    }

    /// <summary>获取 CellType 的中文显示名称</summary>
    public static string GetDisplayName(CellType type) {
        return _typeToName.TryGetValue(type, out var name) ? name : type.ToString();
    }
}
