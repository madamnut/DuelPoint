using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // 플레이어 프리팹

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // Host 스폰 (예: 원점)
            SpawnPlayer(new Vector3(0f, 0f, 0f));
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            // Client 스폰 (예: 오른쪽 5칸 떨어진 위치)
            SpawnPlayer(new Vector3(5f, 0f, 0f));
        }
    }

    private void SpawnPlayer(Vector3 spawnPosition)
    {
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
    }
}
