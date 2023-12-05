using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameSceneDirector : MonoBehaviour
{
    // UI関連
    [SerializeField] Text textTurnInfo;
    [SerializeField] Text textResultInfo;
    [SerializeField] Image FirstTurnInfo;
    [SerializeField] Image SecondTurnInfo;
    [SerializeField] Image WinText;
    [SerializeField] Image LoseText;
    [SerializeField] Button buttonTitle;
    [SerializeField] Button buttonRematch;
    [SerializeField] Button buttonEvolutionApply;
    [SerializeField] Button buttonEvolutionCancel;

    // ゲーム設定
    const int PlayerMax = 2;
    int boardWidth;
    int boardHeight;

    // タイルのプレハブ
    [SerializeField] GameObject prefabTile;

    // ユニットのプレハブ
    [SerializeField] List<GameObject> prefabUnits;

    //カメラ
    [SerializeField] CameraController CC;

    // 初期配置
    int[,] boardSetting =
    {
        { 4, 0, 1, 0, 0, 0, 11, 0, 14 },
        { 5, 2, 1, 0, 0, 0, 11,13, 15 },
        { 6, 0, 1, 0, 0, 0, 11, 0, 16 },
        { 7, 0, 1, 0, 0, 0, 11, 0, 17 },
        { 8, 0, 1, 0, 0, 0, 11, 0, 18 },
        { 7, 0, 1, 0, 0, 0, 11, 0, 17 },
        { 6, 0, 1, 0, 0, 0, 11, 0, 16 },
        { 5, 3, 1, 0, 0, 0, 11,12, 15 },
        { 4, 0, 1, 0, 0, 0, 11, 0, 14 },
    };

    // フィールドデータ
    Dictionary<Vector2Int, GameObject> tiles;
    UnitController[,] units;

    // 現在選択中のユニット
    UnitController selectUnit;

    // 移動可能範囲
    Dictionary<GameObject, Vector2Int> movableTiles;

    // カーソルのプレハブ
    [SerializeField] GameObject prefabCursor;
    [SerializeField] GameObject prefabLastCursor;

    // カーソルオブジェクト
    List<GameObject> cursors;
    //着手後のカーソル
    List<GameObject> lastMovedcursors;

    // プレイヤーとターン
    int nowPlayer;
    int turnCount;
    bool isCpu;

    //ゲームが終了しているかどうか
    bool isgame = true;

    // モード
    enum Mode
    {
        None,
        Start,
        Select,
        WaitEvolution,
        TurnChange,
        Result
    }

    Mode nowMode, nextMode;

    // 持ち駒タイルのプレハブ
    [SerializeField] GameObject prefabUnitTile;

    // 持ち駒を置く場所
    List<GameObject>[] unitTiles;
    [SerializeField] Vector3 unitsTilesPosition;

    // キャプチャされたユニット
    List<UnitController> captureUnits;

    // 敵陣設定
    const int EnemyLine = 3;
    List<int>[] enemyLines;

    // CPU
    [SerializeField] float EnemyWaitTimerMax = 0.01f;
    float enemyWaitTimer;
    public static int PlayerCount = 1;
    float cpusurrendermotivation = 0;

    // サウンド制御
    [SerializeField] SoundController sound;

    //クリックされたスクリーン座標
    Vector3 ClickedScreenPosition;

    [SerializeField] Vector2 worldoffset;
    [SerializeField] Vector2 yesnooffset;

    //プレイヤーの先手かどうか
    private bool firstmove;
    private Vector2 PlayerSideTurnInfo = new Vector2(0, -39);
    private Vector2 OppositeSideTurnInfo = new Vector2(0, 37);

    //駒を取ったかどうか
    private bool takenornot = false;
    //駒が既に選択されているものかどうか
    private bool selectedUnit = false;

    //クリックしたものがmovabletileかどうか
    bool clickthingismobabletileornot;

    [SerializeField] GameObject obid;

    CountDownTimer playersTimer;
    CountDownTimer opponentTimer;

    // Start is called before the first frame update
    void Start()
    {
        sound.PlayBGM(0);

        // UI関連初期設定
        buttonTitle.gameObject.SetActive(false);
        buttonRematch.gameObject.SetActive(false);
        buttonEvolutionApply.gameObject.SetActive(false);
        buttonEvolutionCancel.gameObject.SetActive(false);
        WinText.gameObject.SetActive(false);
        LoseText.gameObject.SetActive(false);   
        textResultInfo.text = "";

        // ボードサイズ
        boardWidth = boardSetting.GetLength(0);
        boardHeight = boardSetting.GetLength(1);

        // フィールド初期化
        tiles = new Dictionary<Vector2Int, GameObject>();
        units = new UnitController[boardWidth, boardHeight];

        // 移動可能範囲
        movableTiles = new Dictionary<GameObject, Vector2Int>();
        cursors = new List<GameObject>();
        lastMovedcursors= new List<GameObject>();   

        // 持ち駒を置く場所
        unitTiles = new List<GameObject>[PlayerMax];

        // キャプチャされたユニット
        captureUnits = new List<UnitController>();

        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                // タイルとユニットのポジション
                float x = i - boardWidth / 2;
                float y = j - boardHeight / 2;

                // ポジション
                Vector3 pos = new Vector3(x, 0, y);

                // タイルのインデックス
                Vector2Int tileindex = new Vector2Int(i, j);

                // タイル作成
                GameObject tile = Instantiate(prefabTile, pos, Quaternion.identity);
                Renderer tilerender = tile.GetComponent<MeshRenderer>();
                tilerender.enabled = false;
                tiles.Add(tileindex, tile);

                // ユニット作成
                int type    = boardSetting[i, j] % 10;
                int player  = boardSetting[i, j] / 10;

                if (0 == type) continue;

                // 初期化
                pos.y = 0.7f;

                GameObject prefab = prefabUnits[type - 1];
                GameObject unit = Instantiate(prefab, pos, Quaternion.Euler(90, player * 180, 0));
                unit.AddComponent<Rigidbody>();

                UnitController unitctrl = unit.AddComponent<UnitController>();
                unitctrl.Init(player, type, tile, tileindex);

                // ユニットデータセット
                units[i, j] = unitctrl;
            }
        }

        for (int i = 0; i < PlayerMax; i++)
        {
            unitTiles[i] = new List<GameObject>();
            int dir = (0 == i) ? 1 : -1;

            for (int j = 0; j < 9; j++)
            {
                Vector3 pos = unitsTilesPosition;
                pos.x = (pos.x + j) * dir;
                pos.z *= dir;

                GameObject obj = Instantiate(prefabUnitTile, pos, Quaternion.identity);
                unitTiles[i].Add(obj);

                obj.SetActive(false);
            }
        }

        // 敵陣設定
        enemyLines = new List<int>[PlayerMax];
        for (int i = 0; i < PlayerMax; i++)
        {
            enemyLines[i] = new List<int>();
            int rangemin = 0;
            if (0 == i)
            {
                rangemin = boardHeight - EnemyLine;
            }

            for (int j = 0; j < EnemyLine; j++)
            {
                enemyLines[i].Add(rangemin + j);
            }
        }

        // TurnChangeからはじめる場合−1
        nowPlayer = -1;

        // 初回モード
        nowMode = Mode.None;
        nextMode = Mode.TurnChange;

        //手番をランダムで決定
        firstmove = decideTurn();

        //先手が後手を知らせるテキストを配置
        if (firstmove)
        {
            FirstTurnInfo.rectTransform.anchoredPosition = PlayerSideTurnInfo;
            SecondTurnInfo.rectTransform.anchoredPosition = OppositeSideTurnInfo;
        }
        else
        {
            FirstTurnInfo.rectTransform.anchoredPosition = OppositeSideTurnInfo;
            SecondTurnInfo.rectTransform.anchoredPosition = PlayerSideTurnInfo;
        }

        addUnits(SelectSceneDirecter.addUnitsAmount,firstmove);

        //タイマーの設定
        playersTimer = GameObject.Find("FrontTimer").GetComponentInChildren<CountDownTimer>();
        opponentTimer = GameObject.Find("BackTimer").GetComponentInChildren<CountDownTimer>();
        playersTimer.SetTurn(firstmove);
        opponentTimer.SetTurn(!firstmove);
    }

    // Update is called once per frame
    void Update()
    {
        if (Mode.Start == nowMode)
        {
            startMode();
        }
        else if (Mode.Select == nowMode)
        {
            selectMode();
        }
        else if (Mode.TurnChange == nowMode)
        {
            turnChangeMode();
        }
        else if(Mode.Result == nowMode)
        {
            //print("結果 " +textResultInfo.text);
            //print("王手しているユニット");
            //foreach (var item in GetOuteUnits(units,nowPlayer))
            //{
            //    print(item.UnitType);
            //}
            ////nowMode = Mode.None;
            //OnClickRematch();
        }

        // モード変更
        if(Mode.None != nextMode)
        {
            nowMode = nextMode;
            nextMode = Mode.None;
        }

    }

    // 選択時
    void setSelectCursors(UnitController unit = null, bool playerunit = true)
    {
        // カーソル削除
        foreach (var item in cursors)
        {
            Destroy(item);
        }
        cursors.Clear();

        // 選択ユニットの非選択状態
        if(selectUnit)
        {
            selectUnit.Select(false);
            selectUnit = null;
        }

        if (!playerunit)
        {
            unit = null;
        }
        // ユニット情報がなければ終了
        if (!unit) return;

        // 移動可能範囲取得
        List<Vector2Int> movabletiles = getMovableTiles(unit);
        movableTiles.Clear();

        foreach (var item in movabletiles)
        {
            movableTiles.Add(tiles[item], item);
            // カーソル生成
            Vector3 pos = tiles[item].transform.position;
            pos.y += 0.51f;
            GameObject cursor = Instantiate(prefabCursor, pos, Quaternion.identity);
            MeshCollider cursorCollider = cursor.GetComponent<MeshCollider>();
            cursorCollider.enabled = false;
            cursors.Add(cursor);
        }

        // 選択状態
        if(playerunit)
        {
            unit.Select();
            selectUnit = unit;
        }
    }

    void setCursoratLastMovedTile(GameObject LastTile)
    {
        foreach(var item in lastMovedcursors)
        {
            Destroy(item);
        }
        lastMovedcursors.Clear();

        Vector3 pos = LastTile.transform.position;
        pos.y += 0.2f;
        GameObject lastCursor = Instantiate(prefabLastCursor,pos, Quaternion.identity); 
        MeshCollider lastCursorCollider = lastCursor.GetComponent<MeshCollider>();
        lastCursorCollider.enabled = false;
        lastMovedcursors.Add(lastCursor);
    }

    // ユニット移動
    Mode moveUnit(UnitController unit, Vector2Int tileindex)
    {
        // 移動し終わった後のモード
        Mode ret = Mode.TurnChange;

        // 現在地
        Vector2Int oldpos = unit.Pos;

        // 移動先に誰かいたらとる
        captureUnit(nowPlayer, tileindex);

        // ユニット移動
        unit.Move(tiles[tileindex], tileindex);

        // 内部データ更新(新しい場所)
        units[tileindex.x, tileindex.y] = unit;

        // ボード上の駒を更新
        if(FieldStatus.OnBard == unit.FieldStatus)
        {
            // 内部データ更新
            units[oldpos.x, oldpos.y] = null;

            // 成
            if(unit.isEvolution()
                && (enemyLines[nowPlayer].Contains(tileindex.y) || enemyLines[nowPlayer].Contains(oldpos.y)))
            {
                // 次のターンに移動可能かどうか
                UnitController[,] copyunits = new UnitController[boardWidth, boardHeight];
                // 自分以外いないフィールドを作る
                copyunits[unit.Pos.x, unit.Pos.y] = unit;

                //次移動できないなら強制成
                if(isCpu ||1 > unit.GetMovableTiles(copyunits).Count)
                {
                    unit.Evolution();
                }
                // 成るか確認
                else
                {
                    // 成った状態を表示
                    unit.Evolution();
                    setSelectCursors(unit);

                    Vector2 tapposition = Camera.main.WorldToScreenPoint(unit.transform.position);
                    buttonEvolutionApply.transform.position = tapposition + yesnooffset + worldoffset;
                    buttonEvolutionCancel.transform.position = tapposition - yesnooffset + worldoffset;
                    // ナビゲーション
                    textResultInfo.text = "成りますか？";
                    buttonEvolutionApply.gameObject.SetActive(true);
                    buttonEvolutionCancel.gameObject.SetActive(true);

                    ret = Mode.WaitEvolution;
                }
            }

        }
        // 持ち駒の更新
        else
        {
            // 持ち駒の更新
            captureUnits.Remove(unit);
        }

        // ユニットの状態を更新
        unit.FieldStatus = FieldStatus.OnBard;

        // 持ち駒表示を更新
        alignCaptureUnits(nowPlayer);

        // SE再生
        sound.PlaySE(0);

        return ret;
    }

    // 移動可能範囲取得
    List<Vector2Int> getMovableTiles(UnitController unit)
    {
        // 通常移動範囲
        List<Vector2Int> ret = unit.GetMovableTiles(units);

        // 王手されてしまうかチェック
        UnitController[,] copyunits = GetCopyArray(units);
        if(FieldStatus.OnBard == unit.FieldStatus)
        {
            copyunits[unit.Pos.x, unit.Pos.y] = null;
        }
        int outecount = GetOuteUnits(copyunits, unit.Player).Count;

        // 王手を回避できる場所を返す
        if(0<outecount)
        {
            ret = new List<Vector2Int>();
            List<Vector2Int> movabletiles = unit.GetMovableTiles(units);
            foreach (var item in movabletiles)
            {
                // 移動した状態を作る
                UnitController[,] copyunits2 = GetCopyArray(copyunits);
                copyunits2[item.x, item.y] = unit;
                outecount = GetOuteUnits(copyunits2, unit.Player, false).Count;
                if (1 > outecount) ret.Add(item);
            }
        }

        return ret;
    }

    // ターン開始
    void startMode()
    {
        // 勝敗がついていなければ通常モード
        nextMode = Mode.Select;

        // Info更新
        textTurnInfo.text = "";
        textResultInfo.text = "";
        if (turnCount > 1)
        {
            FirstTurnInfo.enabled = false;
            SecondTurnInfo.enabled = false;
        }

        // 勝敗チェック

        // 王手しているユニット
        List<UnitController> outeunits = GetOuteUnits(units, nowPlayer);
        bool isoute = 0 < outeunits.Count;
        if (isoute)
        {
            textResultInfo.text = "王手！！";
        }

        // 500手ルール
        if (500 < turnCount)
        {
            textResultInfo.text = "500手ルール！！\n" + "引き分け";
            ResultCounter.numberofdraw += 1;
            isgame = false;
        }

        // 自軍が移動可能か調べる
        int movablecount = 0;
        foreach (var item in getUnits(nowPlayer))
        {
            movablecount += getMovableTiles(item).Count;
        }

        // 動かせない
        if(1>movablecount)
        {
            textResultInfo.text = "動かせません\n" + "引き分け";
            StopTimer();
            if (!isoute && !isCpu)
            {
                ResultCounter.numberofwin += 1;
                WinText.gameObject.SetActive(true);
            }
            else if (!isoute &&isCpu)
            {
                ResultCounter.numberoflose += 1;
                LoseText.gameObject.SetActive(true);
            }

            if(isoute)
            {
                WinText.gameObject.SetActive(true);
                ResultCounter.numberofwin += 1;
            }

            isgame = false;
            nextMode = Mode.Result;
        }

        // CPU判定 
        if (firstmove)
        {
            if (PlayerCount <= nowPlayer)
            {
                isCpu = true;
                enemyWaitTimer = Random.Range(0, EnemyWaitTimerMax);
            }
        }
        else if (!firstmove)
        {
            if (PlayerCount > nowPlayer)
            {
                isCpu = true;
                enemyWaitTimer = Random.Range(0, EnemyWaitTimerMax);
            }
        }

        // 次が結果表示画面なら
        if(Mode.Result == nextMode)
        {
            StartCoroutine(AfterMatch());
        }
    }

    // ユニットとタイル選択
    void selectMode()
    {
        GameObject tile = null;
        UnitController unit = null;
        // プレイヤー処理
        if (Input.GetMouseButtonUp(0) && !isCpu)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // 手前のユニットにも当たり判定があるのでヒットした全てのオブジェクト情報を取得
            foreach (RaycastHit hit in Physics.RaycastAll(ray))
            {
                UnitController hitunit = hit.transform.GetComponent<UnitController>();
                // 持ち駒
                if (hitunit && FieldStatus.Captured == hitunit.FieldStatus && takenornot)
                {
                    unit = hitunit;
                    selectedUnit = false;
                }
                // タイル選択と上に乗っているユニット
                else if (tiles.ContainsValue(hit.transform.gameObject))
                {
                    tile = hit.transform.gameObject;
                    // タイルからユニットを探す
                    foreach (var item in tiles)
                    {
                        if (item.Value == tile)
                        {
                            unit = units[item.Key.x, item.Key.y];
                        }
                    }

                    if(unit &&  unit.transform.position.y < 0.65)
                    {
                        selectedUnit = false;
                    }
                    else if(unit && unit.transform.position.y >= 0.65)
                    {
                        selectedUnit = true;
                    }
                    break;
                }
            }
        }

        // CPU処理
        if(isCpu & isgame)
        {
            // タイマー消化
            if (0 < enemyWaitTimer)
            {
                enemyWaitTimer -= Time.deltaTime;
                return;
            }

            // ユニット選択
            if (!selectUnit)
            {
                //毎ターンランダムで投了
                float cpumotivation = randomnum(100);
                print(cpumotivation);
                if (cpumotivation < cpusurrendermotivation)
                {
                    CpuSurrender("勝利！");
                    return;
                }

                // 全ユニット取得してランダムで選択
                List<UnitController> allunits = getUnits(nowPlayer);
                unit = allunits[Random.Range(0, allunits.Count)];
                // 移動できないならやりなおし
                if (1 > getMovableTiles(unit).Count)
                {
                    unit = null;
                }
            }
            // タイル選択
            else
            {
                // 今回移動可能なタイルをランダムで選択
                List<GameObject> tiles = new List<GameObject>(movableTiles.Keys);
                tile = tiles[Random.Range(0, tiles.Count)];
                // 持ち駒は非表示になっている可能性があるので表示する
                selectUnit.gameObject.SetActive(true);
            }
        }
        if (unit)
        {
            print(unit);
        }
        //動けるタイル以外が選択された時時駒を置く
        if (tile && selectUnit && !movableTiles.ContainsKey(tile))
        {
            selectUnit.Select(false);
            setSelectCursors();
        }
        // 移動先選択
        if (tile && selectUnit && movableTiles.ContainsKey(tile))
        {
            setCursoratLastMovedTile(tile);
            nextMode = moveUnit(selectUnit, movableTiles[tile]);
        }
        // ユニット選択
        if(unit && !selectedUnit)
        {
            print("select unit");
            bool isplayer = nowPlayer == unit.Player;
            setSelectCursors(unit, isplayer);
        }
        else if (unit && selectedUnit)
        {
            bool isplayer = nowPlayer == unit.Player;
            setSelectCursors(unit, !isplayer);
        }
    }

    // ターン変更
    void turnChangeMode()
    {
        // ボタンとカーソルのリセット
        setSelectCursors();
        buttonEvolutionApply.gameObject.SetActive(false);
        buttonEvolutionCancel.gameObject.SetActive(false);

        // CPU状態解除
        isCpu = false;

        // 次のプレイヤーへ
        nowPlayer = GetNextPlayer(nowPlayer);
        ChangeTimerPlayer();

        // 経過ターン
        if( 0 == nowPlayer )
        {
            turnCount++;
        }

        nextMode = Mode.Start;
    }

    // 次のプレイヤー番号を返す
    public static int GetNextPlayer(int player)
    {
        int next = player + 1;
        if (PlayerMax <= next) next = 0;

        return next;
    }

    // ユニットを持ち駒にする
    void captureUnit(int player, Vector2Int tileindex)
    {
        UnitController unit = units[tileindex.x, tileindex.y];
        if (!unit) return;
        unit.Capture(player);
        captureUnits.Add(unit);
        units[tileindex.x, tileindex.y] = null;
        alignCaptureUnits(nowPlayer);

        //持ち駒使用制限の解放
        if (!takenornot) takenornot = true;
    }

    // 持ち駒を並べる
    void alignCaptureUnits(int player)
    {
        // 所持個数をいったん非表示
        foreach (var item in unitTiles[player])
        {
            item.SetActive(false);
        }

        // ユニットごとに分ける
        Dictionary<UnitType, List<UnitController>> typeunits
            = new Dictionary<UnitType, List<UnitController>>();

        foreach (var item in captureUnits)
        {
            if (player != item.Player) continue;
            typeunits.TryAdd(item.UnitType, new List<UnitController>());
            typeunits[item.UnitType].Add(item);
        }

        // タイプごとに並べて一番上だけ表示する
        int tilecount = 0;
        foreach (var item in typeunits)
        {
            if (1 > item.Value.Count) continue;

            // 置く場所
            GameObject tile = unitTiles[player][tilecount++];

            // 非表示にしていたタイルを表示する
            tile.SetActive(true);

            // 所持個数の表示
            tile.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text
                = "" + item.Value.Count;

            // 同じ種類の持ち駒を並べる
            for (int i = 0; i < item.Value.Count; i++)
            {
                // リスト内のユニットを表示
                GameObject unit = item.Value[i].gameObject;
                // 置く場所
                Vector3 pos = tile.transform.position;
                // 一旦ユニットを移動して表示する
                unit.SetActive(true);
                unit.transform.position = pos;
                // 1個目以外は非表示
                if (0 < i) unit.SetActive(false);
            }

        }
    }

    // 指定された配列をコピーして返す
    public static UnitController[,] GetCopyArray(UnitController[,] ary)
    {
        UnitController[,] ret = new UnitController[ary.GetLength(0), ary.GetLength(1)];
        Array.Copy(ary, ret, ary.Length);
        return ret;
    }

    // 指定された配置で王手しているユニットを返す
    public static List<UnitController> GetOuteUnits(UnitController[,] units, int player, bool checkotherunit = true)
    {
        List<UnitController> ret = new List<UnitController>();

        foreach (var unit in units)
        {
            if (!unit || player == unit.Player) continue;

            // ユニットの移動可能範囲
            List<Vector2Int> movabletiles = unit.GetMovableTiles(units, checkotherunit);

            foreach (var tile in movabletiles)
            {
                if (!units[tile.x, tile.y]) continue;

                if(UnitType.Gyoku == units[tile.x,tile.y].UnitType)
                {
                    ret.Add(unit);
                }
            }
        }

        return ret;
    }

    // 成るボタン
    public void OnClickEvolutionApply()
    {
        nextMode = Mode.TurnChange;
    }

    // 成らないボタン
    public void OnClickEvolutionCancel()
    {
        selectUnit.Evolution(false);
        OnClickEvolutionApply();
    }

    // 指定されたプレイヤー番号の全ユニットを取得する
    List<UnitController> getUnits(int player)
    {
        List<UnitController> ret = new List<UnitController>();

        // 全ユニットのリストを作成する
        List<UnitController> allunits = new List<UnitController>(captureUnits);
        allunits.AddRange(units);
        foreach (var item in allunits)
        {
            if (!item || player != item.Player) continue;
            ret.Add(item);
        }

        return ret;
    }

    // リザルト再戦
    public void OnClickRematch()
    {
        SceneManager.LoadScene("GameScene");
    }

    // リザルトタイトルへ
    public void OnClickTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    //プレイヤー側の敗北処理
    public void OnClickButtonSurrender(string message)
    {
        if (!isgame || isCpu) return;
        //textResultInfo.text = message;
        LoseText.gameObject.SetActive(true);
        StopTimer();
        ResultCounter.numberoflose += 1;
        isgame = false;
        StartCoroutine(AfterMatch());
    }

    //相手プレイヤーの敗北処理
    public void CpuSurrender(string message)
    {
        if (!isgame || !isCpu) return;
        //textResultInfo.text = message;
        WinText.gameObject.SetActive(true);
        StopTimer();
        ResultCounter.numberofwin += 1;
        isgame = false;
        StartCoroutine(AfterMatch());
    }

    //再戦、タイトルに戻るボタンを出現させる
    IEnumerator AfterMatch()
    {
        yield return new WaitForSeconds(1);
        nextMode = Mode.Result;
        textTurnInfo.text = "";
        buttonRematch.gameObject.SetActive(true);
        buttonTitle.gameObject.SetActive(true);
    }

    //対局開始時にランダムで手番を決定
    bool decideTurn()
    {
        int t = Random.Range(0, 2);
        CC.RotatebyTurn(t);
        if(t == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //乱数を生成
    float randomnum(float n)
    {
        return Random.Range(0, n);
    }

    //持ち駒を生成
    void addUnit(int unitId, bool playersturn)
    {
        int playerid;
        if (playersturn)
        {
            playerid = 0;
        }
        else
        {
            playerid = 1;
        }

        GameObject prefab = prefabUnits[unitId];
        GameObject unit = Instantiate(prefab,new Vector3(5,0.7f,5),Quaternion.identity);
        unit.AddComponent<Rigidbody>();

        UnitController unitctrl = unit.AddComponent<UnitController>();
        unitctrl.Init(playerid,unitId+1,obid,new Vector2Int(0,0));
        print(unitctrl.name);

        unitctrl.Capture(playerid);
        captureUnits.Add(unitctrl);

        alignCaptureUnits(playerid);
    }

    void addUnits(int[] addunitsAmout, bool playersturn)
    {
        for(int i = 0; i < addunitsAmout.Length; i++)
        {
            for(int j = 0; j < addunitsAmout[i]; j++)
            {
                addUnit(i, playersturn);
            }
        }
    }

    //着手したらカウントストップして相手のタイマーが進むようにする
    void ChangeTimerPlayer()
    {
        playersTimer.changeturn();
        opponentTimer.changeturn();
    }

    //時間切れ負けの処理
    public void TimeUp(bool playerisfirstornot)
    {
        if(playerisfirstornot == firstmove)
        {
            OnClickButtonSurrender("時間切れ");
        }

        else if(playerisfirstornot != firstmove)
        {
            CpuSurrender("時間切れ勝ち");
        }
    }

    void StopTimer()
    {
        playersTimer.StopAllTimer();
        opponentTimer.StopAllTimer();
    }
}
