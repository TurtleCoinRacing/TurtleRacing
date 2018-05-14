using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using TMPro;

public class RaceManager : MonoBehaviour
{

    public GameObject[] TurtlesInTheRace, possibleTracks, possibleFinishLines;
    public List<BetData> CurrentRaceBets = new List<BetData>();
    //public TurtleData[] AllTurtlesData;

    public TurtleForSale[] CurrentTurtlesForSale;
    public AudioClip bellSound;
    public GameObject RaceResultsScreen, StartingLineLocator, FinishLineLocator, RaceStartingTimerObject, BettingClosedDialog, BettingOpenDialog, raceInfoDialog;
    public bool hasRaceStarted, hasRaceEnded, hasPlacedAIBets, hasAucionCompleted;
    public static bool isBettingOpen, isAuctionOpen;
    public float TimeBetweenRaces, timeRaceEnded;
    public CinemachineVirtualCamera BackHalfCam, FinishLineCam;
    public CinemachineDollyCart RaceCam1;
    public TextMeshProUGUI RaceResultsGUITextA, RaceResultsGUITextB, RaceResultsGuiTextC, RaceStartingTimer, bettingOpenInstructions, RaceInfoSurface, RaceInfoLength;


    // Use this for initialization
    void Awake()
    {

        GuestManager.LoadGuestData();

        GuestManager.CurrentRaceresults.Clear();
        for (int i = 0; i <= 9; i++)
        {
            //TurtlesInTheRace[i].name = AllTurtlesData[i].name;
            TurtlesInTheRace[i].name = gameObject.GetComponent<TurtleNamer>().GiveNewRandomName();
        }
        DeactivateObjects(possibleTracks);
        DeactivateObjects(possibleFinishLines);

        int randomTrack = Random.Range(0, possibleTracks.Length);
        int randomLength = Random.Range(0, possibleFinishLines.Length);
        possibleTracks[randomTrack].SetActive(true);
        possibleFinishLines[randomLength].SetActive(true);

        RaceInfoSurface.text = "Surface: " + possibleTracks[randomTrack].name;
        RaceInfoLength.text = "Length: " + possibleFinishLines[randomLength].name;

        FinishLineCam.gameObject.transform.position = new Vector3(FinishLineCam.transform.position.x, FinishLineCam.transform.position.y, possibleFinishLines[randomLength].transform.position.z);
        FinishLineLocator.transform.position = possibleFinishLines[randomLength].transform.position;

        isBettingOpen = true;
        string exampleName = TurtlesInTheRace[Random.Range(2, 5)].name;
        bettingOpenInstructions.text = "Type ''" + exampleName + " Win 10'' to bet 10 on " + exampleName + " to win!";
    }
    void DeactivateObjects(GameObject[] objectsToKill)
    {
        foreach (GameObject oTK in objectsToKill)
        {
            oTK.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad > 1 && !hasPlacedAIBets)
        {
            TwitchChatExample ChatLink = gameObject.GetComponent<TwitchChatExample>();
            for (int i = 0; i <= 128; i++)
            {
                ChatLink.fakeMessage(TurtlesInTheRace[Random.Range(0, 10)].name);

            }
            hasPlacedAIBets = true;
        }


        if (Input.GetKeyDown(KeyCode.X))
        {

            foreach (GameObject eachTurtle in TurtlesInTheRace)
            {
                TurtleAI ti = eachTurtle.GetComponent<TurtleAI>();
                ti.BaseSpeed = 50;
            }
            TimeBetweenRaces = 2;
        }
        if (Input.GetKeyDown(KeyCode.S))
        { //start the race shortcut
            TimeBetweenRaces = 2;
        }

        if (TimeBetweenRaces - 9 < Time.timeSinceLevelLoad && BettingOpenDialog.activeInHierarchy == true)
        {
            AudioSource.PlayClipAtPoint(bellSound, Camera.main.transform.position);
            BettingClosedDialog.SetActive(true);
            BettingOpenDialog.SetActive(false);
            raceInfoDialog.SetActive(false);
        }
        if (TimeBetweenRaces - 6 < Time.timeSinceLevelLoad && isBettingOpen)
        {
            isBettingOpen = false;
        }

        if (TimeBetweenRaces / 2 < Time.timeSinceLevelLoad && !hasAucionCompleted)
        {
            TurtleAuctionManager.FinalizeAuction(this.gameObject);
        }
        if (TimeBetweenRaces < Time.timeSinceLevelLoad && !hasRaceStarted)
        {
            hasRaceStarted = true;
            RaceStartingTimerObject.SetActive(false);
            BettingClosedDialog.SetActive(false);
            RaceCam1.m_Speed = 1;
            //TimeBetweenRaces += Time.time; //this was causing extra delays
            BetManager.FinalizeOdds(this.gameObject);
        }
        else
        {
            RaceStartingTimer.text = Mathf.RoundToInt(TimeBetweenRaces - Time.timeSinceLevelLoad) + " Seconds";
        }
        if (hasRaceStarted && FinishLineCam.Priority != 16)
        {
            foreach (GameObject turtleRef in TurtlesInTheRace)
            {
                TurtleAI turlteScriptRef = turtleRef.GetComponent<TurtleAI>();
                /*if(BackHalfCam.Priority!=15 && turlteScriptRef.percentFinished > 0.5f){
					BackHalfCam.Priority = 15;
				}*/
                if (turlteScriptRef.percentFinished > 0.9f)
                {
                    FinishLineCam.Priority = 16;
                }
            }
        }

        if (TurtleAI.HowManyTurtlesFinished == 10 && !hasRaceEnded)
        {
            hasRaceEnded = true;
            RaceResultsScreen.SetActive(true);
            BettingClosedDialog.SetActive(true);
            raceInfoDialog.SetActive(true);
            RaceResultsGUITextA.text = TurtleAI.RaceResultsColumn1 + "<nobr>";
            RaceResultsGUITextB.text = TurtleAI.RaceResultsColumn2 + "<nobr>";
            RaceResultsGuiTextC.text = TurtleAI.RaceResultsColumn3 + "<nobr>";

            timeRaceEnded = Time.timeSinceLevelLoad;
            int totalTrueBets = 0;
            float totalPaidOut = 0;
            foreach (BetData eachBet in CurrentRaceBets)
            {


                //Check for winning bets
                if (eachBet.BetType == "win")
                {
                    foreach (RaceResultData contestant in GuestManager.CurrentRaceresults)
                    {
                        if (contestant.FinishingPlace == 1 && contestant.TurtleName == eachBet.TurtlesName)
                        {
                            //They predicted this win!
                            eachBet.didBetComeTrue = true;
                            totalTrueBets++;
                        }
                    }
                }
                if (eachBet.BetType == "place")
                {
                    foreach (RaceResultData contestant in GuestManager.CurrentRaceresults)
                    {
                        if (contestant.TurtleName == eachBet.TurtlesName)
                        {
                            if (contestant.FinishingPlace == 1 || contestant.FinishingPlace == 2)
                            {
                                eachBet.didBetComeTrue = true;
                                totalTrueBets++;
                            }
                        }
                    }
                }
                if (eachBet.BetType == "show")
                {
                    foreach (RaceResultData contestant in GuestManager.CurrentRaceresults)
                    {
                        if (contestant.TurtleName == eachBet.TurtlesName)
                        {
                            if (contestant.FinishingPlace <= 3)
                            {
                                eachBet.didBetComeTrue = true;
                                totalTrueBets++;
                            }
                        }
                    }
                }

                foreach (GuestData eachGuest in GuestManager.AllGuests)
                {

                    if (eachGuest.guestName == eachBet.BettersName)
                    {
                        if (eachBet.didBetComeTrue)
                        {
                            //Pay them if the bet won.
                            float amountToPay = eachBet.BetAmount * eachBet.BetOdds;
                            TwitchIRC tIRC = GetComponent<TwitchIRC>();
                            if (eachBet.BetType == "place")
                            {
                                amountToPay = eachBet.BetAmount + OddsDisplay.CurrentTotalPlacePool * (eachBet.BetAmount / (OddsDisplay.CurrentTotalPlacePool / 2));
                                Debug.Log("Paying out " + amountToPay + " to " + eachGuest.guestName + " for betting " + eachBet.BetAmount + " on " + eachBet.TurtlesName + " to place of a pool of " + OddsDisplay.CurrentTotalPlacePool);
                                if (!eachGuest.guestName.Contains("turtlebot"))
                                {
                                    tIRC.SendMsg("Paying out " + amountToPay + " to " + eachGuest.guestName + " for betting " + eachBet.BetAmount + " on " + eachBet.TurtlesName + "to place of a pool of " + OddsDisplay.CurrentTotalPlacePool);
                                }
                            }
                            if (eachBet.BetType == "win")
                            {
                                Debug.Log("Paying out " + amountToPay + " to " + eachGuest.guestName + " for betting " + eachBet.BetAmount + " on " + eachBet.TurtlesName + " at odds of " + eachBet.BetOdds);
                                if (!eachGuest.guestName.Contains("turtlebot"))
                                {
                                    tIRC.SendMsg("Paying out " + amountToPay + " to " + eachGuest.guestName + " for betting " + eachBet.BetAmount + " on " + eachBet.TurtlesName + " at odds of " + eachBet.BetOdds);
                                }
                            }
                            if (eachBet.BetType == "show")
                            {
                                amountToPay = eachBet.BetAmount + OddsDisplay.CurrentTotalShowPool * (eachBet.BetAmount / (OddsDisplay.CurrentTotalShowPool / 3));
                                Debug.Log("Paying out " + amountToPay + " to " + eachGuest.guestName + " for betting " + eachBet.BetAmount + " on " + eachBet.TurtlesName + " to show of a pool of " + OddsDisplay.CurrentTotalShowPool);
                                if (!eachGuest.guestName.Contains("turtlebot"))
                                {
                                    tIRC.SendMsg("Paying out " + amountToPay + " to " + eachGuest.guestName + " for betting " + eachBet.BetAmount + " on " + eachBet.TurtlesName + " to show of a pool of " + OddsDisplay.CurrentTotalShowPool);
                                }
                            }
                            eachGuest.guestCash += amountToPay;
                            totalPaidOut += amountToPay;
                            float totalPaidIn = OddsDisplay.CurrentTotalPlacePool + OddsDisplay.CurrentTotalPot + OddsDisplay.CurrentTotalShowPool;
                            print("Total paid out = " + totalPaidOut + " Total paid in = " + totalPaidIn);





                        }
                    }

                }

            }

        }
        if (timeRaceEnded > 1)
        {
            if (RaceStartingTimerObject.activeInHierarchy != true)
            {
                RaceStartingTimerObject.SetActive(true);
            }
            RaceStartingTimer.text = Mathf.RoundToInt((timeRaceEnded + (TimeBetweenRaces / 4)) - Time.timeSinceLevelLoad) + " Seconds";
            //Debug.Log("Time race ended " + timeRaceEnded + "  Time between races " + TimeBetweenRaces + "  current time " + Time.time);
            if (Time.timeSinceLevelLoad > timeRaceEnded + (TimeBetweenRaces / 2))
            {
                GuestManager.SaveGuestData();
                print("restarting");
                SceneManager.LoadScene("Pachinko");

            }
        }
    }
}
