using System.Collections;
using UnityEngine;

public class BreakableRock : MonoBehaviour
{
    [Header("Break")]
    public GameObject brokenPrefab;
    public ParticleSystem dustPrefab;

    [Header("Impact Condition")]
    public float minBreakVelocity = 4f;
    public LayerMask breakOnLayers = ~0;

    [Header("Explosion")]
    public float explosionForce = 6f;
    public float explosionRadius = 2f;
    public float upwardModifier = 0.4f;
    public float randomForce = 1.5f;
    public float randomTorque = 8f;

    [Header("Disappear")]
    public float chunkLifeTime = 3f;
    public float shrinkDuration = 0.8f;

    private bool hasBroken = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasBroken)
            return;

        if (((1 << collision.gameObject.layer) & breakOnLayers) == 0)
            return;

        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed < minBreakVelocity)
            return;

        Vector3 hitPoint = collision.contacts.Length > 0
            ? collision.contacts[0].point
            : transform.position;

        Break(hitPoint);
    }

    private void Break(Vector3 hitPoint)
    {
        hasBroken = true;

        if (dustPrefab != null)
        {
            ParticleSystem dust = Instantiate(dustPrefab, hitPoint, Quaternion.identity);
            dust.Play();
            Destroy(dust.gameObject, 3f);
        }

        if (brokenPrefab != null)
        {
            GameObject broken = Instantiate(brokenPrefab, transform.position, transform.rotation);

            Rigidbody[] rigidbodies = broken.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = false;
                rb.useGravity = true;

                rb.AddExplosionForce(
                    explosionForce,
                    hitPoint,
                    explosionRadius,
                    upwardModifier,
                    ForceMode.Impulse
                );

                rb.AddForce(Random.insideUnitSphere * randomForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);

                StartCoroutine(ShrinkAndDestroy(rb.transform, chunkLifeTime, shrinkDuration));
            }

            Destroy(broken, chunkLifeTime + shrinkDuration + 0.2f);
        }

        Destroy(gameObject);
    }

    private IEnumerator ShrinkAndDestroy(Transform target, float waitTime, float duration)
    {
        if (target == null)
            yield break;

        Vector3 startScale = target.localScale;

        yield return new WaitForSeconds(waitTime);

        float timer = 0f;

        while (timer < duration)
        {
            if (target == null)
                yield break;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            target.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        if (target != null)
            Destroy(target.gameObject);
    }
}