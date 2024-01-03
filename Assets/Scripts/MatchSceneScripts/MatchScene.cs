using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static UnityEditor.Progress;


public class MatchScene : MonoBehaviourPunCallbacks
{
    //駒の画像
    [SerializeField] private List<GameObject> units = new List<GameObject>();
    //移動範囲を表示
    [SerializeField] private GameObject cursor;
    //選択している駒に対して表示
    [SerializeField] private GameObject selectUnitCursor;
    private GameObject selectCursor;

    //升目の配置
    [SerializeField] private GameObject unitPosition;
    private GameObject[,] positions = new GameObject[9,9];
    private Vector2 unitInterval = new Vector2(31.7f * 5f / (34.8f * 9f), 5f / 9f);
    private Vector2 unit11Position = new Vector2(34.8f * 2.5f / 31.7f, 2.5f);
    private Vector2 adjustmentVector = new Vector2(0.4671805f, 0);

    //駒の初期配置ベクトル
    private int[,] defoultUnitArrangement =
    {
        { 16,17,18,19,22,19,18,17,16 },
        { 0,20,0,0,0,0,0,21,0 },
        { 15,15,15,15,15,15,15,15,15 },
        {0,0,0,0,0,0,0,0,0 },
        {0,0,0,0,0,0,0,0,0 },
        {0,0,0,0,0,0,0,0,0 },
        {1,1,1,1,1,1,1,1,1 },
        {0,7,0,0,0,0,0,6,0 },
        {2,3,4,5,8,5,4,3,2 },
    };

    //現在の駒の配置ベクトル
    private int[,] UnitIDArrangement;

    //駒を配列に格納して管理、呼び出しが行えるようにする
    private GameObject[,] UnitArrangement =
    {
        {null,null,null,null,null,null,null,null,null },
        {null,null,null,null,null,null,null,null,null },
        {null,null,null,null,null,null,null,null,null },
        {null,null,null,null,null,null,null,null,null },
        {null,null,null,null,null,null,null,null,null },
        {null,null,null,null,null,null,null,null,null },
        {null,null,null,null,null,null,null,null,null },
        {null,null,null,null,null,null,null,null,null },
        {null,null,null,null,null,null,null,null,null },
    };

    //選択した駒のUnitBehaviorを一時的に格納する
    private UnitBehavior selectUnitUnitBehavior;
    //選択した駒が進める升目のリスト
    private List<Vector2Int> movablePositionList = new List<Vector2Int>();
    //表示した升目を一時的に格納するリスト
    private List<GameObject> movablePositionCursors = new List<GameObject>();
    //選択中の升目を一時的に格納する変数
    private Vector2Int selectedPosition;
    //駒が選択されているかどうか判定する
    private bool isSelected = false;
    //駒が選択された時にちょっと上に動く量
    private Vector3 selectedUnitLittleFloat = new Vector3(-0.1f, 0.11f, 0);
    //選択した駒が自分の物かどうか
    private bool itsMine;

    //プレイヤーID（先手は0、後手なら１）
    private int playerID = 1;

