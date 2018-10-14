using UnityEngine;

public class SimpleSpell : Spell {

    public GameObject BouncingSpell;

    protected override void OnCantMove(GameObject go) {
        CantMove = true;
        if (go != null) {
            if (go.CompareTag("Ennemy")) {
                Enemy enemy = go.GetComponent<Enemy>();
                enemy.Hit();
            } else if (go.CompareTag("BlockingBg")) {
                Vector3 v = new Vector3(0f, 0f, gameObject.transform.eulerAngles.z + 180f);
                Instantiate(BouncingSpell, gameObject.transform.position, Quaternion.Euler(v));
            }
        }
        MyDestroy();  // destroy the spell once the animation is complete
    }
}
