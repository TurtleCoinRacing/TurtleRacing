using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class BetManager : MonoBehaviour
{

    public float LagDelay;

    // Update is called once per frame
    void Update()
    {

    }

    public static void GetABet(string incomingBet, string incomingBettersName, GameObject raceManagerGameObjectRef)
    {
        //print("We got a bet: "+ incomingBet);

        RaceManager racemanagerScriptRef = raceManagerGameObjectRef.GetComponent<RaceManager>();
        foreach (GameObject eachTurtle in racemanagerScriptRef.TurtlesInTheRace)
        {
            TurtleAI turtleScriptRef = eachTurtle.GetComponent<TurtleAI>();
            if (incomingBet.CaseInsensitiveContains("register"))
            {
                foreach (GuestData gD in GuestManager.AllGuests)
                {
                    if (gD.guestName == incomingBettersName)
                    {
                        gD.registeredAddress = incomingBet;
                    }
                }
            }

            if (incomingBet.CaseInsensitiveContains("bid"))
            {
                if (incomingBet.CaseInsensitiveContains(" a"))
                {
                    Debug.Log("incoming bet " + incomingBet);
                    TurtleForSale turtleForBiddingScriptRef = racemanagerScriptRef.CurrentTurtlesForSale[0].GetComponent<TurtleForSale>();
                    turtleForBiddingScriptRef.GetABid(int.Parse(Regex.Match(incomingBet, @"\d+").Value));
                }
                if (incomingBet.CaseInsensitiveContains(" b"))
                {
                    Debug.Log("incoming bet " + incomingBet);
                    TurtleForSale turtleForBiddingScriptRef = racemanagerScriptRef.CurrentTurtlesForSale[1].GetComponent<TurtleForSale>();
                    turtleForBiddingScriptRef.GetABid(int.Parse(Regex.Match(incomingBet, @"\d+").Value));
                }
                if (incomingBet.CaseInsensitiveContains(" c"))
                {
                    Debug.Log("incoming bet " + incomingBet);
                    TurtleForSale turtleForBiddingScriptRef = racemanagerScriptRef.CurrentTurtlesForSale[2].GetComponent<TurtleForSale>();
                    turtleForBiddingScriptRef.GetABid(int.Parse(Regex.Match(incomingBet, @"\d+").Value));
                }
            }

            if (incomingBet.CaseInsensitiveContains(eachTurtle.name) && RaceManager.isBettingOpen)
            {
                //bet on the turtle
                //print("Bet on this turtle " + eachTurtle.name);


                if (incomingBet.CaseInsensitiveContains("win") || incomingBet.CaseInsensitiveContains("show") || incomingBet.CaseInsensitiveContains("place"))
                {
                    //bet to win
                    string numbersInMessage = Regex.Match(incomingBet, @"\d+").Value;
                    int betAsInt = int.Parse(numbersInMessage);
                    if (betAsInt < 0)
                    {
                        return;
                    }
                    BetData thisBet = new BetData();
                    thisBet.TurtlesName = eachTurtle.name;
                    thisBet.BetAmount = betAsInt;
                    if (incomingBet.CaseInsensitiveContains("win"))
                    {
                        thisBet.BetType = "win";
                        turtleScriptRef.howMuchIsBetOnMe += thisBet.BetAmount;
                    }
                    if (incomingBet.CaseInsensitiveContains("place"))
                    {
                        thisBet.BetType = "place";
                        turtleScriptRef.howMuchIsBetOnMeToPlace += thisBet.BetAmount;
                    }
                    if (incomingBet.CaseInsensitiveContains("show"))
                    {
                        thisBet.BetType = "show";
                        turtleScriptRef.howMuchIsBetOnMeToShow += thisBet.BetAmount;
                    }
                    thisBet.BettersName = incomingBettersName;
                    thisBet.BetOdds = turtleScriptRef.myRealOdds;
                    foreach (GuestData gD in GuestManager.AllGuests)
                    {
                        if (gD.guestName == thisBet.BettersName)
                        {
                            if (gD.guestCash < thisBet.BetAmount)
                            {
                                thisBet.BetAmount = gD.guestCash;
                            }
                            if(gD.guestCash == thisBet.BetAmount){
                                GameObject bottomToaster = GameObject.Find("Toaster");
                                ToasterManager toastScriptRef = bottomToaster.GetComponent<ToasterManager>();
                                toastScriptRef.ShowAToaster( gD.guestName, " Went ALL IN!");
                            }
                            gD.guestCash -= thisBet.BetAmount;
                        }
                    }
                    racemanagerScriptRef.CurrentRaceBets.Add(thisBet);

                    TwitchIRC tIRC = raceManagerGameObjectRef.GetComponent<TwitchIRC>();
                    if (!thisBet.BettersName.Contains("turtlebot"))
                    {
                        tIRC.SendMsg("Confirmed: " + thisBet.BettersName + " Bet " + thisBet.BetAmount + " on " + thisBet.TurtlesName + " to " + thisBet.BetType);
                    }

                    //tIRC.SendCommand(".PRIVMSG #turtleracingalpha :/w turtleracingalpha this is a whisper with .");
                    //tIRC.SendCommand("/PRIVMSG #turtleracingalpha :/w turtleracingalpha this is a whisper with /");
                    //tIRC.SendCommand(":PRIVMSG #turtleracingalpha :/w turtleracingalpha this is a whisper with :");
                    //tIRC.SendMsg("/w turtleracingalpha this is a whisper with msg");

                    //Debug.Log("For <color=green>" + thisBet.BetAmount + "</color> at odds of " + thisBet.BetOdds);
                }

            }
        }

    }

    public static void FinalizeOdds(GameObject raceManagerGameObjectRef)
    {
        RaceManager racemanagerScriptRef = raceManagerGameObjectRef.GetComponent<RaceManager>();
        foreach (BetData betD in racemanagerScriptRef.CurrentRaceBets)
        {
            foreach (GameObject eachTurtle in racemanagerScriptRef.TurtlesInTheRace)
            {
                if (betD.TurtlesName == eachTurtle.name)
                {
                    TurtleAI turtleScriptRef = eachTurtle.GetComponent<TurtleAI>();
                    betD.BetOdds = turtleScriptRef.myRealOdds;
                }
            }
        }
    }



}
