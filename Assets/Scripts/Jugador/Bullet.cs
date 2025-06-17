using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class Bullet : NetworkBehaviour
{
    public int damage;
    public Rigidbody rb;
    private NetworkConnectionToClient maiOwner;
    [Server]
    public void Initialize(NetworkConnectionToClient owner, float force)
    {
        maiOwner = owner;
        rb.AddForce(force * transform.forward, ForceMode.Impulse);
    }

   
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent(out Jugador playa))
        {
            playa.TakeDamage(damage, Teams.Alpha);
            Debug.Log("Hit a" + playa.name);
        }
    }
    
}
