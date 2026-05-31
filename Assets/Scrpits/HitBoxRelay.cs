using UnityEngine;

public class HitBoxRelay : MonoBehaviour
{
    public HitBox hitBox;

    void OnTriggerEnter(Collider other)
    {
        hitBox.HandleTriggerEnter(other);
    }
}
