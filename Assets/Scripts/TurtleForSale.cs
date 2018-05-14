using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurtleForSale : MonoBehaviour
{

    // Use this for initialization

    public TextMeshProUGUI titleUI, accellUI, endUI, favSurfUI, bidUI;

    public string topBiddersName;

    public int currentBid;

    int rA, rB, rC;
    void Start()
    {
        float statsTotal = Random.Range(7,15);

        float fN = statsTotal - (statsTotal/2.5f);
        statsTotal -= fN;

        rA = Mathf.RoundToInt(statsTotal);
        rB = Mathf.RoundToInt(fN);
        rC = Random.Range(1, 4);
        accellUI.text = "Acceleration: " + rA;
        endUI.text = "Endurance: " + rB;
        string favSurfaceText = " ";
        if (rC == 1)
        {
            favSurfaceText = "Grass";
        }
		if (rC == 2)
        {
            favSurfaceText = "Sand";
        }
		if (rC == 3)
        {
            favSurfaceText = "Dirt";
        }


        favSurfUI.text = "Favorite Surface: " + favSurfaceText;

    }

    // Update is called once per frame
    void Update()
    {
        bidUI.text = "Current Bid: " + currentBid;
    }

    public void GetABid(int bid){
        if(bid>currentBid){
            currentBid = bid;
        }
    }
}
