using UnityEngine;
using DG.Tweening;

public class Bridge : MonoBehaviour
{
    private bool isSteppedOn = false;
    public AudioSource audioSource;
    public AudioClip destroyBridgeSound;

    public float dropDistance = 2f;
    public float dropDuration = 2f;
    public Ease dropEase = Ease.InBack;

    private Collider bridgeCollider;

    void Start()
    {
        bridgeCollider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isSteppedOn = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isSteppedOn)
        {
            isSteppedOn = false;

            if (audioSource && destroyBridgeSound != null)
                audioSource.PlayOneShot(destroyBridgeSound);

            if (bridgeCollider != null)
                bridgeCollider.enabled = false;

            transform.DOMoveY(transform.position.y - dropDistance, dropDuration)
                .SetEase(dropEase)
                .OnComplete(() =>
                {
                    Destroy(gameObject);
                });
        }
    }
}
