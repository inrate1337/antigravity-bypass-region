<div align="center">
  
  <img src="patcher%20antigravity/Assets/preview.png" alt="Google Antigravity Patcher Preview" width="600"/>

  <h1>🚀 Google Antigravity Patcher</h1>
  
  <p>
    <strong>automated patcher for Antigravity IDE and CLI.</strong>
  </p>

  <p>
    <a href="https://github.com/ActorArray"><img src="https://img.shields.io/badge/Author-inrate1337%20(@ActorArray)-blue?style=for-the-badge&logo=github" alt="Author" /></a>
    <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 8.0" /></a>
    <a href="https://docs.microsoft.com/en-us/dotnet/desktop/wpf/"><img src="https://img.shields.io/badge/WPF-UI-blueviolet?style=for-the-badge&logo=windows" alt="WPF" /></a>
  </p>
</div>

<br />

## ✨ Overview

**Google Antigravity Patcher** is an elegant WPF-based utility designed to seamlessly modify your Antigravity IDE installation. It features a beautiful, hardware-accelerated animated user interface and operates completely autonomously to locate, unpack, patch, and repack your IDE components.

### ⚠️ Disclaimer
> **Important:** This tool is designed **only** to provide access and remove regional restrictions. You must purchase subscriptions to the services yourself. We do not promote piracy or unauthorized access to paid services.

---

## ⚡ Features

- **🪄 Stunning UI:** A fluid, distraction-free interface with smooth animations, Gaussian blur effects, and video backgrounds.
- **🔍 Smart Discovery:** Automatically locates the Antigravity IDE installation path and `agy.exe` CLI binary via system variables and common directories.
- **📦 ASAR Integration:** Safely unpacks `app.asar`, injects the necessary patches into `main.js`, and repacks the archive without manual intervention.
- **🛠️ CLI Patching:** Automatically applies the "gate patch" to the `agy.exe` binary.
- **🛡️ Idempotent:** Smart checks prevent double-patching if the IDE is already modified.

---

## 🚀 How It Works

1. **Initialization:** The patcher initializes its core services and stunning animated UI.
2. **Discovery:** It scans your system's `AppData`, `Program Files`, and `Scoop` directories to find the IDE and CLI.
3. **Patching `main.js`:** 
   - Finds the mount point.
   - Extracts the `app.asar` archive to a temporary directory.
   - Injects the bypass code.
   - Repacks `app.asar` and cleans up temporary files.
4. **Patching CLI:** Applies the necessary modifications to `agy.exe`.
5. **Completion:** Clears cache and gracefully closes.

---

## 💻 Usage

Simply download the latest release, run the executable, and let the patcher do its magic. Sit back, watch the sleek animations, and wait for the "success" state!

---

## 👨‍💻 Author

Crafted with ❤️ by **inrate1337** (@ActorArray). 

> *“Design is not just what it looks like and feels like. Design is how it works.”*
