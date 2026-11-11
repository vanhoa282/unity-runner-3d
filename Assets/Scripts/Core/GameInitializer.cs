using CosmicRunner.Audio;
using CosmicRunner.Core;
using CosmicRunner.Player;
using CosmicRunner.UI;
using CosmicRunner.World;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CosmicRunner.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitializeGame()
        {
            // Kiểm tra xem đã có GameManager chưa, nếu chưa có thì dựng toàn bộ
            if (FindFirstObjectByType<GameManager>() != null) return;

            Debug.Log("[CosmicRunner] Tự động khởi tạo toàn bộ thế giới Game 3D...");

            // 1. Ánh sáng không gian (Directional Light)
            var existingLight = FindFirstObjectByType<Light>();
            if (existingLight == null)
            {
                GameObject lightObj = new GameObject("SpaceLight");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(0.85f, 0.9f, 1f);
                light.intensity = 1.2f;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // 2. Camera Chính (Main Camera)
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            mainCam.backgroundColor = new Color(0.02f, 0.02f, 0.06f); // Nền trời đêm vũ trụ
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.fieldOfView = 65f;

            // 3. Người chơi (Player Spaceship)
            GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "PlayerSpaceship";
            playerObj.tag = "Player";
            playerObj.transform.position = new Vector3(0, 0.8f, 0);
            playerObj.transform.rotation = Quaternion.Euler(90f, 0, 0); // Nằm ngang dạng phi thuyền lướt
            playerObj.transform.localScale = new Vector3(0.9f, 1.4f, 0.6f);

            // Gắn vật liệu phi thuyền màu xanh neon
            Shader standardShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material shipMat = new Material(standardShader);
            shipMat.color = new Color(0f, 0.9f, 1f);
            if (shipMat.HasProperty("_EmissionColor"))
            {
                shipMat.EnableKeyword("_EMISSION");
                shipMat.SetColor("_EmissionColor", new Color(0f, 0.9f, 1f) * 1.2f);
            }
            var playerRenderer = playerObj.GetComponent<MeshRenderer>();
            if (playerRenderer != null) playerRenderer.material = shipMat;

            // Bỏ capsule collider mặc định, dùng CharacterController
            var defaultCol = playerObj.GetComponent<Collider>();
            if (defaultCol != null) Destroy(defaultCol);

            CharacterController cc = playerObj.AddComponent<CharacterController>();
            cc.radius = 0.5f;
            cc.height = 1.8f;
            cc.center = new Vector3(0, 0, 0);

            playerObj.AddComponent<PlayerController>();

            // Gắn CameraFollow vào MainCamera
            CameraFollow camFollow = mainCam.gameObject.GetComponent<CameraFollow>();
            if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow>();

            // 4. Các Singletons quản lý Core
            GameObject coreManagers = new GameObject("CoreManagers");
            coreManagers.AddComponent<GameManager>();
            coreManagers.AddComponent<SoundManager>();
            coreManagers.AddComponent<TrackManager>();

            // 5. Hệ thống UI & EventSystem
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }

            GameObject uiObj = new GameObject("GameCanvas");
            uiObj.AddComponent<UIManager>();

            // 6. Hiệu ứng hạt bụi không gian (Cosmic Starfield)
            CreateCosmicDustEffect(playerObj.transform);
        }

        private static void CreateCosmicDustEffect(Transform player)
        {
            GameObject dustObj = new GameObject("CosmicStarfield");
            dustObj.transform.SetParent(player);
            dustObj.transform.localPosition = new Vector3(0, 2f, 15f);

            ParticleSystem ps = dustObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(1f, 1f, 1f, 0.8f);
            main.startSize = 0.15f;
            main.startSpeed = -25f; // Bay ngược về phía sau tạo cảm giác tốc độ
            main.startLifetime = 1.5f;
            main.maxParticles = 150;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 80f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(14f, 8f, 1f);
        }
    }
}
