using Blazored.LocalStorage;
using GiamSat.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace GiamSat.UI.Services
{
    /// <summary>
    /// Bọc HubConnection cho LogsHub. Đăng ký Scoped (Blazor WASM ~singleton).
    /// </summary>
    public class LogStreamHubService : IAsyncDisposable
    {
        private readonly ILocalStorageService _localStorage;
        private readonly string _hubUrl;
        private HubConnection? _connection;

        public event Action<LogStreamEntry>? OnLogReceived;
        public event Action<HubConnectionState>? OnStateChanged;

        public HubConnectionState State =>
            _connection != null ? _connection.State : HubConnectionState.Disconnected;

        public bool IsConnected => State == HubConnectionState.Connected;

        public LogStreamHubService(IConfiguration config, ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            var baseUrl = (config["AppSettings:ApiBaseUrl"] ?? string.Empty).TrimEnd('/');
            _hubUrl = baseUrl + "/hubs/logs";
        }

        public async Task StartAsync()
        {
            if (_connection != null && _connection.State != HubConnectionState.Disconnected)
                return;

            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.AccessTokenProvider = async () =>
                        await _localStorage.GetItemAsync<string>(StorageConts.AuthToken);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<LogStreamEntry>("LogEntry", entry =>
            {
                var handler = OnLogReceived;
                if (handler != null) handler.Invoke(entry);
            });

            _connection.Reconnecting += error =>
            {
                NotifyState();
                return Task.CompletedTask;
            };
            _connection.Reconnected += connId =>
            {
                NotifyState();
                return Task.CompletedTask;
            };
            _connection.Closed += error =>
            {
                NotifyState();
                return Task.CompletedTask;
            };

            await _connection.StartAsync();
            NotifyState();
        }

        public async Task StopAsync()
        {
            if (_connection == null) return;
            try { await _connection.StopAsync(); } catch { /* ignore */ }
            NotifyState();
        }

        private void NotifyState()
        {
            var handler = OnStateChanged;
            if (handler != null) handler.Invoke(State);
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                try { await _connection.DisposeAsync(); } catch { /* ignore */ }
                _connection = null;
            }
        }
    }
}
