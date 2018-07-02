using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

    public float MoveTime;
    public LayerMask BlockingLayer;

    private BoxCollider2D _boxCollider;
    private Rigidbody2D _rb2D;
    private readonly Vector2 _shift = new Vector2(0f, 0.33f);
    private float _inverseMoveTime;

    // Use this for initialization
    protected virtual void Start () {
        MoveTime = 0.1f;
        _boxCollider = GetComponent<BoxCollider2D>();
        _rb2D = GetComponent<Rigidbody2D>();
        _inverseMoveTime = 1f / MoveTime;
	}

    protected bool Move(float xDir, float yDir, List<string> tags, out RaycastHit2D[] hits) {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        _boxCollider.enabled = false;  // we don't want to hit our own boxCollider
        hits = Physics2D.LinecastAll(start + _shift, end + _shift, BlockingLayer);  // first hit a bit above
        hits = Merge(hits, Physics2D.LinecastAll(start, end, BlockingLayer));  // second hit in the middle
        hits = Merge(hits, Physics2D.LinecastAll(start - _shift, end - _shift, BlockingLayer));  // third hit a bit under
        _boxCollider.enabled = true;
        if (hits.Any(hit => hit.transform != null && tags.Contains(hit.transform.gameObject.tag))) {
            return false;
        }
        // if there is no element to block the mouvement, we can proceed
        SmoothMovement(end);
        return true;
    }

    RaycastHit2D[] Concat(RaycastHit2D[] array1, RaycastHit2D[] array2) {
        var res = new RaycastHit2D[array1.Length + array2.Length];
        for (int i = 0; i < array1.Length; i++) {
            res[i] = array1[i];
        }
        for (int j = 0; j < array2.Length; j++) {
            res[array1.Length + j] = array2[j];  // we add our element after elements of array1 
        }
        return res;
    }

    RaycastHit2D[] Merge(RaycastHit2D[] array1, RaycastHit2D[] array2) {
        int nbrElementInBothArray = 0;
        foreach (var rc1 in array1) {
            foreach (var rc2 in array2) {
                if (rc1.Equals(rc2)) {
                    nbrElementInBothArray++;
                }
            }
        }

        var res = new RaycastHit2D[array1.Length + array2.Length - nbrElementInBothArray];
        int index = 0;
        for (int i = 0; i < array1.Length; i++) {
            bool isPresentInBothArray = false;
            for (int j = 0; j < array2.Length; j++) {
                if (array1[i].Equals(array2[j])) {
                    isPresentInBothArray = true;
                }
            }
            if (!isPresentInBothArray) {  // we add element present only in the first array
                res[index] = array1[i];
                index++;
            }
        }
        for (int i = 0; i < array2.Length; i++) {  // we add all the element of array2
            res[index] = array2[i];
            index++;
        }
        if(index != res.Length) {
            Debug.Log("error with the merging of RaycastHit2D[]");
        }
        return res;
    }

    protected void SmoothMovement(Vector3 end) {
        Vector3 newPosition = Vector3.MoveTowards(_rb2D.position, end, _inverseMoveTime * Time.deltaTime);
        _rb2D.MovePosition(newPosition);
    }


    protected void AttemptMove(float xDir, float yDir,List<string> tags) {
        RaycastHit2D[] hits;
        bool canMove = Move(xDir, yDir, tags, out hits);
        if (canMove)  // no need to continue
            return;

        //Get a component reference to the component of type T attached to the object that was hit
        int i = 0;
        Transform hitTransform = hits[i].transform;
        i++;
        while(hitTransform != null && !tags.Contains(hitTransform.tag) && i < hits.Length) {
            hitTransform = hits[i].transform;
            i++;
        }
        //If canMove is false and hitComponent is of a tag blocking the mouvement, 
        //meaning MovingObject is blocked and has hit something it can interact with.
        if (hitTransform != null && tags.Contains(hitTransform.tag)) {
            OnCantMove(hitTransform.gameObject);
        } else if (hitTransform != null) {
            Debug.Log("the object wasn't able to move, but no object block his way : " + hitTransform.gameObject.name);
            OnCantMove(null);
        }
    }

    //The abstract modifier indicates that the thing being modified has a missing or incomplete implementation.
    //OnCantMove will be overriden by functions in the inheriting classes.
    protected abstract void OnCantMove(GameObject go);

    public void pathFinding(int x, int y, int x2, int y2){

    }
}
