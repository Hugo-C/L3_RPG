using UnityEngine;

public class Exit : MonoBehaviour {
    private LevelManager _levelManager;

	// Use this for initialization
	void Start () {
		_levelManager = (LevelManager) GameObject.Find("LevelManager").GetComponent(typeof(LevelManager));
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.CompareTag("Player")) {
            _levelManager.LoadScene("main", 2);

            // TODO smooth transition from player
            GameObject dirLight = GameObject.Find("Directional Light");
            Light lightComponent = dirLight.GetComponent<Light>();
            lightComponent.intensity = 0.5f;
            GameObject player = GameObject.Find("Player");
            player.SetActive(false);
        }
    }
}
