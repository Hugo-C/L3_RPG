using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

    public float moveTime;
    public float moveCoef;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private Vector2 shift = new Vector2(0f, 0.33f);
    private float inverseMoveTime;

    // Use this for initialization
    protected virtual void Start () {
        moveTime = 0.1f;
        moveCoef = 0.1f;
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
	}

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit1, out RaycastHit2D hit2, out RaycastHit2D hit3) {
        Vector2 start = transform.position;
        
        float x = xDir * moveCoef;
        float y = yDir * moveCoef;
        Vector2 end = start + new Vector2(x, y);

        boxCollider.enabled = false;  // we don't want to hit our own boxCollider
        hit1 = Physics2D.Linecast(start + shift, end + shift, blockingLayer);  // first hit a bit above
        hit2 = Physics2D.Linecast(start, end, blockingLayer);  // second hit in the middle
        hit3 = Physics2D.Linecast(start - shift, end - shift, blockingLayer);  // third hit a bit under
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


    protected void AttemptMove(int xDir, int yDir) {
        RaycastHit2D hit1;
        RaycastHit2D hit2;
        RaycastHit2D hit3;
        bool canMove = Move(xDir, yDir, out hit1, out hit2, out hit3);
        
        if (hit1.transform == null && hit2.transform == null && hit3.transform == null)
            return;

        //Get a component reference to the component of type T attached to the object that was hit
        Transform hitTransform = hit1.transform;
        if(hitTransform == null) {
            hitTransform = hit2.transform;
        }
        if (hitTransform == null) {
            hitTransform = hit3.transform;
        }
         //If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
        if (!canMove && hitTransform != null)
         OnCantMove(hitTransform.gameObject);
    }

    //The abstract modifier indicates that the thing being modified has a missing or incomplete implementation.
    //OnCantMove will be overriden by functions in the inheriting classes.
    protected abstract void OnCantMove(GameObject gameObject);
}
