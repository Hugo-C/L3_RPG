using UnityEngine;
using UnityEngine.UI;

public abstract class MyButton : MonoBehaviour {
    private Button _button;

    // Use this for initialization
    void Start () {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(TaskOnClick);
    }

    public abstract void TaskOnClick();
}
