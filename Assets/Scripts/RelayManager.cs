using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // Text, InputField, Button
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Text joinCodeText;         // Host가 코드 표시하는 텍스트
    [SerializeField] private InputField joinCodeInput;  // Client가 코드 입력하는 인풋필드
    [SerializeField] private Button hostButton;         // Host 버튼
    [SerializeField] private Button clientButton;       // Client 버튼

    private string joinCode; // 코드 저장용

    private async void Start()
    {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        hostButton.onClick.AddListener(HostRelay);
        clientButton.onClick.AddListener(ClientRelay);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        hostButton.onClick.RemoveListener(HostRelay);
        clientButton.onClick.RemoveListener(ClientRelay);

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    public async void HostRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            joinCodeText.text = joinCode;

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
            joinCode = joinCodeInput.text;

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
        // (⭐) Host가 판단: 현재 연결된 사람 수가 2명 이상일 때만 이동
        if (NetworkManager.Singleton.IsHost)
        {
            int connectedClientCount = NetworkManager.Singleton.ConnectedClients.Count;

            if (connectedClientCount >= 2)  // Host(1) + Client(1)
            {
                SceneManager.LoadScene("Game");
            }
        }
    }
}
