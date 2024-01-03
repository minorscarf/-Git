
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class CountDownTimer : MonoBehaviour
{

    //　トータル制限時間
    private float totalTime;
    //　制限時間（分）
    private int minute;
    //　制限時間（秒）
    private float seconds;
    //持ち時間分秒
    [SerializeField]
    private Vector2 havetime = new Vector2 (0, 0);
    //　前回Update時の秒数
    private float oldSeconds;
    private TextMeshProUGUI timerText;

    bool inmyturn = false;
    public bool firstmoveorsecondmove;

    GameSceneDirector GSD;

    void Start()
    {
        oldSeconds = 0f;
        timerText = GetComponentInChildren<TextMeshProUGUI>();
        Changehavetime(SelectSceneDirecter.gameTimeId);
        minute = (int)havetime.x;
        seconds = havetime.y;
        totalTime = minute * 60 + seconds;
    }

    void Update()
    {
        if (inmyturn) return;
        spendtime();
    }

    public void SetTurn(bool firstmove)
    {
        if (firstmove)
        {
            inmyturn = true;
            firstmoveorsecondmove = true;
        }
        else
        {
            inmyturn = false;
            firstmoveorsecondmove = false;
        }
    }

    public void changeturn()
    {
        inmyturn = !inmyturn;
    }

    void spendtime()
    {
        //　制限時間が0秒以下なら何もしない
        if (totalTime <= 0f)
        {
            return;
        }
        //　一旦トータルの制限時間を計測；
        totalTime = minute * 60 + seconds;
        totalTime -= Time.deltaTime;

        //　再設定
        minute = (int)totalTime / 60;
        seconds = totalTime - minute * 60;

        //　タイマー表示用UIテキストに時間を表示する
        if ((int)seconds != (int)oldSeconds)
        {
            timerText.text = minute.ToString("00") + ":" + ((int)seconds).ToString("00");
        }
        oldSeconds = seconds;
        //　制限時間以下になったらコンソールに『制限時間終了』という文字列を表示する
        if (totalTime <= 0f)
        {
            GSD = GameObject.Find("GameSceneDirector").GetComponent<GameSceneDirector>();
            GSD.TimeUp(firstmoveorsecondmove);
        }
    }

    public void Changehavetime(int modeid)
    {
        switch (modeid)
        {
            case 0:
                havetime.x = 5;
                havetime.y = 1;
                break;

            case 1:
                havetime.x = 2;
                havetime.y = 1;
                break;

            case 2:
                havetime.x = 0;
                havetime.y = 10;
                break;
        }
    }

    public void StopAllTimer()
    {
        inmyturn = true;
    }
}

