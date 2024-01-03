using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class SelectSceneDirecter : MonoBehaviour
{
    //チケットの枚数
    static int ticketAmount = 30;
    //消費するチケットの枚数
    int comsumptionticket;
    //増やす駒を配列として記録
    public static int[] addUnitsAmount = new int[7];
    //駒のコストを記憶
    int[] unitscost = { 1, 7, 8, 3, 4, 5, 6 };
    //駒を増やすか減らすかを切り替える
    bool AddorReduce = true;
    //増やす減らすテキスト
    [SerializeField] Text AddorReduceButtonText;
    //チケットの残り枚数を表示するテキスト
    [SerializeField] Text TicketAmounttext;
    //各コマの増やす枚数
    [SerializeField] Text amountaddHu;
    [SerializeField] Text amountaddKei;
    [SerializeField] Text amountaddKyo;
    [SerializeField] Text amountaddGin;
    [SerializeField] Text amountaddKin;
    [SerializeField] Text amountaddKaku;
    [SerializeField] Text amountaddHisha;
    //持ち時間
    public static int gameTimeId = 0;
    //持ち時間選択ボタンを格納するリスト
    public List<GameObject> gameRuleButton = new List<GameObject>();
    //選択した持ち時間を示すパネル
    public GameObject showSelectGameRulePanel;
    // Start is called before the first frame update
    void Start()
    {
        //配列の初期化
        for(int i = 0; i< addUnitsAmount.Length; i++)
        {
            addUnitsAmount[i] = 0;
        }
        //それぞれの駒の増加枚数を表示
        for(int i = 0; i < 7; i++)
        {
            updateaddunitamount(i);
        }
        //消費チケット枚数の初期化
        displayTicketAmount(0, ticketAmount);
    }

    public void OnClickAddUnits(int unitID)
    {
        print("Clicked！");
        //チケットが足りないとき
        if (ticketAmount - unitscost[unitID] < 0 && AddorReduce)
        {
            print("チケットが足りません！");
            return;
        }

        addUnitsAmount[unitID] += 1;
        ticketAmount -= unitscost[unitID];
        comsumptionticket += unitscost[unitID];

        displayTicketAmount(comsumptionticket,ticketAmount);
        updateaddunitamount(unitID);
    }

    public void OnClickReduceUnits(int unitID)
    {
        if (addUnitsAmount[unitID] <= 0) return;
        addUnitsAmount[unitID] -= 1;
        ticketAmount += unitscost[unitID];
        comsumptionticket -= unitscost[unitID];

        displayTicketAmount(comsumptionticket, ticketAmount);
        updateaddunitamount(unitID);
    }

    public void ChangeAddorReduce()
    {
        AddorReduce = !AddorReduce;
        if(AddorReduce)
        {
            AddorReduceButtonText.text = "増やす";
        }
        else if (!AddorReduce)
        {
            AddorReduceButtonText.text = "減らす";
        }
    }

    public void OnClickStartButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickReturnButton()
    {
        SceneManager.LoadScene("TitleScene");
    }

    void displayTicketAmount(int useticket,int reminingticket)
    {
        TicketAmounttext.text =useticket.ToString() +"枚 / "+ reminingticket.ToString();
    }

    void updateaddunitamount(int unitID)
    {
        var kindofaddunitamount = "×" + addUnitsAmount[unitID].ToString();
        switch (unitID)
        {
            case 0:
                amountaddHu.text = kindofaddunitamount;
                break;
            case 1:
                amountaddKaku.text = kindofaddunitamount;
                break;
            case 2:
                amountaddHisha.text = kindofaddunitamount;
                break;
            case 3:
                amountaddKyo.text = kindofaddunitamount;
                break;
            case 4:
                amountaddKei.text = kindofaddunitamount;
                break;
            case 5:
                amountaddGin.text = kindofaddunitamount;
                break;
            case 6:
                amountaddKin.text = kindofaddunitamount;
                break;
        }
    }

    public void OnClickTimeButton(int gamerule)
    {
        gameTimeId = gamerule;
        showSelectGameRulePanel.transform.position = gameRuleButton[gamerule].transform.position;
        print(gameTimeId);
    }
}
