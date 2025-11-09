using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float movementSpeed = 5f;

    public void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        transform.Translate(new Vector3(horizontal * movementSpeed * Time.deltaTime, 0, 0));
    }
}
