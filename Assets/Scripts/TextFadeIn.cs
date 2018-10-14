using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TextFadeIn : MonoBehaviour {
	
	[Range(0f, 1f)] 
	public float AlphaStart;
	[Range(0f, 1f)] 
	public float AlphaEnd;
	public float Time;
	
	
	// Use this for initialization
	void Start () {
		var graphic = GetComponent<Graphic>();
		graphic.canvasRenderer.SetAlpha(AlphaStart);
		graphic.CrossFadeAlpha(AlphaEnd, Time, false);
	}
}
