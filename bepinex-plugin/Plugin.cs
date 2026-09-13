using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace BepInExDiscordConnect;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "valheim.discordconnect";
    private const string PluginName = "Valheim Discord Connect";
    private const string PluginVersion = "0.1.0";

    private ConfigEntry<string> _webhookUrl = null!;
    private readonly object _playersLock = new();
    private List<string> _cachedPlayers = new();
    private HttpListener? _statusListener;

    private static readonly HttpClient HttpClient = new();

    private void Awake()
    {
        _webhookUrl = Config.Bind("Discord", "WebhookUrl", "", "Discord webhook URL. Prefer setting VALHEIM_DISCORD_WEBHOOK_URL instead.");

        var webhookUrl = ReadWebhookUrlFromDotEnv();
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            webhookUrl = Environment.GetEnvironmentVariable("VALHEIM_DISCORD_WEBHOOK_URL");
        }
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            webhookUrl = _webhookUrl.Value;
        }

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            Logger.LogWarning("Valheim Discord Connect booted but Discord is not configured yet. No webhook URL is set.");
        }
        else
        {
            Logger.LogInfo("Valheim Discord Connect connected to Discord and ready to post updates.");
            _ = PublishMessageToDiscordAsync("Valheim server started. Discord Connect plugin is working.");
        }

        StartCoroutine(WatchForPlayerJoins());
        StartStatusListener();
    }

    private System.Collections.IEnumerator WatchForPlayerJoins()
    {
        var knownPlayers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var initialized = false;

        while (true)
        {
            yield return new WaitForSeconds(1f);

            var players = GetOnlinePlayerNames();
            lock (_playersLock)
            {
                _cachedPlayers = players;
            }

            if (!initialized)
            {
                knownPlayers.UnionWith(players);
                initialized = true;
                continue;
            }

            var joinedPlayers = players.Where(player => !knownPlayers.Contains(player)).ToList();
            var leftPlayers = knownPlayers.Where(player => !players.Contains(player, StringComparer.OrdinalIgnoreCase)).ToList();
            knownPlayers.Clear();
            knownPlayers.UnionWith(players);

            foreach (var player in joinedPlayers)
            {
                var message = DiscordMessageFormatter.FormatPlayerJoined(player, players);
                Logger.LogInfo(message);
                _ = PublishMessageToDiscordAsync(message);
            }

            foreach (var player in leftPlayers)
            {
                var message = DiscordMessageFormatter.FormatPlayerLeft(player, players);
                Logger.LogInfo(message);
                _ = PublishMessageToDiscordAsync(message);
            }
        }
    }

    private void StartStatusListener()
    {
        try
        {
            _statusListener = new HttpListener();
            _statusListener.Prefixes.Add("http://127.0.0.1:8765/");
            _statusListener.Start();
            _ = Task.Run(HandleStatusRequestsAsync);
            Logger.LogInfo("Valheim status bridge listening on localhost:8765.");
        }
        catch (Exception exception)
        {
            Logger.LogError($"Could not start the Valheim status bridge: {exception.Message}");
        }
    }

    private async Task HandleStatusRequestsAsync()
    {
        while (_statusListener != null && _statusListener.IsListening)
        {
            try
            {
                var context = await _statusListener.GetContextAsync();
                if (context.Request.HttpMethod != "GET" || context.Request.Url?.AbsolutePath != "/players")
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close();
                    continue;
                }

                List<string> players;
                lock (_playersLock)
                {
                    players = new List<string>(_cachedPlayers);
                }

                var responseBody = $"{{\"players\":[{string.Join(",", players.Select(player => $"\"{EscapeJson(player)}\""))}]}}";
                var responseBytes = Encoding.UTF8.GetBytes(responseBody);
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = responseBytes.Length;
                await context.Response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                context.Response.Close();
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception exception)
            {
                Logger.LogWarning($"Valheim status bridge request failed: {exception.Message}");
            }
        }
    }

    private void OnDestroy()
    {
        if (_statusListener == null)
        {
            return;
        }

        _statusListener.Stop();
        _statusListener.Close();
        _statusListener = null;
    }

    private static List<string> GetOnlinePlayerNames()
    {
        if (ZNet.instance == null)
        {
            return new List<string>();
        }

        return ZNet.instance.GetPlayerList()
            .Select(player => player.m_name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task PublishMessageToDiscordAsync(string content)
    {
        var webhookUrl = GetWebhookUrl();

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            Logger.LogWarning("Discord is disabled until WebhookUrl is configured.");
            return;
        }

        try
        {
            using var request = new StringContent($"{{\"content\":\"{EscapeJson(content)}\"}}", System.Text.Encoding.UTF8, "application/json");
            using var response = await HttpClient.PostAsync(webhookUrl, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning($"Discord webhook returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }
        }
        catch (Exception exception)
        {
            Logger.LogError($"Could not publish the Valheim player list to Discord: {exception.Message}");
        }
    }

    private string GetWebhookUrl()
    {
        var webhookUrl = ReadWebhookUrlFromDotEnv();
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            webhookUrl = Environment.GetEnvironmentVariable("VALHEIM_DISCORD_WEBHOOK_URL");
        }
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            webhookUrl = _webhookUrl.Value;
        }

        return webhookUrl;
    }

    private static string EscapeJson(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
    }

    private static string ReadWebhookUrlFromDotEnv()
    {
        try
        {
            var envFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            if (!File.Exists(envFile))
            {
                return string.Empty;
            }

            foreach (var line in File.ReadAllLines(envFile))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = trimmed.Substring(0, separatorIndex).Trim();
                var value = trimmed.Substring(separatorIndex + 1).Trim();
                if (string.Equals(key, "VALHEIM_DISCORD_WEBHOOK_URL", StringComparison.OrdinalIgnoreCase))
                {
                    return value.Trim('"', '\'');
                }
            }
        }
        catch (Exception)
        {
            // Ignore malformed local env files; the normal config/env mechanisms remain supported.
        }

        return string.Empty;
    }
}