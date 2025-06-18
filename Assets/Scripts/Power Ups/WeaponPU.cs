using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponPU : APickUp
{
    public WeaponObject weaponType;

    [Server]
    public override void TakeEffect(Jugador playa)
    {
        base.TakeEffect(playa);
        playa.currentWeapon = weaponType.ToData().data;
        playa.currentProjectile = weaponType.ToData().projectile;
    }
}
