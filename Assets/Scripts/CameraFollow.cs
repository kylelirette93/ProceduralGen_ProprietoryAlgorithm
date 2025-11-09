using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    Vector3 camPosition = Vector3.zero;
    float initialX;

    private void Start()
    {
        camPosition = transform.position;
        initialX = camPosition.x;
    }


    private void LateUpdate()
    {
        transform.position = new Vector3(initialX, target.position.y, camPosition.z);
    }
}
