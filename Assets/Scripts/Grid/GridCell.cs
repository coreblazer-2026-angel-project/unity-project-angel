using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public enum Direction
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}






public static class DirectionExtensions
{
    // 方向转向量
    public static Vector2Int ToVector(this Direction dir)
    {
        return dir switch
        {
            Direction.Up => Vector2Int.up,
            Direction.Right => Vector2Int.right,
            Direction.Down => Vector2Int.down,
            Direction.Left => Vector2Int.left,
            _ => Vector2Int.zero
        };
    }

    // 获取反向
    public static Direction Opposite(this Direction dir)
    {
        return dir switch
        {
            Direction.Up => Direction.Down,
            Direction.Right => Direction.Left,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            _ => dir
        };
    }
}


public class GridCell : MonoBehaviour
{

    public Vector2Int position;      //坐标
    public CellType type;            // 单元格类型
    public int signalStrength;       // 信号强度
    public bool[] connections =  new bool[4];  //四方向链接
    
    
    
    
    
    

    
}
