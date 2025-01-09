using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject ARPlayerPrefab;
    public GameObject DesktopPlayerPrefab;
    public GameObject ImagePrefab;

    public void PlayerJoined(PlayerRef player)
    {
        NetworkObject spawned = null;
        if (player == Runner.LocalPlayer)
        {
            Debug.Log($"PlatformManager.IsDesktop(): {PlatformManager.IsDesktop()}");

            if(PlatformManager.IsDesktop()){
                /*Runner.Spawn(ImagePrefab,  new Vector3(0, 1, 0), Quaternion.identity);
                return;*/
                spawned = Runner.Spawn(DesktopPlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
                Runner.SetPlayerObject(Runner.LocalPlayer, spawned);
                Debug.Log("BCZ disabilito AR controller");
                spawned.gameObject.GetComponent<DesktopSphereController>().enabled = true;
                spawned.gameObject.GetComponent<ARSphereController>().enabled = false;
                spawned.gameObject.GetComponent<DisplayConnector>().enabled = false;
                return;
            }
            spawned = Runner.Spawn(ARPlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            /*spawned.gameObject.GetComponent<DesktopSphereController>().enabled = false;
            spawned.gameObject.GetComponent<ARSphereController>().enabled = true;
            spawned.gameObject.GetComponent<DisplayConnector>().enabled = true;*/
            Debug.Log("Spawnato : " + spawned.name);
            Runner.SetPlayerObject(Runner.LocalPlayer, spawned);
        }
    }
}
