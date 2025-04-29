using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;

    private void Update()
    {
        // (1) 자기 자신의 오브젝트만 조작
        if (!IsOwner) return;

        // (2) 이동
        float h = Input.GetAxis("Horizontal"); // A, D
        float v = Input.GetAxis("Vertical");   // W, S

        Vector3 move = new Vector3(h, 0, v) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.Self);

        // (3) 마우스 좌우 회전
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, mouseX * rotateSpeed * Time.deltaTime);
    }
}
