using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;


public class TurtleAI : MonoBehaviour {


	public float BaseSpeed, BaseSpeedAnimMultiplier, SpeedChangeTickRate;
	bool hasRaceStarted, hasPickedMaterial, didIFinish;
	private Animator myAnimator;
	private float SpeedChangeTimer, secondWindTimer, idleCounter;
	public static string usedMats ="", RaceResultsColumn1="", RaceResultsColumn2="", RaceResultsColumn3="";
	public static int HowManyTurtlesFinished;
	public float myOdds = 23, howMuchIsBetOnMe = 0, howMuchIsBetOnMeToShow = 0, howMuchIsBetOnMeToPlace = 0;
	public float myRealOdds, percentFinished;
	public CinemachineVirtualCamera ShoulderCam;
	public GameObject RaceManagerReference, UIName, UIOdds, UIOddsTitle, OverHeadName;
	public Material [] PossibleMaterials;
	public SkinnedMeshRenderer theMesh;
	private TextMeshProUGUI oddsGui;
	RaceManager RaceManagerScriptRef;

	// Use this for initialization
	void Start () {
		RaceResultsColumn1 = "";
		RaceResultsColumn2="";
		RaceResultsColumn3="";
		idleCounter = Random.Range(0,15);
		usedMats="";
		
		HowManyTurtlesFinished = 0;
		RaceManagerScriptRef =  RaceManagerReference.GetComponent<RaceManager>();
		myAnimator = gameObject.GetComponent<Animator> ();
		BaseSpeed += Random.Range (-10,10)*.01f;
		SpeedChangeTimer = SpeedChangeTickRate;
		oddsGui = UIOdds.GetComponent<TextMeshProUGUI>();
		//TurtleNamer TurlteNamerScriptRef = RaceManagerReference.GetComponent<TurtleNamer>();
		//gameObject.name = TurlteNamerScriptRef.GiveNewRandomName();
		//gameObject.name = TurlteNamerScriptRef.PossibleNames[Random.Range(0,TurlteNamerScriptRef.PossibleNames.Length -1)];
		UIName.GetComponent<TextMeshProUGUI>().text = gameObject.name;
	}
	
	// Update is called once per frame
	void Update () {
		percentFinished = transform.position.z / RaceManagerScriptRef.FinishLineLocator.transform.position.z;
		if(!hasPickedMaterial){	//To pick a random color
			Material matToTryThisTime = PossibleMaterials[Random.Range(0,PossibleMaterials.Length)];
			theMesh.material = matToTryThisTime;
			if(usedMats.Contains(matToTryThisTime.name)){
				return;
			}
			usedMats = usedMats + matToTryThisTime.name;
			hasPickedMaterial = true;
			UIName.GetComponent<TextMeshProUGUI>().color = matToTryThisTime.color;
			oddsGui.color = matToTryThisTime.color;
			//oddsGui.text = oddsReturner(myOdds);
			
			UIOddsTitle.GetComponent<TextMeshProUGUI>().color = matToTryThisTime.color;
			OverHeadName.GetComponent<TextMeshProUGUI>().color = matToTryThisTime.color;
			OverHeadName.GetComponent<TextMeshProUGUI>().text = gameObject.name;
		}

		if(!RaceManagerScriptRef.hasRaceStarted){
			if(idleCounter < Time.time){
				myAnimator.SetTrigger("Play Idle");
				idleCounter = Time.time + Random.Range(10,20);
			}

			//calculate odds
			if(howMuchIsBetOnMe > 0){
				myRealOdds = Mathf.RoundToInt((OddsDisplay.CurrentTotalPot / howMuchIsBetOnMe));
				
				
				//oddsGui.text = oddsReturner(myRealOdds);
				//int oX = Mathf.RoundToInt( myRealOdds * 100);
				//Fraction showtheseOdds = RealToFraction(oX,0);
				
				//print("odds = "+myRealOdds+ " betonme= "+ howMuchIsBetOnMe + " total= " + OddsDisplay.CurrentTotalPot );
	
				//oddsGui.text =myRealOdds.ToString()+"/1";
				//oddsGui.text =  System.Math.Round(myRealOdds,2).ToString();
				//print("My real odds" + myRealOdds + " total pot" + OddsDisplay.CurrentTotalPot + " on me " + howMuchIsBetOnMe);
			}
			else{
				myRealOdds = OddsDisplay.CurrentTotalPot;
			}
			oddsGui.text =myRealOdds.ToString()+"/1";
		}

		if (RaceManagerScriptRef.hasRaceStarted) {
			myAnimator.SetTrigger ("Start Walking");
			myAnimator.SetFloat ("MoveAnimSpeed", BaseSpeed / BaseSpeedAnimMultiplier);
			transform.Translate (Vector3.forward * BaseSpeed * Time.deltaTime);
		}
        if (Time.time >= SpeedChangeTimer && RaceManagerScriptRef.hasRaceStarted){
            if (percentFinished < 0.8f){
                SpeedChangeTimer = Time.time + SpeedChangeTickRate;
                BaseSpeed += Random.Range(-50, 50) * .001f;
                if (BaseSpeed <= 0.83f){
                    BaseSpeed = 1.5f;
                    print(gameObject.name + " got a second wind!");
                    ShoulderCam.Priority += 2;
                    secondWindTimer = Time.time;
						GameObject bottomToaster = GameObject.Find("Toaster");
                		ToasterManager toastScriptRef = bottomToaster.GetComponent<ToasterManager>();
						toastScriptRef.ShowAToaster(gameObject.name, "Got a second wind!");
				}
                if (ShoulderCam.Priority != 10){
                    if (secondWindTimer + 3 < Time.time){
                        ShoulderCam.Priority = 10;
                    }
                }
                if (BaseSpeed <= 1.45 && ShoulderCam.Priority > 10){
                    ShoulderCam.Priority = 10;
                }
            }
			if(percentFinished>1){ //we passed the finish line
				if(BaseSpeed > 0.01f){
					BaseSpeed -= 0.002f;
				}

			}

			//print ("Changed speeds" + Time.time + "   " +SpeedChangeTimer + "   " + SpeedChangeTickRate);
		}
		//float dist = Vector3.Distance (FinishLineLocator.transform.position, transform.position);
		//print(gameObject.name + dist);
		if(gameObject.transform.position.z>RaceManagerScriptRef.FinishLineLocator.transform.position.z && !didIFinish){
			//record race result.
			RaceResultData aSingleResult = new RaceResultData();
			aSingleResult.TurtleName = gameObject.transform.name;
			aSingleResult.FinishingPlace = HowManyTurtlesFinished + 1;
			//aSingleResult.odds = myRealOdds;
			//aSingleResult.totalPool = howMuchIsBetOnMe;
			GuestManager.CurrentRaceresults.Add(aSingleResult);
			RaceResultsColumn1 += (HowManyTurtlesFinished +1) +")<nobr>"+gameObject.name + " \n";
			RaceResultsColumn2 += myRealOdds.ToString() +"/1\n";
			RaceResultsColumn3 += howMuchIsBetOnMe.ToString()+"\n";
			//RaceResults += "</nobr>" + (HowManyTurtlesFinished+1) + ". " + gameObject.name + " - Total Pool: " + howMuchIsBetOnMe + " Odds: " +myRealOdds +"</nobr> \n ";
			HowManyTurtlesFinished++;
			//print(RaceResults);
			didIFinish = true;
		}


		
	}

