# Cosmic Runner 3D (Vũ Trụ Vô Tận)

Game 3D Endless Runner đề tài không gian viễn tưởng, tối ưu cho thiết bị di động Android.

---

## 🎮 Cách Chơi (Gameplay)
- **Vuốt Trái / Phải:** Chuyển đổi giữa 3 làn đường không gian (-3m, 0m, +3m) với hiệu ứng lướt mượt mà.
- **Vuốt Lên:** Nhảy qua chướng ngại vật (Space Laser, chướng ngại vật thấp).
- **Phím tắt (Khi test):** A/D hoặc Mũi tên Trái/Phải để đổi làn; W/Space/Mũi tên Lên để nhảy.
- **Cơ chế tốc độ:** Bắt đầu ở vận tốc 12 m/s, tăng dần theo thời gian tối đa 35 m/s.
- **Thu thập:** Nhặt tinh thể năng lượng / đồng xu vàng trên các làn an toàn.
- **Tính điểm & Lưu trữ:** Điểm quãng đường và tổng số xu được lưu tự động vào `PlayerPrefs`.

---

## 🏗️ Kiến Trúc Mã Nguồn (Clean Architecture)
```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs       # Quản lý vòng đời (Menu, Playing, GameOver), tính điểm, tăng tốc độ
│   └── GameInitializer.cs   # Tự động dựng toàn bộ Scene 3D, Camera, Ánh sáng, Player, UI khi mở Scene trống
├── Player/
│   ├── PlayerController.cs  # Nhận diện cảm ứng vuốt Android, di chuyển 3 làn, trọng lực nhảy
│   └── CameraFollow.cs      # Bám sát góc nhìn người thứ ba, chống rung giật
├── World/
│   └── TrackManager.cs      # Sinh đường vô tận, tái sử dụng (Recycle) đoạn đường để chống tràn RAM
├── Audio/
│   └── SoundManager.cs      # Tổng hợp âm thanh thủ tục (Procedural Synth) cho Jump, Coin, Crash, Click
└── UI/
    └── UIManager.cs         # Giao diện Canvas thích ứng tỉ lệ màn hình điện thoại (HUD, Menu, Game Over)
```

---

## 🚀 Cách Xuất File APK Không Cần Máy Tính (Cloud Build)
Vì máy tính/Editor không có sẵn trên điện thoại (Unity Editor chỉ hỗ trợ x86_64, không có bản native Linux ARM64), dự án được trang bị sẵn workflow **GitHub Actions** (`.github/workflows/build-android.yml`):

1. Khởi tạo Git và đẩy thư mục này lên GitHub (Private hoặc Public):
   ```bash
   cd ~/unity-runner-3d
   git init
   git add .
   git commit -m "Initial commit for Cosmic Runner 3D"
   git branch -M main
   git remote add origin https://github.com/<your-username>/<your-repo-name>.git
   git push -u origin main
   ```
2. Thêm Secret bản quyền Unity vào Settings của Repo (nếu dùng GameCI) hoặc chạy workflow.
3. GitHub Actions sẽ tự động biên dịch file APK target ARM64 trên máy chủ đám mưu x86_64 của GitHub.
4. Tải file `CosmicRunner3D-Android-APK.apk` trong thẻ **Actions > Artifacts** trực tiếp về điện thoại và cài đặt!
