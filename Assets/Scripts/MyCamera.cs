using UnityEngine;
using System.Collections;

public class MyCamera : MonoBehaviour {

    public float Smoothing = 0.4f;
    
    private GameObject _player;
    private Vector3 _offset;

    // Use this for initialization
    void Start() {
        _player = GameObject.Find("Player");
        _offset = new Vector3(0, 0, -10);
        transform.position = _player.transform.position + _offset;
    }

    // LateUpdate is called after Update each frame
    private void LateUpdate() {
        if (_player != null) {
            //transform.position = Vector3.Lerp(transform.position, _player.transform.position + _offset, Smoothing);  // smooth transition (but appear laggy)
            transform.position =_player.transform.position + _offset;
        } else {
            Debug.LogWarning("camera can't find the player");
            _player = GameObject.Find("Player");
        }
    }
}