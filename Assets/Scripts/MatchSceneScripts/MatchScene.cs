using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static UnityEditor.Progress;


public class MatchScene : MonoBehaviourPunCallbacks
{
    //��̉摜
    [SerializeField] private List<GameObject> units = new List<GameObject>();
    //�ړ��͈͂�\��
    [SerializeField] private GameObject cursor;
    //�I�����Ă����ɑ΂��ĕ\��
    [SerializeField] private GameObject selectUnitCursor;
    private GameObject selectCursor;

    //���ڂ̔z�u
    [SerializeField] private GameObject unitPosition;
    private GameObject[,] positions = new GameObject[9,9];
    private Vector2 unitInterval = new Vector2(31.7f * 5f / (34.8f * 9f), 5f / 9f);
    private Vector2 unit11Position = new Vector2(34.8f * 2.5f / 31.7f, 2.5f);
    private Vector2 adjustmentVector = new Vector2(0.4671805f, 0);

    //��̏����z�u�x�N�g��
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

    //���݂̋�̔z�u�x�N�g��
    private int[,] UnitIDArrangement;

    //���z��Ɋi�[���ĊǗ��A�Ăяo�����s����悤�ɂ���
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

    //�I���������UnitBehavior���ꎞ�I�Ɋi�[����
    private UnitBehavior selectUnitUnitBehavior;
    //�I��������i�߂鏡�ڂ̃��X�g
    private List<Vector2Int> movablePositionList = new List<Vector2Int>();
    //�\���������ڂ��ꎞ�I�Ɋi�[���郊�X�g
    private List<GameObject> movablePositionCursors = new List<GameObject>();
    //�I�𒆂̏��ڂ��ꎞ�I�Ɋi�[����ϐ�
    private Vector2Int selectedPosition;
    //��I������Ă��邩�ǂ������肷��
    private bool isSelected = false;
    //��I�����ꂽ���ɂ�����Ə�ɓ�����
    private Vector3 selectedUnitLittleFloat = new Vector3(-0.1f, 0.11f, 0);
    //�I������������̕����ǂ���
    private bool itsMine;

    //�v���C���[ID�i����0�A���Ȃ�P�j
    private int playerID = 1;

    //�����̎�Ԃ��ǂ���
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

    //�N���b�N���ꂽ�ꏊ�ɂ����𔻒肵�ď������s��
    void JudgeSelectUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // �q�b�g�����I�u�W�F�N�g�̏����擾
            GameObject hitObject = hit.collider.gameObject;
            string objectName = hitObject.name;

            // �����񂩂琔�����������o��
            string[] coordinates = objectName.Split(',');
            float x = float.Parse(coordinates[0].Trim('(', ' '));
            float y = float.Parse(coordinates[1].Trim(')', ' '));

            // Vector2���쐬
            Vector2 vector2 = new Vector2(x+1, y+1);

            // Vector2Int�ɕϊ��i�����_�ȉ���؂�̂āj
            Vector2Int vector2Int = new Vector2Int(Mathf.RoundToInt(vector2.x),
                Mathf.RoundToInt(vector2.y));

            selectUnitUnitBehavior = GetUnitObject(vector2Int).GetComponent<UnitBehavior>();

