using CosmicRunner.Core;
using UnityEngine;

namespace CosmicRunner.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target & Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 3.8f, -6.5f);
        [SerializeField] private float smoothSpeed = 10f;
        [SerializeField] private float lookAheadDistance = 12f;

        private Vector3 velocity = Vector3.zero;

        private void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) target = player.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Vị trí camera mong muốn (bám theo người chơi)
            Vector3 desiredPosition = new Vector3(
                Mathf.Lerp(transform.position.x, target.position.x * 0.6f, smoothSpeed * Time.deltaTime),
                target.position.y + offset.y,
                target.position.z + offset.z
            );

            transform.position = desiredPosition;

            // Camera nhìn về phía trước trên đường chạy
            Vector3 lookTarget = target.position + (Vector3.forward * lookAheadDistance);
            lookTarget.y = target.position.y + 1.0f;
            transform.LookAt(lookTarget);
        }
    }
}
