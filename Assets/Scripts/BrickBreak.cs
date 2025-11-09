using UnityEngine;

public class BrickBreak : MonoBehaviour
{
    [SerializeField] ParticleSystem explosionParticles;


    public void BreakTile()
    {
        if (explosionParticles != null)
        {
            GameObject temp = Instantiate(explosionParticles.gameObject, transform.position, Quaternion.identity);
            AudioManager.Instance.PlaySound("BrickBreak");
            Destroy(gameObject);
        }
    }
}