	string oddsReturner(float o){
		if(o <= 0.2f)	return("1/5"); //0.2
		if(o <= 0.4f)	return("2/5"); //0.4
		if(o <=0.5f)	return("1/2"); //0.5
		if(o <=0.6f)	return("3/5"); //0.6
		if(o <=0.8f)	return("4/5"); //0.8
		if(o <=1)	return("1/1"); //1.0
		if(o <=1.2f)	return("6/5"); //1.2
		if(o <=1.4f)	return("7/5"); //1.4
		if(o <=1.5f)	return("3/2"); //1.5
		if(o <= 1.6f)	return("8/5"); //1.6
		if(o <= 1.8f)	return("9/5"); //1.8
		if(o <= 2)	return("2/1"); //2.0
		if(o <= 2.5f)	return("5/2"); //2.5
		if(o <= 3)	return("3/1"); //3
		if(o <= 3.5f)	return("7/2"); //3.5
		if(o <= 4)	return("4/1"); //4
		if(o <= 4.5f)	return("9/2"); //4.5
		if(o <= 5)	return("5/1"); //5
		if(o <= 6)	return("6/1"); 
		if(o <= 7)	return("7/1"); 
		if(o <= 8)	return("8/1"); 
		if(o <= 9)	return("9/1"); 
		if(o <= 10)	return("10/1");
		if(o <= 15)	return("15/1");
		if(o <= 20)	return("20/1");
		if(o <= 30)	return("30/1");
		if(o <= 50)	return("50/1");
		return("None");
	}

	public Fraction RealToFraction(double value, double accuracy)
{
    if (accuracy <= 0.0 || accuracy >= 1.0)
    {
		
        //throw new  ArgumentOutOfRangeException("accuracy", "Must be > 0 and < 1.");
    }

    int sign = System.Math.Sign(value);

    if (sign == -1)
    {
        value = System.Math.Abs(value);
    }

    // Accuracy is the maximum relative error; convert to absolute maxError
    double maxError = sign == 0 ? accuracy : value * accuracy;

    int n = (int) System.Math.Floor(value);
    value -= n;

    if (value < maxError)
    {
        return new Fraction(sign * n, 1);
    }

    if (1 - maxError < value)
    {
        return new Fraction(sign * (n + 1), 1);
    }

    // The lower fraction is 0/1
    int lower_n = 0;
    int lower_d = 1;

    // The upper fraction is 1/1
    int upper_n = 1;
    int upper_d = 1;

    while (true)
    {
        // The middle fraction is (lower_n + upper_n) / (lower_d + upper_d)
        int middle_n = lower_n + upper_n;
        int middle_d = lower_d + upper_d;

        if (middle_d * (value + maxError) < middle_n)
        {
            // real + error < middle : middle is our new upper
            upper_n = middle_n;
            upper_d = middle_d;
        }
        else if (middle_n < (value - maxError) * middle_d)
        {
            // middle < real - error : middle is our new lower
            lower_n = middle_n;
            lower_d = middle_d;
        }
        else
        {
            // Middle is our best fraction
            return new Fraction((n * middle_d + middle_n) * sign, middle_d);
        }
    }
}
public struct Fraction
{
    public Fraction(int n, int d)
    {
        N = n;
        D = d;
    }

    public int N { get; private set; }
    public int D { get; private set; }
}
}
