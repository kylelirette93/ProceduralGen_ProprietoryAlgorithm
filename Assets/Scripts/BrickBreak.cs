using UnityEngine;

public class BrickBreak : MonoBehaviour
{
    [SerializeField] ParticleSystem explosionParticles;

    /// <summary>
    /// Handles tile breakage with fancy particles.
    /// </summary>
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
