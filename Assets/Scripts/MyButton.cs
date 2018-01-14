using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class MyButton : MonoBehaviour {

    Button button;

    // Use this for initialization
    virtual public void Start () {
        button = GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
    }

    public abstract void TaskOnClick();
}
