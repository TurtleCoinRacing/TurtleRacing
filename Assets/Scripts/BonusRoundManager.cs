using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(TwitchIRC))]
public class BonusRoundManager : MonoBehaviour
{
    private TwitchIRC IRC;
    public float xposition;
    public string movedirection;
    public float SpeedMultiplier = 5, lengthOfGame = 60;
    public GameObject ballToSpawn;
    public TextMeshProUGUI RaceStartingTimer;
    void Awake()
    {
        GuestManager.LoadGuestData();
    }

    void Start()
    {
        IRC = this.GetComponent<TwitchIRC>();
        //IRC.SendCommand("CAP REQ :twitch.tv/tags"); //register for additional data such as emote-ids, name color etc.
        IRC.messageRecievedEvent.AddListener(OnChatMsgRecieved);
    }

    // Update is called once per frame
    void Update()
    {
        xposition = gameObject.transform.position.x;
        if (xposition > 10)
        {
            movedirection = "left";
        }
        if (xposition < -10)
        {
            movedirection = ("right");
        }
        if (movedirection == "right")
        {
            transform.Translate(Time.deltaTime * SpeedMultiplier, 0, 0);
        }
        if (movedirection == "left")
        {
            transform.Translate(-Time.deltaTime * SpeedMultiplier, 0, 0);
        }
        if(Input.GetKeyDown(KeyCode.X)){
            lengthOfGame=3;
        }
        if (Time.timeSinceLevelLoad > lengthOfGame)
        {
            foreach (GuestData gD in GuestManager.AllGuests)
            {
                gD.bonusRoundBallsDropped = 0;
            }
            GuestManager.SaveGuestData();
            print("restarting");
            SceneManager.LoadScene("Race");
        }
        RaceStartingTimer.text = Mathf.RoundToInt(lengthOfGame - Time.timeSinceLevelLoad) + " Seconds";
    }

    void OnChatMsgRecieved(string msg)
    {
        //parse from buffer.
        int msgIndex = msg.IndexOf("PRIVMSG #");
        string msgString = msg.Substring(msgIndex + IRC.channelName.Length + 11);
        string user = msg.Substring(1, msg.IndexOf('!') - 1);

        GuestManager.CheckOrRegisterGuest(user);
        foreach (GuestData gD in GuestManager.AllGuests)
        {
            if (gD.guestName == user && gD.bonusRoundBallsDropped < 6)
            {
                gD.bonusRoundBallsDropped++;
                DropBall(user);
            }
        }
        //BetManager.GetABet(msgString, user, this.gameObject);
        Debug.Log("<color=purple> msg = " + msg + "</color><color=blue> msgString = " + msgString + "</color><color=purple>user = " + user + "</color>");

    }
    public void DropBall(string droppersName)
    {
        GameObject newBall = Instantiate(ballToSpawn, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.Euler(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360))));
        BallController ballsScriptRef = newBall.GetComponent<BallController>();
        ballsScriptRef.label.text = droppersName;

    }

    public void fakeMessage(string fM)
    {
        int randomTurtleBot = Random.Range(1, 5);

        OnChatMsgRecieved(":turtlebot" + randomTurtleBot + "!turtleracingalpha@turtleracingalpha.tmi.twitch.tv PRIVMSG #turtleracingalpha :Bet " + fM + " 5 win");
    }
}
