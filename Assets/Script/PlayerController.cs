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

    public AudioSource audioSource;
    public AudioClip stickHitRockSound;
    public AudioClip winSound;

    public Transform mouthPoint;

    private GameObject carriedStick = null;
    private Vector3 stickOffsetFromPickPoint; 
    private Vector3 playerInitialPosition;
    private Vector3 stickOriginalWorldPosition;
    private Quaternion playerInitialRotation;

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
        if (!isBusy) StartCoroutine(HandleActions());
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
        Vector3 start = transform.position;
        Vector3 end = start + direction.normalized * moveDistance;

        if (!IsGrounded(end))
        {
            yield break;
        }

        RaycastHit hitLow;
        RaycastHit hitHigh;

        bool hitLowRock = Physics.Raycast(start + Vector3.up * 0.2f, direction.normalized, out hitLow, moveDistance + 0.1f) && hitLow.collider.CompareTag("Rock");
        bool hitHighRock = Physics.Raycast(start + Vector3.up * 1.0f, direction.normalized, out hitHigh, moveDistance + 0.1f) && hitHigh.collider.CompareTag("Rock");

        if (hitLowRock || hitHighRock)
        {
            yield break;
        }

        animator.Play("Walking02");

        float totalElapsed = 0f;

        while (totalElapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, totalElapsed / moveDuration);
            totalElapsed += Time.deltaTime;

            if (carriedStick != null && IsStickHittingRock())
            {
                if (audioSource && stickHitRockSound != null)
                {
                    audioSource.PlayOneShot(stickHitRockSound);
                }
                transform.position = start;
                yield break;
            }

            yield return null;
        }

        transform.position = end;

        if (carriedStick != null && IsStickHittingRock())
        {
            if (audioSource && stickHitRockSound != null)
            {
                audioSource.PlayOneShot(stickHitRockSound);
            }
            transform.position = start;
        }

        if (carriedStick != null && !IsStickHittingRock())
        {
            playerInitialPosition = transform.position;
            playerInitialRotation = transform.rotation;
        }
    }

    bool IsGrounded(Vector3 targetPosition)
    {
        Ray ray = new Ray(targetPosition + Vector3.up * 1f, Vector3.down);
        return Physics.Raycast(ray, 3f, LayerMask.GetMask("Ground"));
    }

    IEnumerator Rotate(float angle)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, angle, 0f);
        float elapsed = 0f;

        while (elapsed < rotateDuration)
        {
            transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / rotateDuration);

            if (carriedStick != null && IsStickHittingRock())
            {
                if (audioSource && stickHitRockSound != null)
                {
                    audioSource.PlayOneShot(stickHitRockSound);
                }
                transform.rotation = startRot;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;

        if (carriedStick != null && IsStickHittingRock())
        {
            if (audioSource && stickHitRockSound != null)
            {
                audioSource.PlayOneShot(stickHitRockSound);
            }
            transform.rotation = startRot;
        }

        if (carriedStick != null && !IsStickHittingRock())
        {
            playerInitialPosition = transform.position;
            playerInitialRotation = transform.rotation;
        }
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
                float angleBetween = Vector3.Angle(transform.forward, nearestStick.transform.forward);
                if (angleBetween < 45f || angleBetween > 135f || angleBetween < 90f)
                {
                    yield break;
                }

                Transform pickA = nearestStick.transform.Find("PickPointA");
                Transform pickB = nearestStick.transform.Find("PickPointB");
                Transform pickC = nearestStick.transform.Find("PickPointC");

                Transform chosenPickPoint = null;
                float minPickDist = float.MaxValue;

                if (pickA != null)
                {
                    float distA = Vector3.Distance(pickA.position, mouthPoint.position);
                    if (distA < minPickDist) { minPickDist = distA; chosenPickPoint = pickA; }
                }
                if (pickB != null)
                {
                    float distB = Vector3.Distance(pickB.position, mouthPoint.position);
                    if (distB < minPickDist) { minPickDist = distB; chosenPickPoint = pickB; }
                }
                if (pickC != null)
                {
                    float distC = Vector3.Distance(pickC.position, mouthPoint.position);
                    if (distC < minPickDist) { minPickDist = distC; chosenPickPoint = pickC; }
                }

                if (chosenPickPoint == null) yield break;

                stickOffsetFromPickPoint = nearestStick.transform.position - chosenPickPoint.position;
                stickOriginalWorldPosition = transform.InverseTransformPoint(nearestStick.transform.position);

                Vector3 targetPos = mouthPoint.position + stickOffsetFromPickPoint;

                float lerpDuration = animDuration - 1f;
                float elapsed = 0f;

                Vector3 startPos = nearestStick.transform.position;
                Quaternion startRot = nearestStick.transform.rotation;

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

                playerInitialPosition = transform.position;
                playerInitialRotation = transform.rotation;
            }
        }
        else
        {
            carriedStick.transform.SetParent(null);

            Vector3 dropPosition = transform.TransformPoint(stickOriginalWorldPosition);
            carriedStick.transform.position = dropPosition;

            carriedStick = null;

            yield return new WaitForSeconds(animDuration);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Paw") && carriedStick != null)
        {
            StartCoroutine(HandleWin(other));
        }
    }

    IEnumerator HandleWin(Collider other)
    {
        if (audioSource && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }

        yield return new WaitForSeconds(1f);

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnLevelCompleted();
        }
    }

    bool IsStickHittingRock()
    {
        if (carriedStick == null) return false;

        Collider stickCollider = carriedStick.GetComponent<Collider>();
        if (stickCollider == null) return false;

        Bounds bounds = stickCollider.bounds;

        Collider[] hits = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, LayerMask.GetMask("Default"));
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Rock"))
                return true;
        }

        return false;
    }
}
