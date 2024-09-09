using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed; // track the player speed
    private bool isMoving; // whether player is moving
    private Vector2 input; // holds 2 values (x, y) whether player is moving up or down, left or right

    private Animator animator;

    public LayerMask solidObjectsLayer;
    public LayerMask interactablesLayer;
    public LayerMask battleLayer;

    // called once loaded 
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // called every frame
    public void HandleUpdate()
    {
        if (!isMoving)
        {
            // get the user's input
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //Debug.Log("This is input X: " + input.x);
            //Debug.Log("This is input y: " + input.y);


            // if moving left or right y axis remain unchange
            if (input.x != 0) input.y = 0;

            // diff input that is not 0
            if (input != Vector2.zero)
            {
                // animator value
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                // world position
                var targetPos = transform.position;
                // move according to the input
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (isWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }

        animator.SetBool("isMoving", isMoving);

        // press C button to interact
        if (Input.GetKeyDown(KeyCode.C))
            Interact();
    }

    void Interact()
    {
        // facing npc
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        // draw line debugging
        //Debug.DrawLine(transform.position, interactPos, Color.red, 1f);

        // is it overlap with the npc
        var collider = Physics2D.OverlapCircle(interactPos, 0.2f, interactablesLayer);
        if (collider != null)
        {
            // if is interactable run the funct
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    // start a coroutine
    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        // if there is any movement > 0 = means there is a movement
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // move towards
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;

        CheckForEncounters(); // battle zone
    }

    private bool isWalkable(Vector3 targetPos)
    {
        // if something we are overlapping
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactablesLayer) != null)
        {
            return false; // can't walk
        }
        return true;
    }

    private void CheckForEncounters() 
    {
        // if something we are overlapping
        if (Physics2D.OverlapCircle(transform.position, 0.1f, battleLayer) != null)
        {
            if (Random.Range(1, 100) <= 50)
            {
                Debug.Log("Battle started!");
            }
        }
    }
}
