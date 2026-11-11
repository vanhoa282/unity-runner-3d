using System.Collections.Generic;
using CosmicRunner.Core;
using UnityEngine;

namespace CosmicRunner.World
{
    public class TrackManager : MonoBehaviour
    {
        public static TrackManager Instance { get; private set; }

        [Header("Track Generation")]
        [SerializeField] private float segmentLength = 40.0f;
        [SerializeField] private int activeSegmentsCount = 6;
        [SerializeField] private float laneDistance = 3.0f;

        [Header("References")]
        [SerializeField] private Transform playerTransform;

        private readonly List<GameObject> activeSegments = new List<GameObject>();
        private float spawnZ = 0.0f;
        private readonly float safeDistanceBeforeObstacles = 40.0f;

        // Vật liệu thủ tục
        private Material roadMaterial;
        private Material obstacleMaterial;
        private Material coinMaterial;

        private void Awake()
        {
            Instance = this;
            CreateProceduralMaterials();
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }

            ResetTrack();
        }

        private void Update()
        {
            if (playerTransform == null || activeSegments.Count == 0) return;

            // Khi người chơi vượt qua nửa phân đoạn đầu tiên, tái sử dụng và sinh phân đoạn mới
            if (playerTransform.position.z - segmentLength > activeSegments[0].transform.position.z)
            {
                RecycleSegment();
            }
        }

        public void ResetTrack()
        {
            foreach (var seg in activeSegments)
            {
                Destroy(seg);
            }
            activeSegments.Clear();
            spawnZ = 0.0f;

            for (int i = 0; i < activeSegmentsCount; i++)
            {
                SpawnSegment(i == 0); // Phân đoạn đầu tiên không có chướng ngại vật
            }
        }

        private void SpawnSegment(bool isSafeZone = false)
        {
            GameObject segment = new GameObject($"TrackSegment_{spawnZ}");
            segment.transform.SetParent(transform);
            segment.transform.position = new Vector3(0, 0, spawnZ);

            // Tạo mặt đường không gian (3 làn)
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Road";
            road.transform.SetParent(segment.transform);
            road.transform.localPosition = new Vector3(0, -0.25f, segmentLength / 2f);
            road.transform.localScale = new Vector3(laneDistance * 3.5f, 0.5f, segmentLength);
            
            var roadRenderer = road.GetComponent<MeshRenderer>();
            if (roadRenderer != null) roadRenderer.material = roadMaterial;

            // Tạo dải đèn neon biên 2 bên đường
            CreateNeonBorder(segment.transform, -laneDistance * 1.8f, segmentLength);
            CreateNeonBorder(segment.transform, laneDistance * 1.8f, segmentLength);

            // Sinh chướng ngại vật và coin nếu không phải vùng an toàn ban đầu
            if (!isSafeZone && spawnZ >= safeDistanceBeforeObstacles)
            {
                PopulateSegment(segment.transform);
            }

            activeSegments.Add(segment);
            spawnZ += segmentLength;
        }

        private void CreateNeonBorder(Transform parent, float xPos, float length)
        {
            GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
            border.name = "NeonBorder";
            border.transform.SetParent(parent);
            border.transform.localPosition = new Vector3(xPos, 0.1f, length / 2f);
            border.transform.localScale = new Vector3(0.3f, 0.4f, length);
            
            var r = border.GetComponent<MeshRenderer>();
            if (r != null) r.material = obstacleMaterial;
            
            var c = border.GetComponent<Collider>();
            if (c != null) Destroy(c); // Chỉ để trang trí, không chặn
        }

        private void PopulateSegment(Transform segment)
        {
            int obstacleCount = Random.Range(1, 3);
            List<int> usedLanes = new List<int>();

            for (int i = 0; i < obstacleCount; i++)
            {
                int lane = Random.Range(-1, 2); // -1, 0, 1
                if (usedLanes.Contains(lane)) continue;
                usedLanes.Add(lane);

                // Đảm bảo không chặn kín cả 3 làn
                if (usedLanes.Count >= 3) break;

                float zOffset = Random.Range(5f, segmentLength - 5f);
                CreateObstacle(segment, lane * laneDistance, zOffset);
            }

            // Sinh chuỗi coin ở làn còn trống
            int coinLane = Random.Range(-1, 2);
            if (!usedLanes.Contains(coinLane))
            {
                for (int c = 0; c < 4; c++)
                {
                    float zPos = 8f + (c * 4f);
                    CreateCoin(segment, coinLane * laneDistance, zPos);
                }
            }
        }

        private void CreateObstacle(Transform parent, float x, float z)
        {
            GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obs.name = "SpaceObstacle";
            obs.tag = "Obstacle";
            obs.transform.SetParent(parent);
            obs.transform.localPosition = new Vector3(x, 0.75f, z);
            obs.transform.localScale = new Vector3(2.0f, 1.5f, 1.0f);

            var r = obs.GetComponent<MeshRenderer>();
            if (r != null) r.material = obstacleMaterial;

            var col = obs.GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = true;
        }

        private void CreateCoin(Transform parent, float x, float z)
        {
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "SpaceCoin";
            coin.tag = "Coin";
            coin.transform.SetParent(parent);
            coin.transform.localPosition = new Vector3(x, 0.8f, z);
            coin.transform.localScale = new Vector3(1.0f, 0.15f, 1.0f);
            coin.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var r = coin.GetComponent<MeshRenderer>();
            if (r != null) r.material = coinMaterial;

            var col = coin.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            // Thêm script xoay tròn tự động
            coin.AddComponent<CoinRotator>();
        }

        private void RecycleSegment()
        {
            GameObject oldSegment = activeSegments[0];
            activeSegments.RemoveAt(0);
            Destroy(oldSegment);

            SpawnSegment(false);
        }

        private void CreateProceduralMaterials()
        {
            Shader standardShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            // Mặt đường: Xanh đen viễn tưởng
            roadMaterial = new Material(standardShader);
            roadMaterial.color = new Color(0.08f, 0.09f, 0.14f);

            // Chướng ngại vật: Đỏ neon phát sáng
            obstacleMaterial = new Material(standardShader);
            obstacleMaterial.color = new Color(1f, 0.15f, 0.3f);
            if (obstacleMaterial.HasProperty("_EmissionColor"))
            {
                obstacleMaterial.EnableKeyword("_EMISSION");
                obstacleMaterial.SetColor("_EmissionColor", new Color(1f, 0.15f, 0.3f) * 1.5f);
            }

            // Tinh thể / Xu: Vàng Neon phát sáng
            coinMaterial = new Material(standardShader);
            coinMaterial.color = new Color(1f, 0.85f, 0.1f);
            if (coinMaterial.HasProperty("_EmissionColor"))
            {
                coinMaterial.EnableKeyword("_EMISSION");
                coinMaterial.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.1f) * 1.8f);
            }
        }
    }

    public class CoinRotator : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(Vector3.forward, 150f * Time.deltaTime, Space.Self);
        }
    }
}
