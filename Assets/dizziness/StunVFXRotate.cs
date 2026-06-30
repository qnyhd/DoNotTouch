using UnityEngine;

public class StunVFXRotate : MonoBehaviour
{
    public float rotateSpeed = 180f;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
    }
}
