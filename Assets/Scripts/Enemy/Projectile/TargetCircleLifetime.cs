using UnityEngine;

public class TargetCircleLifetime : MonoBehaviour
{
    public float maxLifeTime = 6f;

    private void Start()
    {
        Destroy(gameObject, maxLifeTime);
    }
}