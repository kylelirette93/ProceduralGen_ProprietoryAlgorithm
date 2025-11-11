using UnityEngine;

/// <summary>
/// Basic camera controller to follow player smoothly.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    Vector3 camPosition = Vector3.zero;
    float initialX;
    Camera cam;

    private void Start()
    {
        camPosition = transform.position;
        initialX = camPosition.x;
        cam = Camera.main;
    }


    private void LateUpdate()
    {
        // Lerp de lerp the camera. So smooth.
        float camHalfWidth = cam.orthographicSize * cam.aspect;
        float targetX = initialX;
        if (target.position.x > camHalfWidth)
        {
            targetX = Mathf.Lerp(transform.position.x, target.position.x, Time.deltaTime * 2f);
        }
        else if (target.position.x < -camHalfWidth)
        {
            targetX = Mathf.Lerp(transform.position.x, target.position.x, Time.deltaTime * 2f);
        }
        else
        {
            targetX = Mathf.Lerp(transform.position.x, initialX, Time.deltaTime * 2f);
        }
        transform.position = new Vector3(targetX, target.position.y, camPosition.z);
    }
}
