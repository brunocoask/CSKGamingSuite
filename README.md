# 🚀 CSK Gaming Suite

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-0078D6?logo=windows)](https://microsoft.com)
[![Framework](https://img.shields.io/badge/.NET-8.0%20WPF-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Languages](https://img.shields.io/badge/Languages-EN%20%7C%20PT--BR%20%7C%20ES%20%7C%20RU%20%7C%20ZH-orange)](#-multilingual-support)

**CSK Gaming Suite** is a unified, open-source C# / WPF (.NET 8) desktop software designed for PC gamers, hardware enthusiasts, and overclockers. It combines real-time hardware telemetry, OS low-latency tuning, GPU ray tracing benchmarking, audio device routing, and 21:9 / 32:9 Ultrawide window management into a luxury dark/light interface.

---

## 🌍 Multilingual Support

CSK Gaming Suite features real-time, dynamic language switching without requiring an app restart:
- 🇺🇸 **English (US)**
- 🇧🇷 **Português (Brasil)**
- 🇪🇸 **Español**
- 🇷🇺 **Русский**
- 🇨🇳 **中文 (简体)**

The application automatically identifies your Windows display culture upon first launch and persists your preferences locally in `%AppData%/CSKGamingSuite`.

---

## ✨ Modules & Capabilities

### 📊 Real-Time Hardware Telemetry (`CSK HARDWARE`)
- **System Overview:** Deep hardware inspection (Motherboard, BIOS, CPU microcode, PCI-e bus link speeds).
- **Live Hardware Sensors:** Multi-sensor polling (vCore, package power, fan RPM, memory junction & hotspot temperatures).
- **S.M.A.R.T. Disk Health:** Full NVMe, SATA SSD, and HDD diagnostics with endurance calculation and health scoring.
- **Power & Energy Profiles:** Fast switching between Windows Power Plans and Ultimate Performance modes.
- **Network & Latency:** Real-time ping jitter, DNS benchmarks, and network adapter MTU inspection.

### 🎮 Gaming Optimization (`CSK GAMES`)
- **Game Library:** Local scanner detecting Steam, Epic Games, GOG, and custom standalone executables.
- **Estimated Game FPS:** Algorithmic FPS predictor tailored for 1080p, 1440p, and 4K with Upscaling presets (DLSS / FSR / XeSS).
- **Bottleneck Calculator:** Predictive CPU vs. GPU bottleneck evaluation for competitive games.
- **In-Game Overlay (HUD):** Lightweight DirectX overlay displaying live framerates, 1% low, and temperatures.

### ⚡ Performance & Benchmarking (`CSK PERFORMANCE`)
- **GPU 3D Ray Tracing Benchmark:** Native DirectX 12 hardware ray tracing engine measuring real-world compute and frametime consistency.
- **Stress Testing:** High-load thermal burn-in utility with automated safety thermal throttling cutoffs.
- **Safe Overclocking:** Direct GPU core and memory clock offset controls via NVML with instant profile restore.

### 🛠️ System Tools & Tweaks (`CSK TOOLS`)
- **Windows Gaming Tweaks:** Optimizes synthetic timers (HPET), disables telemetry, game bar DVR, and unnecessary background polling.
- **Startup & Services Manager:** Safely toggles startup bloatware without corrupting registry hives.
- **Display Driver Uninstaller (DDU) Helper:** Clean extraction and driver isolation tools.
- **RGB Lighting & Hardware Sync:** Direct SDK synchronization across Corsair iCUE, OpenRGB, and Razer Chroma.
- **34" Ultrawide Manager:** Intelligent window snapper designed for 21:9 monitors (50/50 split, 33/33/33 column, and 25/50/25 productivity layouts).

### 🔌 Maker & Serial Tools (`CSK MAKER`)
- **COM Port Cleaner:** Inspects, troubleshoots, and frees ghost/locked COM serial ports (CH340, CP210x, FTDI, Arduino).

---

## 🛠️ Architecture & Technologies

- **Language:** C# 12
- **Framework:** .NET 8.0 Windows (WPF)
- **DirectX Interop:** Vortice.Direct3D11 & Vortice.Direct3D12
- **Hardware Telemetry:** LibreHardwareMonitorLib & Native NVML
- **Architecture:** x64 / AnyCPU

---

## 🚀 Building from Source

### Prerequisites
1. Windows 10 (Build 19041+) or Windows 11
2. [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Clone & Compile
```bash
# Clone the repository
git clone https://github.com/brunocoask/CSKGamingSuite.git

# Enter the source directory
cd CSKGamingSuite/src

# Build in Release mode
dotnet build CSKGamingSuite.csproj -c Release -o ../CSK_App_Build
```

The compiled standalone executable and dependencies will be generated in `CSK_App_Build/CSK.exe`.

---

## 🤝 Contributing

Contributions from the open-source community are very welcome!
- Adding support for new hardware sensors
- Adding new language translations to `I18n/*.json`
- Optimizing benchmark shaders and algorithms
- Reporting issues or suggesting new features

Please feel free to submit a **Pull Request** or open an **Issue** on GitHub!

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.
