using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class BasiBullet : ABullet
{
    private void OnCollisionEnter(Collision collision)
    {
        HasHit(collision);
    }

    public override void HasHit(Collision col)
    {
        if (col.collider.gameObject.TryGetComponent(out Jugador playa))
        {
            playa.TakeDamage(damage, team);
        }
        base.HasHit(col);
    }
}
