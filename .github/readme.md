# 🍴 Customization for [LLMCopilot]

Welcome! This is a customized fork of [foryoung365/vs-extension-llmcopilot](https://github.com/foryoung365/vs-extension-llmcopilot)).

### 💡 Why this fork exists
I created this fork to implement specific changes, including:
- [x] Adding support for OpenAI-compatible API endpoints
- [x] Adding Cancel button to the LLM Chat window to abort long text generations

### 🚀 Usage
You can use the pre-built private release 1.7.0 for Visual Studio 2019 or you can pull these changes into your project and build it all yourself.

### Screenshots
![Screen1](https://github.com/esam171/vs-extension-llmcopilot/blob/main/LLmCopilot-Screen1.jpg)

### 📜 Original Documentation
# LLMCopilot

[中文](https://github.com/foryoung365/vs-extension-llmcopilot)

**LLMCopilot** is now **fully open source**! Previously, it was not open source mainly because this extension was a toy project of vibecoding, and I planned to optimize it later. In fact, I don't have much energy to maintain this project, so now it's open source under the `MIT LICENSE`. You are free to modify it. PRs are welcome, but I may not be able to process them in time.

This is a Visual Studio extension based on [Ollama](https://github.com/ollama/ollama). Thanks to Ollama's localized deployment and powerful LLM (Large Language Model) support, you can get code suggestions and tips similar to GitHub Copilot, but all data is local and can even run offline.

**If this extension is helpful to you, please give us a 5-star rating on the marketplace.**

## Version History

[Version History](https://github.com/foryoung365/vs-extension-llmcopilot/blob/main/CHANGELOG_EN.md)

- Please note that the shortcut key to accept all predictions has been changed to `Alt+Q` to reduce misinterpretation of combinations like `Ctrl+C` during normal input. For more details, see the `Settings` section.

## Features
- Based on `Ollama`'s local large model, all data is stored locally and can be used even offline.
- Assists programming with large model capabilities:
  - Explains code
  - Auto-completion
  - Code error checking
  - Adds comments
  - Chats with the local large model
  - Generate Unit Test

## Settings
- Supports customization to use different large models, allowing you to experiment:
  - Custom chat large model
  - Custom auto-completion large model

- Supports customization of the "Fill in the middle" token, compatible with more large models.
- Auto-completion switch, default is `off`
    - Press `Alt+Q` to accept all predictions
    - Press `numbers 1-9` to accept the top N lines of predictions
    - Press `ESC` to reject predictions

- Note! The default large model is `DeepSeek-Coder:6.7b`. If your VRAM is less than 4G, it is recommended to use the `DeepSeek-Coder` model. For VRAM of 8G and above, it is recommended to use `DeepSeek-Coder:6.7b`.
- If VRAM is less than 16G, it is not recommended to use different large models for `chatting` and `code completion` simultaneously, as this will cause the `Ollama` server to frequently load different models, leading to slow response and affecting the experience.
- Supports setting the model response language to Chinese or English (depending on model support).

## Screenshots
![Settings](https://raw.githubusercontent.com/foryoung365/vs-extension-llmcopilot/main/Images/image.png)
![Chatting](https://raw.githubusercontent.com/foryoung365/vs-extension-llmcopilot/main/Images/image-1.png)
![Auto-completion](https://raw.githubusercontent.com/foryoung365/vs-extension-llmcopilot/main/Images/image-2.png)

## Acknowledgements
- Thanks to [Ollama](https://github.com/ollama/ollama) for providing an excellent large model server backend and easy-to-use API interface.
- Thanks to [OllamaSharp](https://github.com/awaescher/OllamaSharp) for the client to access the Ollama API.
- Thanks to [MdXaml](https://github.com/whistyun/MdXaml) for markdown rendering in the chat window.
- Thanks to [privy](https://github.com/srikanth235/privy) for the prompt templates.

Many thanks to them for providing excellent tools.

## FAQ 
[FAQ](./FAQ_EN.md)
