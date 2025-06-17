using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class Health : NetworkBehaviour
{
    public int healing = 5;

    [SyncVar(hook = nameof(ActiveChanged))]
    public bool isActive = true;

    private PowerUpSpawner maiSpawner;

    private void ActiveChanged(bool old, bool newActive)
    {
        gameObject.SetActive(newActive);
    }

    [Server]
    public void HealPlayer(Jugador playa)
    {
        playa.IncreaseHealth(healing);
    }

    public void Initialize(PowerUpSpawner spawner)
    {
        maiSpawner = spawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (other.gameObject.TryGetComponent(out Jugador player))
        {
            HealPlayer(player);
            maiSpawner.StartCoroutine(nameof(PowerUpSpawner.CollectPowerUp));
        }
    }
}
