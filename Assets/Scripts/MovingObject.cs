using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

    public float moveTime = 0.00001f;
    public float moveCoef = 0.00001f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private Vector2 shift = new Vector2(0f, 0.33f);
    private float inverseMoveTime;

    // Use this for initialization
    protected virtual void Start () {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
	}

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit1, out RaycastHit2D hit2) {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2((float)xDir * moveCoef * 0.25f, (float)yDir * moveCoef * 0.25f);  // TODO nettoyer tous ça

        boxCollider.enabled = false;  // we don't want to hit our own boxCollider
        hit1 = Physics2D.Linecast(start + shift, end + shift, blockingLayer);  // first hit a bit above
        hit2 = Physics2D.Linecast(start - shift, end - shift, blockingLayer);  // second hit a bit under
        boxCollider.enabled = true;
        if(hit1.transform == null && hit2.transform == null) {
            gameObject.transform.position = end;
            SmoothMovement(end);
            return true;
        }
        return false;
    }

    protected void SmoothMovement(Vector3 end) {
        Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
        rb2D.MovePosition(newPosition);
    }


    protected virtual void AttemptMove <T> (int xDir, int yDir)
        where T : Component {
        RaycastHit2D hit1;
        RaycastHit2D hit2;
        bool canMove = Move(xDir, yDir, out hit1, out hit2);

        if(hit1.transform == null && hit2.transform == null) {
            return;
        }

        T hitComponent1 = hit1.transform.GetComponent<T>();
        T hitComponent2 = hit2.transform.GetComponent<T>();
        if (!canMove && ((hitComponent1 != null) || (hitComponent1 != null))) {
            T hitComponent = hitComponent1;
            if (hitComponent == null) {
                hitComponent = hitComponent2;
            }
            OnCantMove(hitComponent);
        }
    }

    protected abstract void OnCantMove <T> (T component)
        where T : Component;
}
