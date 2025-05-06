using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // ✅ Host 본인 캐릭터 직접 생성
            GameObject hostPlayer = Instantiate(playerPrefab, spawnPoint1.position, Quaternion.identity);
            hostPlayer.name = "HostCha";
            hostPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(0);
            Debug.Log("[GameManager] Host player spawned");

            // ✅ 클라이언트용 콜백 등록
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
        }
    }

    private void OnDestroy()
    {
        // ✅ Null 방지 조건 처리
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
        }
    }

    private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (sceneName != "Game") return;
        if (clientId == 0) return; // Host는 이미 스폰 완료

        Vector3 spawnPos = spawnPoint2.position;

        GameObject clientPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        clientPlayer.name = "ClientCha";
        clientPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"[GameManager] Client player spawned (clientId={clientId}) at {spawnPos}");
    }
}