            //�I���A�I�𒆂̋�ړ��ł���}�X�ɑ΂��ď��������s
            if (selectUnitUnitBehavior.owingPlayerID == playerID + 1 
                || movablePositionList.Contains(vector2Int))
            {
                VisualizeSelectUnit(vector2Int);

                GetRangeOfMotionArea(vector2Int);

                MoveUnit(vector2Int);
            }
        }
    }

    //�I�𒆂̋�����F�ł���悤�ɂ���
    void VisualizeSelectUnit(Vector2Int vector2Int)
    {
        //�������I���������i��������グ�����Ƃ��j
        if (selectedPosition == vector2Int && !isSelected)
        {
            GetUnitObject(vector2Int).transform.position += 
                Mathf.Pow(-1, (playerID)) * selectedUnitLittleFloat;

            isSelected = true;
        }
        //�������I���������i���u�������Ƃ��j
        else if (selectedPosition == vector2Int && isSelected)
        {
            GetUnitObject(vector2Int).transform.position -= 
                Mathf.Pow(-1, (playerID)) * selectedUnitLittleFloat;
            movablePositionList.Clear();
            isSelected = false;
        }
        //�قȂ��I�����ꂽ��
        else
        {
            GetUnitObject(vector2Int).transform.position += 
                Mathf.Pow(-1, (playerID)) * selectedUnitLittleFloat;
            //�O�ɕ\�������J�[�\�����폜���đI��������������J�[�\����\��

            if (selectedPosition != Vector2Int.zero && isSelected == true)
            {
                GetUnitObject(selectedPosition).transform.position -= 
                    Mathf.Pow(-1, (playerID)) * selectedUnitLittleFloat;
            }
            isSelected = true;
        }
    }

    //�w��̃}�X�̋�̓�����͈͂��������ĔՏ�ɕ\��
   �@void GetRangeOfMotionArea(Vector2Int vector2Int)
    {
        GameObject selectedUnit = GetUnitObject(vector2Int);
        UnitBehavior unitBehavior = selectedUnit.GetComponent<UnitBehavior>();
        List<Vector2Int> range = new List<Vector2Int>();

        range.Clear();

        if (!movablePositionList.Contains(vector2Int))
        {
            movablePositionList.Clear();
        }

        //�ȑO�ɕ\�������J�[�\�����폜
        foreach (var item in movablePositionCursors)
        {
            Destroy(item);
        }

        //���������o�^�Ȃ�o�^
        if (!unitBehavior.registered)
        {
            unitBehavior.RegisteUnitBehavior(selectedUnit.name);
            unitBehavior.registered = true;
        }
        //�I���������ѓ���i���A��ԁA�p�A���A�n�ȊO�̎��̓�����ꏊ�̓o�^���@�j
        if (!unitBehavior.projectile)
        {
            //�o�^���ꂽ�x�N�g�������Ƃɓ�����ꏊ��������
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }

            //�ՊO����
            JudgeBoardOutSide(range, vector2Int);
        }
        //�I����������̍��Ԃ̎��̓o�^���@
        else if (unitBehavior.thisUnitName == "FirstTurnKyo(Clone)")
        {
            JudgeForward(vector2Int, unitBehavior, range);
        }

        //�I����������̍��Ԃ̎��̓o�^���@
        else if (unitBehavior.thisUnitName == "SecondTurnKyo(Clone)")
        {
            JudgeBack(vector2Int, unitBehavior, range);
        }

        //�I���������Ԃ̎�
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

        //�Ō�ɃJ�[�\����\���A���u���ꍇ�ɂ͂��Ȃ�
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

    //movablePositionList�����Ƃɋ�𓮂���
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
            //����������������J�[�\����\��
            selectCursor = Instantiate(selectUnitCursor, positions[vector2Int[0] - 1
                    , vector2Int[1] - 1].transform.position
                    , Quaternion.identity) as GameObject;

            movablePositionList.Clear();
            isSelected = false;
            //turn = false; �^�[���̐؂�ւ��A�e�X�g�i�K�ł͘A���œ������������悢�̂Ŏ��s���Ȃ�
        }
        selectedPosition = vector2Int;
    }

    //�ՊO����
    void JudgeBoardOutSide(List<Vector2Int> range, Vector2Int vector2Int)
    {
        //������ꏊ����Ղ̊O�̕������폜
        for (int i = range.Count - 1; i >= 0; i--)
        {
            //���肷��}�X
            var item = vector2Int - Vector2Int.one + new Vector2Int(1, -1) * range[i];

            //�Ղ̊O�𔻒�
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

    //���Ԃ��ԂȂǂ̑O�������̔���i���̏ꍇ�͌�i�����j
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
            //�r���ŋ�ɓ�����Ȃ��ꍇ
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }

            JudgeBoardOutSide(range, vector2Int);
        }
    }

    //���Ԃ��ԂȂǂ̌�������̔���i���̏ꍇ�͑O�������j
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
            //�r���ŋ�ɓ�����Ȃ��ꍇ
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
            //�r���ŋ�ɓ�����Ȃ��ꍇ
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
            //�r���ŋ�ɓ�����Ȃ��ꍇ
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
            //�r���ŋ�ɓ�����Ȃ��ꍇ
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
            //�r���ŋ�ɓ�����Ȃ��ꍇ
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
            //�r���ŋ�ɓ�����Ȃ��ꍇ
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
            //�r���ŋ�ɓ�����Ȃ��ꍇ
            foreach (var item in unitBehavior.unitMovableDirections)
            {
                range.Add(item);
            }
            JudgeBoardOutSide(range, vector2Int);
        }
    }

    //��̗����ɑ��̋����A���̋����̋�������i�����ɓ�������������}�X�j
    void AddEnemyUnitMass(Vector2Int movableVector,Vector2Int vector2Int,List<Vector2Int> range)
    {
        if (GetUnitID(movableVector)/ 14 != playerID)
        {
            range.Add(new Vector2Int(1, -1) * (movableVector - vector2Int));
        }
    }

    //�w�肵�����ڂɓ����Ă�����Ԃ��A������o�O�̍����A
    //���͂����ۂ̏����̕����Ɠ����ɂ��悤�Ƃ��ĕs����Y�ތ���
    //�ɂȂ��Ă��邪�A�����C���������Ȃ�
    //�i��A4�܂̎w�肵�����Ƃ��̓��͂�vector2Int(4,5)����͂��邪�A
    //�@unitArrangement�ɂ�����4�ܒn�_�̋�̏��́i3,4�j�ɓ����Ă���j
    GameObject GetUnitObject(Vector2Int boardPositionID)
    {
        int positionx = boardPositionID.x - 1;
        int positiony = boardPositionID.y - 1;

        return UnitArrangement[positiony, positionx];
    }

    //�w�肵�����ڂɓ����Ă�����ID��Ԃ�
    int GetUnitID(Vector2Int boardPositionID)
    {
        int positionx = boardPositionID.x - 1;
        int positiony = boardPositionID.y - 1;

        return UnitIDArrangement[positiony, positionx];
    }
}
