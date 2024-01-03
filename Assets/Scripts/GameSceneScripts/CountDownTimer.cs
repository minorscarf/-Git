
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class CountDownTimer : MonoBehaviour
{

    //�@�g�[�^����������
    private float totalTime;
    //�@�������ԁi���j
    private int minute;
    //�@�������ԁi�b�j
    private float seconds;
    //�������ԕ��b
    [SerializeField]
    private Vector2 havetime = new Vector2 (0, 0);
    //�@�O��Update���̕b��
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
        //�@�������Ԃ�0�b�ȉ��Ȃ牽�����Ȃ�
        if (totalTime <= 0f)
        {
            return;
        }
        //�@��U�g�[�^���̐������Ԃ��v���G
        totalTime = minute * 60 + seconds;
        totalTime -= Time.deltaTime;

        //�@�Đݒ�
        minute = (int)totalTime / 60;
        seconds = totalTime - minute * 60;

        //�@�^�C�}�[�\���pUI�e�L�X�g�Ɏ��Ԃ�\������
        if ((int)seconds != (int)oldSeconds)
        {
            timerText.text = minute.ToString("00") + ":" + ((int)seconds).ToString("00");
        }
        oldSeconds = seconds;
        //�@�������Ԉȉ��ɂȂ�����R���\�[���Ɂw�������ԏI���x�Ƃ����������\������
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

