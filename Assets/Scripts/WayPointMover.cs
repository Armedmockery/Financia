using System.Collections;
using UnityEngine;

public class WayPointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float moveSpeed = 2f;
    public float waitTime = 2f;
    public bool loopWaypoints = true;

    private Transform[] waypoints;
    private int currentWaypointIndex;
    private bool isWaiting;

    // 🔥 NEW
    private Animator animator;
    private Vector2 lastMoveDir;

    void Start()
    {
        waypoints = new Transform[waypointParent.childCount];
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        }

        // 🔥 NEW
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isWaiting)
            MoveToWaypoint();
    }

    void MoveToWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];

        // 🔥 NEW — get movement direction
        Vector2 direction = (target.position - transform.position).normalized;

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // 🔥 NEW — if moving, play walk animation
        if (Vector2.Distance(transform.position, target.position) > 0.1f)
        {
            animator.SetBool("IsMoving", true);
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
            lastMoveDir = direction;
        }
        else
        {
            animator.SetBool("IsMoving", false);
            animator.SetFloat("MoveX", lastMoveDir.x);
            animator.SetFloat("MoveY", lastMoveDir.y);

            StartCoroutine(WaitWaypoint());
        }
    }

    IEnumerator WaitWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        currentWaypointIndex = loopWaypoints
            ? (currentWaypointIndex + 1) % waypoints.Length
            : Mathf.Min(currentWaypointIndex + 1, waypoints.Length - 1);

        isWaiting = false;
    }
}
