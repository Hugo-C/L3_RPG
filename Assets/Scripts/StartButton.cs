using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButton : MyButton {
    private Text _text;
    private Color _original;
    private Color _originalTransparent;
    private Vector3 _originalScale;
    private Vector3 _maxScale;
    
    protected override void Start() {
        _text = GetComponent<Text>();
        _original = _text.color;
        _originalTransparent = new Color(_original.r, _original.g, _original.b, 0.2f);
        _originalScale = transform.localScale;
        _maxScale = new Vector3(_originalScale.x * 1.3f, _originalScale.y * 1.3f);
        base.Start();
    }

    private void Update() {
        _text.color =  Color.Lerp(_original, _originalTransparent, Mathf.PingPong(Time.time / 2f, 1));
        transform.localScale = Vector3.Lerp(_originalScale, _maxScale, 1 - Mathf.PingPong(Time.time / 2f, 1));
    }

    public override void TaskOnClick() {
        LevelManager levelManager = LevelManager.Instance;
        levelManager.LoadScene("main");
    }
}
