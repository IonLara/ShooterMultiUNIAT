using System.Collections;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class PowerUpSpawner : NetworkBehaviour
{
    public PowerType maiType;
    public enum PowerType
    {
        Health = 0,
        Weapon = 1
    }
    public GameObject[] powerUps = new GameObject[2];

    private GameObject spawned;

    public float spawnTime = 5;

    public override void OnStartServer()
    {
        base.OnStartServer();
        CmdSpawn();
    }

    [Server]
    private void CmdSpawn()
    {
        var pu = powerUps[(int)maiType];
        spawned = Instantiate(pu, transform.position + pu.transform.position, pu.transform.rotation);
        spawned.GetComponent<APickUp>().Initialize(this);
        
        NetworkServer.Spawn(spawned);
        
    }

 

    public IEnumerator CollectPowerUp()
    {
        var foo = spawned.GetComponent<APickUp>();
        foo.isActive = false;

        yield return new WaitForSeconds(spawnTime);

        foo.isActive = true;
    }

}
