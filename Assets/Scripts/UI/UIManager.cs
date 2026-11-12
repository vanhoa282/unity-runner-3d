using CosmicRunner.Audio;
using CosmicRunner.Core;
using CosmicRunner.World;
using UnityEngine;
using UnityEngine.UI;

namespace CosmicRunner.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private GameObject mainMenuPanel;
        private GameObject inGameHudPanel;
        private GameObject gameOverPanel;

        // HUD Elements
        private Text scoreText;
        private Text coinsText;
        private Text speedText;

        // Menu Elements
        private Text bestScoreMenuText;

        // Game Over Elements
        private Text finalScoreText;
        private Text finalCoinsText;
        private Text newRecordText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildUIHierarchy();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStarted += ShowInGameHUD;
                GameManager.Instance.OnGameOver += ShowGameOverScreen;
                GameManager.Instance.OnScoreUpdated += UpdateScoreText;
                GameManager.Instance.OnCoinsUpdated += UpdateCoinsText;
            }

            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStarted -= ShowInGameHUD;
                GameManager.Instance.OnGameOver -= ShowGameOverScreen;
                GameManager.Instance.OnScoreUpdated -= UpdateScoreText;
                GameManager.Instance.OnCoinsUpdated -= UpdateCoinsText;
            }
        }

        private void Update()
        {
            // Fallback chạm trực tiếp: nếu nút bấm UI không nhận chạm (EventSystem/raycast lỗi),
            // chạm bất cứ đâu ở menu cũng bắt đầu game (dùng Input touch trực tiếp, không qua uGUI).
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Menu)
            {
                bool tapped = false;
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) tapped = true;
                if (Input.GetMouseButtonDown(0)) tapped = true;
                if (tapped) TryStartGame();
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                if (speedText != null)
                {
                    speedText.text = $"SPEED: {GameManager.Instance.CurrentSpeed:F1} m/s";
                }
            }
        }

        // Bắt đầu game từ menu, chỉ chạy khi đang ở trạng thái Menu (chặn bấm đúp).
        private void TryStartGame()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Menu) return;
            try { SoundManager.Instance?.PlayButtonClick(); } catch (System.Exception e) { Debug.LogError("[CosmicRunner] PlayButtonClick loi: " + e.Message); }
            TrackManager.Instance?.ResetTrack();
            GameManager.Instance?.StartGame();
        }

        public void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
            inGameHudPanel.SetActive(false);
            gameOverPanel.SetActive(false);

            if (bestScoreMenuText != null && GameManager.Instance != null)
            {
                bestScoreMenuText.text = $"BEST DISTANCE: {GameManager.Instance.HighScore}m";
            }
        }

        public void ShowInGameHUD()
        {
            mainMenuPanel.SetActive(false);
            inGameHudPanel.SetActive(true);
            gameOverPanel.SetActive(false);
        }

        public void ShowGameOverScreen()
        {
            mainMenuPanel.SetActive(false);
            inGameHudPanel.SetActive(false);
            gameOverPanel.SetActive(true);

            if (GameManager.Instance != null)
            {
                int currentScore = (int)GameManager.Instance.DistanceScore;
                finalScoreText.text = $"DISTANCE: {currentScore}m";
                finalCoinsText.text = $"COINS: {GameManager.Instance.CoinsCollected}";

                bool isNewRecord = currentScore >= GameManager.Instance.HighScore && currentScore > 0;
                newRecordText.gameObject.SetActive(isNewRecord);
            }
        }

        private void UpdateScoreText(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{score}m";
            }
        }

        private void UpdateCoinsText(int coins)
        {
            if (coinsText != null)
            {
                coinsText.text = $"★ {coins}";
            }
        }

        // Tự động dựng toàn bộ UI Canvas chuẩn Mobile qua code
        private void BuildUIHierarchy()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // --- 1. MAIN MENU PANEL ---
            mainMenuPanel = CreatePanel("MainMenuPanel", transform, new Color(0, 0, 0, 0.75f));

            CreateText("TitleText", mainMenuPanel.transform, "COSMIC RUNNER", font, 68, FontStyle.Bold, Color.cyan, new Vector2(0, 420), new Vector2(900, 120));
            CreateText("SubTitleText", mainMenuPanel.transform, "Vũ Trụ Vô Tận 3D", font, 36, FontStyle.Normal, Color.white, new Vector2(0, 340), new Vector2(800, 80));

            bestScoreMenuText = CreateText("BestScoreText", mainMenuPanel.transform, "BEST DISTANCE: 0m", font, 40, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), new Vector2(0, 120), new Vector2(800, 80));

            CreateText("InstructionText", mainMenuPanel.transform, "Vuốt Trái / Phải để đổi làn\nVuốt Lên để Nhảy", font, 34, FontStyle.Italic, new Color(0.8f, 0.8f, 0.9f), new Vector2(0, -60), new Vector2(800, 120));

            CreateButton("PlayButton", mainMenuPanel.transform, "BẮT ĐẦU CHƠI", font, 44, new Vector2(0, -260), new Vector2(480, 110), new Color(0.1f, 0.7f, 1f), () =>
            {
                TryStartGame();
            });

            // --- 2. IN-GAME HUD PANEL ---
            inGameHudPanel = CreatePanel("InGameHUDPanel", transform, Color.clear);

            scoreText = CreateText("ScoreText", inGameHudPanel.transform, "0m", font, 60, FontStyle.Bold, Color.white, new Vector2(0, 820), new Vector2(500, 100));
            coinsText = CreateText("CoinsText", inGameHudPanel.transform, "★ 0", font, 46, FontStyle.Bold, new Color(1f, 0.85f, 0.1f), new Vector2(-360, 820), new Vector2(300, 80));
            coinsText.alignment = TextAnchor.MiddleLeft;

            speedText = CreateText("SpeedText", inGameHudPanel.transform, "SPEED: 12.0 m/s", font, 32, FontStyle.Normal, new Color(0.7f, 0.9f, 1f), new Vector2(0, 750), new Vector2(500, 60));

            // --- 3. GAME OVER PANEL ---
            gameOverPanel = CreatePanel("GameOverPanel", transform, new Color(0.05f, 0.05f, 0.12f, 0.9f));

            CreateText("GameOverTitle", gameOverPanel.transform, "CHIẾN HẠM BỊ PHÁ HỦY", font, 54, FontStyle.Bold, new Color(1f, 0.25f, 0.35f), new Vector2(0, 400), new Vector2(900, 100));

            newRecordText = CreateText("NewRecordBanner", gameOverPanel.transform, "★ KỶ LỤC MỚI! ★", font, 44, FontStyle.Bold, new Color(1f, 0.9f, 0.1f), new Vector2(0, 290), new Vector2(800, 80));

            finalScoreText = CreateText("FinalScoreText", gameOverPanel.transform, "DISTANCE: 0m", font, 48, FontStyle.Bold, Color.white, new Vector2(0, 160), new Vector2(800, 80));
            finalCoinsText = CreateText("FinalCoinsText", gameOverPanel.transform, "COINS: 0", font, 40, FontStyle.Normal, new Color(1f, 0.85f, 0.2f), new Vector2(0, 80), new Vector2(800, 80));

            CreateButton("RestartButton", gameOverPanel.transform, "CHƠI LẠI", font, 42, new Vector2(0, -120), new Vector2(440, 100), new Color(0.2f, 0.8f, 0.4f), () =>
            {
                SoundManager.Instance?.PlayButtonClick();
                TrackManager.Instance?.ResetTrack();
                GameManager.Instance?.RestartGame();
            });

            CreateButton("MenuButton", gameOverPanel.transform, "VỀ MENU CHÍNH", font, 36, new Vector2(0, -250), new Vector2(440, 90), new Color(0.4f, 0.4f, 0.5f), () =>
            {
                SoundManager.Instance?.PlayButtonClick();
                TrackManager.Instance?.ResetTrack();
                ShowMainMenu();
            });
        }

        private GameObject CreatePanel(string name, Transform parent, Color bgColor)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = panel.GetComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = false; // Panel nền không chặn chạm, nút bấm bên trong tự nhận
            return panel;
        }

        private Text CreateText(string name, Transform parent, string content, Font font, int fontSize, FontStyle style, Color color, Vector2 anchoredPos, Vector2 size)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(parent, false);

            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Text txt = textObj.GetComponent<Text>();
            txt.text = content;
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false; // Chữ không chặn chạm
            return txt;
        }

        private void CreateButton(string name, Transform parent, string label, Font font, int fontSize, Vector2 anchoredPos, Vector2 size, Color btnColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            img.color = btnColor;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(onClick);

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            Text txt = textObj.GetComponent<Text>();
            txt.text = label;
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false; // Chữ trên nút không chặn chạm, nút tự nhận
        }
    }
}
