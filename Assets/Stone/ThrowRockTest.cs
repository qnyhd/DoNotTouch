using UnityEngine;

public class ThrowRockTest : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 throwDirection = new Vector3(0f, 1f, 1f);
    public float throwForce = 10f;
    public float torqueForce = 5f;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Throw();
    }

    public void Throw()
    {
        if (rb == null)
            return;

        rb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
    }
}
