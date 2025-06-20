using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float moveDistance = 1f;
    public float moveDuration = 0.833f; 
    public float rotateDuration = 0.5f; 
    public float rotationAngle = 90f;
    public float pickUpRange = 1f;

    public Transform mouthPoint; 
    private GameObject carriedStick = null;
    private Vector3 stickOffsetFromPickPoint;

    private Animator animator;
    private bool isBusy = false;
    private Queue<IEnumerator> actionQueue = new Queue<IEnumerator>();

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("WigglingTail");
        Invoke("PlayIdle", 1f);
    }

    void PlayIdle()
    {
        animator.Play("Breathing");
    }

    void Update()
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            EnqueueAction(Move(camForward));
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            EnqueueAction(Move(-camForward));
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            EnqueueAction(Move(-camRight));
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            EnqueueAction(Move(camRight));
        else if (Input.GetKeyDown(KeyCode.Space))
            EnqueueAction(HandleStick());
        else if (Input.GetKeyDown(KeyCode.Q))
            EnqueueAction(Rotate(-rotationAngle));
        else if (Input.GetKeyDown(KeyCode.E))
            EnqueueAction(Rotate(rotationAngle));
    }

    void EnqueueAction(IEnumerator action)
    {
        actionQueue.Enqueue(action);
        if (!isBusy)
            StartCoroutine(HandleActions());
    }

    IEnumerator HandleActions()
    {
        isBusy = true;
        while (actionQueue.Count > 0)
        {
            yield return StartCoroutine(actionQueue.Dequeue());
        }
        isBusy = false;
        animator.Play("Breathing");
    }

    IEnumerator Move(Vector3 direction)
    {
        animator.Play("Walking02");
        Vector3 start = transform.position;
        Vector3 end = start + direction.normalized * moveDistance;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }
    IEnumerator Rotate(float angle)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, angle, 0f);
        float elapsed = 0f;
        while (elapsed < rotateDuration)
        {
            transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / rotateDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRot;
    }
    IEnumerator HandleStick()
    {
        animator.Play("EatingCycle");

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("EatingCycle"));
        float animDuration = animator.GetCurrentAnimatorStateInfo(0).length;

        if (carriedStick == null)
        {
            yield return new WaitForSeconds(1f);

            Collider[] hits = Physics.OverlapSphere(transform.position, pickUpRange);
            GameObject nearestStick = null;
            float minDist = float.MaxValue;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Stick"))
                {
                    float dist = Vector3.Distance(hit.transform.position, transform.position);
                    if (dist < minDist)
                    {
                        nearestStick = hit.gameObject;
                        minDist = dist;
                    }
                }
            }

            if (nearestStick != null)
            {
                Transform pickA = nearestStick.transform.Find("PickPointA");
                Transform pickB = nearestStick.transform.Find("PickPointB");

                if (pickA == null || pickB == null)
                    yield break;

                float distA = Vector3.Distance(pickA.position, mouthPoint.position);
                float distB = Vector3.Distance(pickB.position, mouthPoint.position);
                Transform chosenPickPoint = distA < distB ? pickA : pickB;

                stickOffsetFromPickPoint = nearestStick.transform.position - chosenPickPoint.position;
                Vector3 targetPos = mouthPoint.position + stickOffsetFromPickPoint;

                float lerpDuration = animDuration - 1f;
                float elapsed = 0f;

                Vector3 startPos = nearestStick.transform.position;
                Quaternion startRot = nearestStick.transform.rotation;

                nearestStick.GetComponent<Collider>().enabled = false;

                while (elapsed < lerpDuration)
                {
                    nearestStick.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / lerpDuration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                nearestStick.transform.position = targetPos;
                nearestStick.transform.rotation = startRot; 
                nearestStick.transform.SetParent(mouthPoint);
                carriedStick = nearestStick;
            }
        }
        else
        {
            carriedStick.transform.SetParent(null);
            Vector3 dropTargetPoint = transform.position + transform.forward * 0.6f + Vector3.up * 0.2f;
            Vector3 finalStickPosition = dropTargetPoint + stickOffsetFromPickPoint;

            carriedStick.transform.position = finalStickPosition;
            carriedStick.GetComponent<Collider>().enabled = true;
            carriedStick = null;

            yield return new WaitForSeconds(animDuration);
        }
    }
}
