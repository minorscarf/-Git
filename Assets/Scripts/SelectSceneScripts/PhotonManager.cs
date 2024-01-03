using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public PhotonManager instance;
    //対局待ちの時に表示するパネル
    public GameObject waitPanel;
    //対局待ち時に表示するテキスト
    public Text waitText;
    //チケット選択UI 
    public GameObject selectTicketPart;
    //持ち時間選択ボタン
    public GameObject selectGameRule;
    //対局開始ボタン
    public GameObject startGameButton;
    //タイトル画面に戻るボタン
    public GameObject returnTitleSceneButton;
    //駒選択モードに戻るボタン
    public GameObject returnSelectModeButton;
    //待ち時間
    private float waitTime = 5f;
    //ルームマスターとして部屋を作成するモードかどうか
    private bool isRoomMaster = false;
    //部屋にアクセスするためのキーのリスト
    private int[] playerKeys;
    //プレイヤーのランク
    private int playerRank;

    private void Awake()
    {
        instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        ChangeSelectModeOrWait(true); 
    }

    //対局開始を押すとマッチング開始
    public void ConnectNetwork()
    {
        playerRank = Setting.CalculateRank(PlayerPrefs.GetInt("PlayerRating", 1500));
        int keyWidth = PlayerPrefs.GetInt("KeyWidth", 1);
        playerKeys = Setting.CalculateKeys(playerRank, keyWidth);

        PhotonNetwork.ConnectUsingSettings();

        ChangeSelectModeOrWait(false);
    }

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Connected to Master Server");

            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        print("Joined lobby!");
        PhotonNetwork.JoinRandomRoom();
        //JoinRandomRoomWithRating();
    }

    //戻るボタンを押すとネットワークから切断される
    public void DisConnectNetwork()
    {
        PhotonNetwork.Disconnect();

        ChangeSelectModeOrWait(true);
    }

    //待機状態と選択状態の切り替え
    //SelectModeがtrueならUIを表示して駒選択モード
    //falseなら待機状態
    void ChangeSelectModeOrWait(bool SelectMode)
    {
        selectTicketPart.SetActive(SelectMode);

        selectGameRule.SetActive(SelectMode);

        startGameButton.SetActive(SelectMode);

        returnTitleSceneButton.SetActive(SelectMode);

        waitPanel.SetActive(!SelectMode);

        returnSelectModeButton.SetActive(!SelectMode);
    }

    //ルームをランク±１の範囲で検索
    void JoinRandomRoomWithRating()
    {
        if (PhotonNetwork.InLobby)
        {
            foreach (int key in playerKeys)
            {
                ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
                expectedCustomRoomProperties.Add("RoomKey", key);

                PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 2);

                print(key);
            }

            //適切な部屋が見つからなかった場合新しく部屋を作成する
          //  CreateRoomWithKey(playerRank);
        }
        else
        {
            Debug.Log("Not in lobby yet. Waiting for OnJoinedLobby or OnConnectedToMaster callback.");
        }
        
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join a random room. Creating a new room.");
        CreateRoomWithKey(playerRank);
    }

    public override void OnCreatedRoom()
    {
        // 部屋が正常に作成されたときの処理
        Debug.Log("Room created successfully.");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 新しいプレイヤーがルームに入ったときの処理
        Debug.Log("Player entered the room: " + newPlayer.NickName);
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                StartMatch();
            }
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room!");
    }

    void CreateRoomWithKey(int roomKey)
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "RoomKey", roomKey } },
            CustomRoomPropertiesForLobby = new string[] { "RoomKey" } // ロビーで表示するカスタムプロパティ
        };

        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    //対局シーンに遷移
    private void StartMatch()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }
}
