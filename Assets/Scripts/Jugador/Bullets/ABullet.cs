using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public abstract class ABullet : NetworkBehaviour
{
	public int damage;
	public float speed;
    public bool destroyOnHit;
	public float lifetime;
	public Rigidbody rb;
    public Jugador owner;
    public Teams team;
	public virtual void Initialize(Jugador player, Teams elTeamo)
	{
        Destroy(gameObject, lifetime);
        if (rb != null)
        {
			rb.AddForce(transform.forward * speed, ForceMode.Impulse);
        }
        else
        {
            if (TryGetComponent(out rb))
            {
                rb.AddForce(transform.forward * speed, ForceMode.Impulse);
            }
            Debug.LogAssertion("Compa, olvidaste el Rigidbody");
        }
    }

    public virtual void HasHit(Collision col)
    {
        if (destroyOnHit == true)
        {
            Destroy(gameObject);
        }
    }

}
