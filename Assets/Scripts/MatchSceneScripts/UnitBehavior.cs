using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitBehavior : MonoBehaviour
{
    public List<Vector2Int> unitMovableDirections = new List<Vector2Int>();
    public bool registered = false;
    public int owingPlayerID;
    public string thisUnitName;
    public bool projectile = false;

    public void RegisterUnitInfo(int unitID)
    {
        if (unitID == 0) return;
        owingPlayerID = (unitID  / 14)+1; 
    }
    public void RegisteUnitBehavior(string unitName)
    {
        thisUnitName = unitName;

        if (unitName == "FirstTurnHu(Clone)")
        {
            unitMovableDirections.Add(new Vector2Int(0, 1));
        }
        if (unitName == "FirstTurnKei(Clone)")
        {
            unitMovableDirections.Add(new Vector2Int(1, 2));
            unitMovableDirections.Add(new Vector2Int(-1, 2));
        }
        if (unitName == "FirstTurnKyo(Clone)")
        {
            MoveForward();
            projectile = true;
        }
        if (unitName == "FirstTurnGin(Clone)")
        {
            unitMovableDirections.Add(new Vector2Int(0, 1));
            unitMovableDirections.Add(new Vector2Int(1, 1));
            unitMovableDirections.Add(new Vector2Int(-1, 1));
            unitMovableDirections.Add(new Vector2Int(1, -1));
            unitMovableDirections.Add(new Vector2Int(-1, -1));
        }
        if (unitName == "FirstTurnKin(Clone)")
        {
            unitMovableDirections.Add(new Vector2Int(0, 1));
            unitMovableDirections.Add(new Vector2Int(1, 1));
            unitMovableDirections.Add(new Vector2Int(-1, 1));
            unitMovableDirections.Add(new Vector2Int(1, 0));
            unitMovableDirections.Add(new Vector2Int(-1, 0));
            unitMovableDirections.Add(new Vector2Int(0, -1));
        }
        if (unitName == "FirstTurnKaku(Clone)")
        {
            MoveUpRight();
            MoveDownRight();
            MoveDownLeft();
            MoveUpLeft();
            projectile = true;
        }
        if (unitName == "FirstTurnHisha(Clone)")
        {
            MoveForward();
            MoveBack();
            MoveRight();
            MoveLeft();
            projectile = true;
        }
        if (unitName == "FirstTurnOu(Clone)" || unitName == "SecondTurnOu(Clone)")
        {
            unitMovableDirections.Add(new Vector2Int(0, 1));
            unitMovableDirections.Add(new Vector2Int(1, 0));
            unitMovableDirections.Add(new Vector2Int(0, -1));
            unitMovableDirections.Add(new Vector2Int(-1, 0));
            unitMovableDirections.Add(new Vector2Int(1, 1));
            unitMovableDirections.Add(new Vector2Int(-1, 1));
            unitMovableDirections.Add(new Vector2Int(1, -1));
            unitMovableDirections.Add(new Vector2Int(-1, -1));
        }

        if (unitName == "SecondTurnHu(Clone)")
        {
            unitMovableDirections.Add(new Vector2Int(0, -1));
        }
        if (unitName == "SecondTurnKei(Clone)")
        {
            unitMovableDirections.Add(new Vector2Int(-1, -2));
            unitMovableDirections.Add(new Vector2Int(1, -2));
        }
        if (unitName == "SecondTurnKyo(Clone)")
        {
            MoveBack();
            projectile = true;
        }
        if (unitName == "SecondTurnGin(Clone)")
        {
            unitMovableDirections.Add(new Vector2Int(0, -1));
            unitMovableDirections.Add(new Vector2Int(-1, -1));
            unitMovableDirections.Add(new Vector2Int(1, -1));
            unitMovableDirections.Add(new Vector2Int(-1, 1));
            unitMovableDirections.Add(new Vector2Int(1, 1));
        }
        if (unitName == "SecondTurnKin(Clone)")
        {
            unitMovableDirections.Add(new Vector2Int(0, -1));
            unitMovableDirections.Add(new Vector2Int(-1, -1));
            unitMovableDirections.Add(new Vector2Int(1, -1));
            unitMovableDirections.Add(new Vector2Int(0,1));
            unitMovableDirections.Add(new Vector2Int(-1, 0));
            unitMovableDirections.Add(new Vector2Int(1, 0));
        }
        if (unitName == "SecondTurnKaku(Clone)")
        {
            MoveUpRight();
            MoveDownRight();
            MoveDownLeft();
            MoveUpLeft();
            projectile = true;

        }
        if (unitName == "SecondTurnHisha(Clone)")
        {
            MoveForward();
            MoveBack();
            MoveRight();
            MoveLeft();
            projectile = true;
        }

    }

    public void MoveForward()
    {
        for (int i = 1; i < 9; i++)
        {
            unitMovableDirections.Add(new Vector2Int(0, i));
        }
    }
    public void MoveBack()
    {
        for (int i = 1; i < 9; i++)
        {
            unitMovableDirections.Add(new Vector2Int(0, -i));
        }
    }
    public void MoveRight()
    {
        for (int i = 1; i < 9; i++)
        {
            unitMovableDirections.Add(new Vector2Int(i, 0));
        }
    }
    public void MoveLeft()
    {
        for (int i = 1; i < 9; i++)
        {
            unitMovableDirections.Add(new Vector2Int(-i, 0));
        }
    }
    public void MoveUpRight()
    {
        for (int i = 1; i < 9; i++)
        {
            unitMovableDirections.Add(new Vector2Int(i, i));
        }
    }
    public void MoveDownRight()
    {
        for (int i = 1; i < 9; i++)
        {
            unitMovableDirections.Add(new Vector2Int(i, -i));
        }
    }
    public void MoveDownLeft()
    {
        for (int i = 1; i < 9; i++)
        {
            unitMovableDirections.Add(new Vector2Int(-i, -i));
        }
    }
    public void MoveUpLeft()
    {
        for (int i = 1; i < 9; i++)
        {
            unitMovableDirections.Add(new Vector2Int(-i, i));
        }
    }

    public List<Vector2Int> FirstTurnLanceBehavior(Vector2Int startPosition ,Vector2Int endPosition)
    {
        var movablePositions = new List<Vector2Int>();

        for (int i = startPosition[1] - endPosition[1] -1; i >= 1; i--)
        {
            movablePositions.Add(new Vector2Int(0, i));
        }
        return movablePositions;
    }

    public List<Vector2Int> SecondTurnLanceBehavior(Vector2Int startPosition, Vector2Int endPosition)
    {
        var movablePositions = new List<Vector2Int>();

        for (int i = endPosition[1] - startPosition[1] -1; i >= 1; i--)
        {
            movablePositions.Add(new Vector2Int(0, -i));
        }
        return movablePositions;
    }

    public List<Vector2Int> GetLeftDirection(Vector2Int startPosition, Vector2Int endPosition)
    {
        var movablePositions = new List<Vector2Int>();

        for (int i = endPosition[0] - startPosition[0] - 1; i >= 1; i--)
        {
            movablePositions.Add(new Vector2Int(i,0));
        }
        return movablePositions;
    }

    public List<Vector2Int> GetRightDirection(Vector2Int startPosition, Vector2Int endPosition)
    {
        var movablePositions = new List<Vector2Int>();
        for (int i = startPosition[0] - endPosition[0] - 1; i >= 1; i--)
        {
            movablePositions.Add(new Vector2Int(-i, 0));
        }
        return movablePositions;
    }

    public List<Vector2Int> GetLeftDownDirection(Vector2Int startPosition, Vector2Int endPosition)
    {
        var movablePositions = new List<Vector2Int>();

        for (int i = endPosition[1] - startPosition[1] - 1; i >= 1; i--)
        {
            movablePositions.Add(new Vector2Int(i, -i));
        }
        return movablePositions;
    }

    public List<Vector2Int> GetRightDownDirection(Vector2Int startPosition, Vector2Int endPosition)
    {
        var movablePositions = new List<Vector2Int>();

        for (int i = endPosition[1] - startPosition[1] - 1; i >= 1; i--)
        {
            movablePositions.Add(new Vector2Int(-i, -i));
        }
        return movablePositions;
    }

    public List<Vector2Int> GetRightUpDirection(Vector2Int startPosition, Vector2Int endPosition)
    {
        var movablePositions = new List<Vector2Int>();

        for (int i = startPosition[1] - endPosition[1] - 1; i >= 1; i--)
        {
            movablePositions.Add(new Vector2Int(-i, i));
        }
        return movablePositions;
    }

    public List<Vector2Int> GetLeftUpDirection(Vector2Int startPosition, Vector2Int endPosition)
    {
        var movablePositions = new List<Vector2Int>();

        for (int i = startPosition[1] - endPosition[1] - 1; i >= 1; i--)
        {
            movablePositions.Add(new Vector2Int(i, i));
        }
        return movablePositions;
    }
}
