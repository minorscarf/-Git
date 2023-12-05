using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneDirector : MonoBehaviour
{
    // タイルのプレハブ
    [SerializeField] GameObject prefabTile;

    // ユニットのプレハブ
    [SerializeField] List<GameObject> prefabUnits;

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

    //マイページ
    [SerializeField] GameObject MypagePanel;
    [SerializeField] Text NumberOfWinInfo;
    static int amountofticket = 20;
    static List<int> usetickets = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        MypagePanel.SetActive(false);

        // ボードサイズ
        int boardWidth = boardSetting.GetLength(0);
        int boardHeight = boardSetting.GetLength(1);

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

                // ユニット作成
                int type = boardSetting[i, j] % 10;
                int player = boardSetting[i, j] / 10;

                if (0 == type) continue;

                // 初期化
                pos.y = 0.7f;

                GameObject prefab = prefabUnits[type - 1];
                GameObject unit = Instantiate(prefab, pos, Quaternion.Euler(90, player * 180, 0));
                unit.AddComponent<Rigidbody>();

                UnitController unitctrl = unit.AddComponent<UnitController>();
                unitctrl.Init(player, type, tile, tileindex);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickPvP()
    {
        GameSceneDirector.PlayerCount = 2;
        SceneManager.LoadScene("SelectScene");
    }

    public void OnClickPvE()
    {
        GameSceneDirector.PlayerCount = 1;
        SceneManager.LoadScene("SelectScene");
    }

    public void OnClickEvE()
    {
        GameSceneDirector.PlayerCount = 0;
        SceneManager.LoadScene("SelectScene");
    }

    public void OnClickMypage()
    {
        MypagePanel.SetActive(true);
        NumberOfWinInfo.text = ResultCounter.numberofwin + "勝" + ResultCounter.numberoflose + "敗" + ResultCounter.numberofdraw + "分";
    }

    public void OnClickMaypageClose()
    {
        MypagePanel.SetActive(false);
        SceneManager.LoadScene("TitleScene");
    }
}
