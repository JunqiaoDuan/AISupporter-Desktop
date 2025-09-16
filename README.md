# AISupporter-Desktop

**AISupporter-Desktop** is an AI-powered Windows desktop application that helps users troubleshoot and optimize their computers in real time. The app combines advanced screen-sharing with OpenAI Vision to detect issues, provide actionable guidance, and help keep Windows running smoothly.

## ✨ Features

- **AI Screen-Sharing Troubleshooter**
  - Share your screen with an AI assistant that visually understands what's happening.
  - Automatic detection of common issues (error messages, disconnected Wi-Fi, app crashes, etc.).
  - Step-by-step, clickable instructions to resolve problems.
  - Real-time highlighting and guidance, similar to a human tech support agent.
  - Optional: Ask questions via chat interface.

- **Windows Optimizer**
  - Clean temporary files, cache, and junk.
  - Manage startup apps for faster boot times.
  - Kill heavy or unresponsive processes.
  - One-click “Boost Performance” feature.
  - Safe basic registry and network optimizations.

## 🛠️ Tech Stack

- **Frontend:** WPF (Windows Presentation Foundation) with .NET 8
- **Language:** C#
- **AI Integration:** OpenAI GPT-4 Vision API (for image understanding & troubleshooting)
- **Screen Capture:** Windows Desktop Duplication API
- **System Operations:** System.Diagnostics, System.IO, Microsoft.Win32

## 🚀 Getting Started

### Prerequisites

- Windows 10 or above
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- An [OpenAI API key](https://platform.openai.com/)

### Build & Run

```bash
git clone https://github.com/JunqiaoDuan/AISupporter-Desktop.git
cd AISupporter-Desktop
dotnet build
dotnet run
```

### Configuration

1. Create a file named `appsettings.json` in the root directory:
    ```json
    {
      "OpenAI": {
        "ApiKey": "YOUR_OPENAI_API_KEY"
      }
    }
    ```

2. (Optional) Adjust any optimizer or screen sharing settings in the app's Settings panel.

## 📦 Packaging

- The app can be published as a single EXE using .NET's [single-file publish](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/).
- For installer creation, use [MSIX Packaging Tool](https://learn.microsoft.com/en-us/windows/msix/packaging-tool/create-app-package) or [Inno Setup](https://jrsoftware.org/isinfo.php).

## 🧑‍💻 Contributing

Contributions are welcome! Please open issues or pull requests for suggestions or improvements.

## 📄 License

MIT License

---

*AISupporter-Desktop is not affiliated with Microsoft or OpenAI. All trademarks belong to their respective owners.*
