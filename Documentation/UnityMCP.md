# Unity MCP setup

Installed 14 September 2026 from https://github.com/IvanMurzak/Unity-MCP.

- Unity package: `com.ivanmurzak.unity.mcp` **0.90.0**, via OpenUPM.
- Official CLI: `unity-mcp-cli` **0.90.0** (global npm installation).
- Local server: **9.2.5**, downloaded with checksum verification.
- Editor: Unity **6000.4.9f1**.
- Codex configuration: `.codex/config.toml`, server name `ai-game-developer`.
- Endpoint: `http://localhost:28252` (Streamable HTTP; loopback only).
- Editor connection: `UserSettings/AI-Game-Developer-Config.json`, Custom mode, `keepConnected` and `keepServerRunning` enabled. This machine-local file is ignored by Git.

Unity manages the local server in `Library/mcp-server/win-x64`. Keep this project open in Unity while using its tools. Restart Codex if the newly configured MCP tools are not listed in an existing session. The older global `unityMCP` configuration was preserved; it is a different integration.

## Verification performed

The MCP protocol initialization succeeded and reported `gamedev-mcp-server` version `9.2.5.0`. A real JSON-RPC `tools/call` to `scene-get-data` returned `isError: false`, the loaded, saved `Assets/Scenes/Main.unity` scene, and the `Main Camera` and `Game` root objects. The raw result is in `Logs/MCP-scene-verification.txt`.

The official CLI also successfully ran `editor-application-get-state`, `scene-get-data`, and `console-get-logs`. Unity was not compiling, updating, or playing. The plugin logged its advisory about spaces in the project path and initial connection retries while the server started; subsequent scene and editor queries succeeded. No project move was needed for these checks.

## Recheck the connection

```powershell
unity-mcp-cli status 'C:/Unity/Projects/Incremental game' --timeout 5000
unity-mcp-cli run-tool editor-application-get-state 'C:/Unity/Projects/Incremental game' --raw
unity-mcp-cli run-tool scene-get-data 'C:/Unity/Projects/Incremental game' --raw
```

## Restore on another checkout

Install `unity-mcp-cli@0.90.0` with npm, then run `install-plugin <project-path> --plugin-version 0.90.0 --with-server`. Open Unity to finish dependency import. In Window > AI Game Developer, select a Custom local connection, enable connection/server persistence, and configure Codex with the actual local endpoint. This project's port is 28252; other checkout paths may receive different ports.

This is a local editor integration. No cloud MCP account or project credential is configured.