    //自分の手番かどうか
    private bool turn;
    // Start is called before the first frame update
    void Start()
    {
        Vector2 unitPositionTransform;
        UnitIDArrangement = defoultUnitArrangement;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                positions[i, j] = Instantiate(unitPosition);
                unitPositionTransform = (((unit11Position - new Vector2(i, j) * unitInterval)
                    + (unit11Position - new Vector2(i + 1, j + 1) * unitInterval)) / 2)
                    - adjustmentVector;
                positions[i, j].transform.position = unitPositionTransform;
                positions[i,j].name = new Vector2(i, j).ToString();

                UnitArrangement[j, i] = Instantiate(units[defoultUnitArrangement[j, i]],
                    unitPositionTransform, Quaternion.identity);
                UnitBehavior unitBehavior = UnitArrangement[j, i].AddComponent<UnitBehavior>();
                unitBehavior.RegisterUnitInfo(defoultUnitArrangement[j, i]);
            }
        }

        Camera.main.transform.rotation = Quaternion.Euler(0, 0, 180*playerID);
        if(playerID == 1)
        {
            turn = true;
        }
        else
        {
            turn = false;
        }
    }

    private void Update()
    {
        if(turn && Input.GetMouseButtonDown(0))
        {
            JudgeSelectUnit();
        }
    }

    //クリックされた場所にある駒を判定して処理を行う
    void JudgeSelectUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // ヒットしたオブジェクトの情報を取得
            GameObject hitObject = hit.collider.gameObject;
            string objectName = hitObject.name;

            // 文字列から数字部分を取り出す
            string[] coordinates = objectName.Split(',');
            float x = float.Parse(coordinates[0].Trim('(', ' '));
            float y = float.Parse(coordinates[1].Trim(')', ' '));

            // Vector2を作成
            Vector2 vector2 = new Vector2(x+1, y+1);

            // Vector2Intに変換（小数点以下を切り捨て）
            Vector2Int vector2Int = new Vector2Int(Mathf.RoundToInt(vector2.x),
                Mathf.RoundToInt(vector2.y));

            selectUnitUnitBehavior = GetUnitObject(vector2Int).GetComponent<UnitBehavior>();

            //選択、選択中の駒が移動できるマスに対して処理を実行
            if (selectUnitUnitBehavior.owingPlayerID == playerID + 1 
                || movablePositionList.Contains(vector2Int))
            {
                VisualizeSelectUnit(vector2Int);

                GetRangeOfMotionArea(vector2Int);

                MoveUnit(vector2Int);
            }
        }
    }

    //選択中の駒を視認できるようにする
    void VisualizeSelectUnit(Vector2Int vector2Int)
    {
        //同じ駒を選択した時（駒を持ち上げたいとき）
        if (selectedPosition == vector2Int && !isSelected)
        {
            GetUnitObject(vector2Int).transform.position += 
                Mathf.Pow(-1, (playerID)) * selectedUnitLittleFloat;

            isSelected = true;
        }
        //同じ駒を選択した時（駒を置きたいとき）
        else if (selectedPosition == vector2Int && isSelected)
        {
            GetUnitObject(vector2Int).transform.position -= 
                Mathf.Pow(-1, (playerID)) * selectedUnitLittleFloat;
            movablePositionList.Clear();
            isSelected = false;
        }
        //異なる駒が選択された時
        else
        {
            GetUnitObject(vector2Int).transform.position += 
                Mathf.Pow(-1, (playerID)) * selectedUnitLittleFloat;
            //前に表示したカーソルを削除して選択した駒を示すカーソルを表示

            if (selectedPosition != Vector2Int.zero && isSelected == true)
            {
                GetUnitObject(selectedPosition).transform.position -= 
                    Mathf.Pow(-1, (playerID)) * selectedUnitLittleFloat;
            }
            isSelected = true;
        }
    }

    //指定のマスの駒の動ける範囲を検索して盤上に表示
   　void GetRangeOfMotionArea(Vector2Int vector2Int)
    {
        GameObject selectedUnit = GetUnitObject(vector2Int);
        UnitBehavior unitBehavior = selectedUnit.GetComponent<UnitBehavior>();
        List<Vector2Int> range = new List<Vector2Int>();

        range.Clear();

        if (!movablePositionList.Contains(vector2Int))
        {
            movablePositionList.Clear();
        }

        //以前に表示したカーソルを削除
        foreach (var item in movablePositionCursors)
        {
            Destroy(item);
        }

        //動きが未登録なら登録
        if (!unitBehavior.registered)
        {
            unitBehavior.RegisteUnitBehavior(selectedUnit.name);
            unitBehavior.registered = true;
        }
        //選択した駒が飛び道具（香、飛車、角、龍、馬以外の時の動ける場所の登録方法）
        if (!unitBehavior.projectile)
        {
            //登録されたベクトルをもとに動ける場所を仮決定
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }

            //盤外判定
            JudgeBoardOutSide(range, vector2Int);
        }
        //選択した駒が先手の香車の時の登録方法
        else if (unitBehavior.thisUnitName == "FirstTurnKyo(Clone)")
        {
            JudgeForward(vector2Int, unitBehavior, range);
        }

        //選択した駒が後手の香車の時の登録方法
        else if (unitBehavior.thisUnitName == "SecondTurnKyo(Clone)")
        {
            JudgeBack(vector2Int, unitBehavior, range);
        }

        //選択した駒が飛車の時
        else if (unitBehavior.thisUnitName.Contains("Hisha"))
        {
            JudgeForward(vector2Int, unitBehavior, range);
            JudgeBack(vector2Int, unitBehavior, range);
            JudgeRight(vector2Int, unitBehavior, range);
            JudgeLeft(vector2Int, unitBehavior, range);
        }

        else if (unitBehavior.thisUnitName.Contains("Kaku"))
        {
            JudgeLeftDown(vector2Int, unitBehavior, range);
            JudgeRightDown(vector2Int, unitBehavior, range);
            JudgeRightUp(vector2Int, unitBehavior, range);
            JudgeLeftUp(vector2Int, unitBehavior, range);
        }

        //最後にカーソルを表示、駒を置く場合にはしない
        if (isSelected)
        {
            foreach (var rangePosition in range)
            {
                movablePositionCursors.Add(Instantiate(cursor,
                    positions[vector2Int[0] - 1 + rangePosition[0]
                    , vector2Int[1] - 1 - rangePosition[1]].transform.position
                    , Quaternion.identity));
                movablePositionList.Add(new Vector2Int(vector2Int[0] + rangePosition[0]
                    , vector2Int[1] - rangePosition[1]));
            }
        }
    }

    //movablePositionListをもとに駒を動かす
    void MoveUnit(Vector2Int vector2Int)
    {
        if(isSelected && movablePositionList.Contains(vector2Int))
        {
            GetUnitObject(selectedPosition).transform.position 
                = positions[vector2Int.x-1,vector2Int.y-1].transform.position;

            var unitObj = UnitArrangement[vector2Int.y - 1, vector2Int.x - 1];
            var unitInt = UnitIDArrangement[vector2Int.y - 1, vector2Int.x - 1];

            UnitArrangement[vector2Int.y - 1, vector2Int.x - 1] 
                = UnitArrangement[selectedPosition.y - 1, selectedPosition.x - 1];
            UnitArrangement[selectedPosition.y - 1, selectedPosition.x - 1]
                = unitObj;

            UnitIDArrangement[vector2Int.y - 1, vector2Int.x - 1]
                = UnitIDArrangement[selectedPosition.y - 1, selectedPosition.x - 1];
            UnitIDArrangement[selectedPosition.y - 1, selectedPosition.x - 1]
                = unitInt;

            Destroy(selectCursor);
            //動かした駒を示すカーソルを表示
            selectCursor = Instantiate(selectUnitCursor, positions[vector2Int[0] - 1
                    , vector2Int[1] - 1].transform.position
                    , Quaternion.identity) as GameObject;

            movablePositionList.Clear();
            isSelected = false;
            //turn = false; ターンの切り替え、テスト段階では連続で動かせた方がよいので実行しない
        }
        selectedPosition = vector2Int;
    }

    //盤外判定
    void JudgeBoardOutSide(List<Vector2Int> range, Vector2Int vector2Int)
    {
        //動ける場所から盤の外の部分を削除
        for (int i = range.Count - 1; i >= 0; i--)
        {
            //判定するマス
            var item = vector2Int - Vector2Int.one + new Vector2Int(1, -1) * range[i];

            //盤の外を判定
            if (item[0] > 8 || item[1] > 8 ||
                item[0] < 0 || item[1] < 0)
            {
                range.RemoveAt(i);
                continue;
            }

            if (GetUnitObject(item + Vector2Int.one).GetComponent<UnitBehavior>().owingPlayerID
                == selectUnitUnitBehavior.owingPlayerID)
            {
                range.RemoveAt(i);
                continue;
            }

            if (GetUnitObject(item + Vector2Int.one).GetComponent<UnitBehavior>().owingPlayerID != 0)
            {
                if (GetUnitObject(item + Vector2Int.one).GetComponent<UnitBehavior>().owingPlayerID
                != selectUnitUnitBehavior.owingPlayerID)
                {
                    continue;
                }
            }
        }
    }

    //香車や飛車などの前方方向の判定（後手の場合は後進方向）
    void JudgeForward(Vector2Int vector2Int,UnitBehavior unitBehavior, List<Vector2Int> range)
    {
        var hit = false;
        for (int i = 1; i < 9; i++)
        {
            var movableVector = vector2Int + new Vector2Int(0, -i);
            if (movableVector[0] > 9 || movableVector[1] > 9 ||
            movableVector[0] < 1 || movableVector[1] < 1)
            {
                foreach (var vector in unitBehavior.FirstTurnLanceBehavior(vector2Int, movableVector - Vector2Int.right))
                {
                    range.Add(vector);
                }
                hit = true;
                break;
            }
            if (GetUnitID(movableVector) != 0)
            {
                foreach (var vector in unitBehavior.FirstTurnLanceBehavior(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                AddEnemyUnitMass(movableVector,vector2Int, range);
                hit = true;
                break;
            }
        }

        if (!hit)
        {
            print("no hit");
            //途中で駒に当たらない場合
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }

            JudgeBoardOutSide(range, vector2Int);
        }
    }

    //香車や飛車などの後方方向の判定（後手の場合は前方方向）
    void JudgeBack(Vector2Int vector2Int,UnitBehavior unitBehavior,List<Vector2Int> range)
    {
        var hit = false;
        for (int i = 1; i < 9; i++)
        {
            var movableVector = vector2Int + new Vector2Int(0, i);
            if (movableVector[0] > 9 || movableVector[1] > 9 ||
            movableVector[0] < 1 || movableVector[1] < 1)
            {
                foreach (var vector in unitBehavior.SecondTurnLanceBehavior(vector2Int, movableVector - Vector2Int.right))
                {
                    range.Add(vector);
                }
                hit = true;
                break;
            }
            if (GetUnitID(movableVector) != 0)
            {
                foreach (var vector in unitBehavior.SecondTurnLanceBehavior(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                AddEnemyUnitMass(movableVector, vector2Int, range);
                hit = true;
                break;
            }
        }
        if (!hit)
        {
            //途中で駒に当たらない場合
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }
            JudgeBoardOutSide(range, vector2Int);
        }
    }

    void JudgeRight(Vector2Int vector2Int, UnitBehavior unitBehavior, List<Vector2Int> range)
    {
        var hit = false;
        for (int i = 1; i < 9; i++)
        {
            var movableVector = vector2Int + new Vector2Int(-i, 0);
            if (movableVector[0] > 9 || movableVector[1] > 9 ||
            movableVector[0] < 1 || movableVector[1] < 1)
            {
                foreach (var vector in unitBehavior.GetRightDirection(vector2Int, movableVector - Vector2Int.up))
                {
                    range.Add(vector);
                }
                hit = true;
                break;
            }
            if (GetUnitID(movableVector) != 0)
            {
                foreach (var vector in unitBehavior.GetRightDirection(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                AddEnemyUnitMass(movableVector, vector2Int, range);
                hit = true;
                break;
            }
        }
        if (!hit)
        {
            //途中で駒に当たらない場合
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }
            JudgeBoardOutSide(range, vector2Int);
        }
    }

    void JudgeLeft(Vector2Int vector2Int, UnitBehavior unitBehavior, List<Vector2Int> range)
    {
        var hit = false;
        for (int i = 1; i < 9; i++)
        {
            var movableVector = vector2Int + new Vector2Int(i, 0);
            if (movableVector[0] > 9 || movableVector[1] > 9 ||
            movableVector[0] < 1 || movableVector[1] < 1)
            {
                foreach (var vector in unitBehavior.GetLeftDirection(vector2Int, movableVector - Vector2Int.up))
                {
                    range.Add(vector);
                }
                hit = true;
                break;
            }
            if (GetUnitID(movableVector) != 0)
            {
                foreach (var vector in unitBehavior.GetLeftDirection(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                AddEnemyUnitMass(movableVector, vector2Int, range);
                hit = true;
                break;
            }
        }
        if (!hit)
        {
            //途中で駒に当たらない場合
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }
            JudgeBoardOutSide(range, vector2Int);
        }
    }

    void JudgeLeftDown(Vector2Int vector2Int, UnitBehavior unitBehavior, List<Vector2Int> range)
    {
        var hit = false;
        for (int i = 1; i < 9; i++)
        {
            var movableVector = vector2Int + new Vector2Int(i, i);
            if (movableVector[0] > 9 || movableVector[1] > 9 ||
            movableVector[0] < 1 || movableVector[1] < 1)
            {
                foreach (var vector in unitBehavior.GetLeftDownDirection(vector2Int, movableVector - Vector2Int.one))
                {
                    range.Add(vector);
                }
                hit = true;
                break;
            }
            if (GetUnitID(movableVector) != 0)
            {
                foreach (var vector in unitBehavior.GetLeftDownDirection(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                AddEnemyUnitMass(movableVector, vector2Int, range);
                hit = true;
                break;
            }
        }
        if (!hit)
        {
            //途中で駒に当たらない場合
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }
            JudgeBoardOutSide(range, vector2Int);
        }
    }

    void JudgeRightDown(Vector2Int vector2Int, UnitBehavior unitBehavior, List<Vector2Int> range)
    {
        var hit = false;
        for (int i = 1; i < 9; i++)
        {
            var movableVector = vector2Int + new Vector2Int(-i, i);
            if (movableVector[0] > 9 || movableVector[1] > 9 ||
            movableVector[0] < 1 || movableVector[1] < 1)
            {
                foreach (var vector in unitBehavior.GetRightDownDirection(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                hit = true;
                break;
            }
            if (GetUnitID(movableVector) != 0)
            {
                foreach (var vector in unitBehavior.GetRightDownDirection(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                AddEnemyUnitMass(movableVector, vector2Int, range);
                hit = true;
                break;
            }
        }
        if (!hit)
        {
            //途中で駒に当たらない場合
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }
            JudgeBoardOutSide(range, vector2Int);
        }
    }

    void JudgeRightUp(Vector2Int vector2Int, UnitBehavior unitBehavior, List<Vector2Int> range)
    {
        var hit = false;
        for (int i = 1; i < 9; i++)
        {
            var movableVector = vector2Int + new Vector2Int(-i, -i);
            if (movableVector[0] > 9 || movableVector[1] > 9 ||
            movableVector[0] < 1 || movableVector[1] < 1)
            {
                foreach (var vector in unitBehavior.GetRightUpDirection(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                hit = true;
                break;
            }
            if (GetUnitID(movableVector) != 0)
            {
                foreach (var vector in unitBehavior.GetRightUpDirection(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                AddEnemyUnitMass(movableVector, vector2Int,range);
                hit = true;
                break;
            }
        }
        if (!hit)
        {
            //途中で駒に当たらない場合
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }
            JudgeBoardOutSide(range, vector2Int);
        }
    }

    void JudgeLeftUp(Vector2Int vector2Int, UnitBehavior unitBehavior, List<Vector2Int> range)
    {
        var hit = false;
        for (int i = 1; i < 9; i++)
        {
            var movableVector = vector2Int + new Vector2Int(i, -i);
            if (movableVector[0] > 9 || movableVector[1] > 9 ||
            movableVector[0] < 1 || movableVector[1] < 1)
            {
                foreach (var vector in unitBehavior.GetLeftUpDirection(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                hit = true;
                break;
            }
            if (GetUnitID(movableVector) != 0)
            {
                foreach (var vector in unitBehavior.GetLeftUpDirection(vector2Int, movableVector))
                {
                    range.Add(vector);
                }
                AddEnemyUnitMass(movableVector, vector2Int,range);
                hit = true;
                break;
            }
        }
        if (!hit)
        {
            //途中で駒に当たらない場合
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }
            JudgeBoardOutSide(range, vector2Int);
        }
    }

    //駒の利きに他の駒があり、その駒が相手の駒だった時（そこに動かしたら駒が取れるマス）
    void AddEnemyUnitMass(Vector2Int movableVector,Vector2Int vector2Int,List<Vector2Int> range)
    {
        if (GetUnitID(movableVector)/ 14 != playerID)
        {
            range.Add(new Vector2Int(1, -1) * (movableVector - vector2Int));
        }
    }

    //指定した升目に入っている駒を返す、あらゆるバグの根源、
    //入力を実際の将棋の符号と同じにしようとして不具合を産む結果
    //になっているが、もう修正が利かない
    //（例、4五の指定したいときの入力はvector2Int(4,5)を入力するが、
    //　unitArrangementにおける4五地点の駒の情報は（3,4）に入っている）
    GameObject GetUnitObject(Vector2Int boardPositionID)
    {
        int positionx = boardPositionID.x - 1;
        int positiony = boardPositionID.y - 1;

        return UnitArrangement[positiony, positionx];
    }

    //指定した升目に入っている駒のIDを返す
    int GetUnitID(Vector2Int boardPositionID)
    {
        int positionx = boardPositionID.x - 1;
        int positiony = boardPositionID.y - 1;

        return UnitIDArrangement[positiony, positionx];
    }
}
