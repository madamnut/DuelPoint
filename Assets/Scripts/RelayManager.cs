using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // Text, InputField, Button
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Text joinCodeText;         // Host가 코드 표시하는 텍스트
    [SerializeField] private InputField joinCodeInput;  // Client가 코드 입력하는 인풋필드
    [SerializeField] private Button hostButton;         // Host 버튼
    [SerializeField] private Button clientButton;       // Client 버튼

    private string joinCode;

    private void Start()
    {
        // (1) 버튼 클릭 이벤트 연결
        hostButton.onClick.AddListener(HostRelay);
        clientButton.onClick.AddListener(ClientRelay);

        // (2) 네트워크 연결 완료 이벤트 등록
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        // (1) 버튼 이벤트 해제
        hostButton.onClick.RemoveListener(HostRelay);
        clientButton.onClick.RemoveListener(ClientRelay);

        // (2) 네트워크 연결 이벤트 해제
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    public async void HostRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            joinCodeText.text = joinCode;  // 생성된 코드 표시

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Host 생성 실패: {e.Message}");
        }
    }

    public async void ClientRelay()
    {
        try
        {
            joinCode = joinCodeInput.text;  // 입력한 코드 읽기

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Client 참가 실패: {e.Message}");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            SceneManager.LoadScene("Game");
        }
    }
}
