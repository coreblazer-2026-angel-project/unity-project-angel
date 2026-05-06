using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 将 CSV 文本解析为 LevelItem 列表。
/// 必需列：elementId, cellType, x, y
/// 可选列：signalStrength, requiredStrength, amplifyValue, activateThreshold, connections
/// </summary>
public static class LevelCSVParser {

    public static List<LevelItem> Parse(string csvText) {
        var items = new List<LevelItem>();
        if (string.IsNullOrWhiteSpace(csvText)) {
            Debug.LogWarning("LevelCSVParser: CSV 文本为空");
            return items;
        }

        using var reader = new StringReader(csvText);

        // 读取并解析表头
        string headerLine = reader.ReadLine();
        if (headerLine == null) {
            Debug.LogWarning("LevelCSVParser: CSV 没有内容");
            return items;
        }

        string[] headers = SplitLine(headerLine);
        var colIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++) {
            colIndex[headers[i].Trim()] = i;
        }

        // 必需的列
        string[] requiredCols = { "elementId", "cellType", "x", "y" };
        foreach (var col in requiredCols) {
            if (!colIndex.ContainsKey(col)) {
                Debug.LogError($"LevelCSVParser: 缺少必需列 '{col}'");
                return items;
            }
        }

        var seenIds = new HashSet<int>();
        int lineNum = 1;
        string line;

        while ((line = reader.ReadLine()) != null) {
            lineNum++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] fields = SplitLine(line);
            if (fields.Length < requiredCols.Length) {
                Debug.LogWarning($"LevelCSVParser: 第 {lineNum} 行字段数不足，跳过");
                continue;
            }

            var item = new LevelItem();

            // elementId
            if (!int.TryParse(GetField(fields, colIndex, "elementId"), out int elementId)) {
                Debug.LogWarning($"LevelCSVParser: 第 {lineNum} 行 elementId 解析失败，跳过");
                continue;
            }
            if (seenIds.Contains(elementId)) {
                Debug.LogWarning($"LevelCSVParser: 第 {lineNum} 行 elementId {elementId} 重复，跳过");
                continue;
            }
            seenIds.Add(elementId);
            item.elementId = elementId;

            // cellType（支持中文名称映射）
            string typeStr = GetField(fields, colIndex, "cellType");
            if (!CellTypeNames.TryParse(typeStr, out CellType cellType)) {
                Debug.LogWarning($"LevelCSVParser: 第 {lineNum} 行 cellType '{typeStr}' 无效，跳过");
                continue;
            }
            item.type = cellType;

            // x, y
            if (!int.TryParse(GetField(fields, colIndex, "x"), out int x) ||
                !int.TryParse(GetField(fields, colIndex, "y"), out int y)) {
                Debug.LogWarning($"LevelCSVParser: 第 {lineNum} 行坐标解析失败，跳过");
                continue;
            }
            item.position = new Vector2Int(x, y);

            // 以下为可选列，有则读，无则默认 0
            item.signalStrength     = ParseIntField(fields, colIndex, "signalStrength");
            item.requiredStrength   = ParseIntField(fields, colIndex, "requiredStrength");
            item.amplifyValue       = ParseIntField(fields, colIndex, "amplifyValue");
            item.activateThreshold  = ParseIntField(fields, colIndex, "activateThreshold");

            // connections 可选
            string connStr = GetField(fields, colIndex, "connections");
            item.connections = ParseConnections(connStr);

            items.Add(item);
        }

        Debug.Log($"LevelCSVParser: 成功解析 {items.Count} 个元件");
        return items;
    }

    /// <summary>简单 CSV 行拆分，支持 "包裹的字段</summary>
    static string[] SplitLine(string line) {
        var result = new List<string>();
        bool inQuotes = false;
        var field = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++) {
            char c = line[i];

            if (c == '"') {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes) {
                result.Add(field.ToString().Trim());
                field.Clear();
                continue;
            }

            field.Append(c);
        }

        result.Add(field.ToString().Trim());
        return result.ToArray();
    }

    static string GetField(string[] fields, Dictionary<string, int> colIndex, string colName) {
        if (!colIndex.TryGetValue(colName, out int idx) || idx >= fields.Length) return string.Empty;
        return fields[idx].Trim();
    }

    static int ParseIntField(string[] fields, Dictionary<string, int> colIndex, string colName) {
        string val = GetField(fields, colIndex, colName);
        if (string.IsNullOrEmpty(val)) return 0;
        if (int.TryParse(val, out int result)) return result;
        // 兼容 Excel 导出的浮点格式（"8.0" / "3.5"）
        if (float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fresult))
            return Mathf.RoundToInt(fresult);
        return 0;
    }

    static List<int> ParseConnections(string connStr) {
        var list = new List<int>();
        if (string.IsNullOrWhiteSpace(connStr)) return list;

        foreach (var part in connStr.Split(',')) {
            string trimmed = part.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            if (int.TryParse(trimmed, out int id)) {
                list.Add(id);
            }
        }
        return list;
    }
}
