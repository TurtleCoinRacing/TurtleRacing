using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour {

	public TextMeshPro tm;
	// Use this for initialization
	void Start () {
         tm = GetComponent<TextMeshPro>();
     
	}
	
	// Update is called once per frame
	void Update () {
		if(tm.color.a <0.5f){
			tm.color = new Color(tm.color.r, tm.color.g, tm.color.b, tm.color.a *0.92f);
		}
		else{
		tm.color = new Color(tm.color.r, tm.color.g, tm.color.b, tm.color.a *0.987f);
		}
		transform.Translate(Vector3.up * Time.deltaTime * 1.15f);
	}
}
