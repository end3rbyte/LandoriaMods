using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Landoria.SharedLib;

namespace Landoria.ServerGateway
{
    internal sealed class LocalGatewayServer : IDisposable
    {
        private const int MaximumRequestLineLength = 2048;
        private const int MaximumHeaderCount = 50;
        private readonly int _port;
        private readonly string _token;
        private readonly Func<bool> _queueSave;
        private readonly ModLog _log;
        private TcpListener _listener;
        private Thread _thread;
        private volatile bool _running;
        private string _response = StatusSnapshot.NotReadyJson;

        internal LocalGatewayServer(int port, string token, Func<bool> queueSave, ModLog log)
        {
            _port = port;
            _token = token ?? string.Empty;
            _queueSave = queueSave;
            _log = log;
        }

        internal void Start()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Loopback, _port);
                _listener.Start();
                _running = true;
                _thread = new Thread(Listen) { IsBackground = true, Name = "ServerGateway HTTP" };
                _thread.Start();
                _log.LogInfo($"Server gateway is listening on http://127.0.0.1:{_port}.");
            }
            catch (Exception exception)
            {
                _log.LogError($"Could not start the server gateway: {exception}");
                Dispose();
            }
        }

        internal void SetResponse(string response)
        {
            Interlocked.Exchange(ref _response, response);
        }

        private void Listen()
        {
            while (_running)
            {
                try
                {
                    using (TcpClient client = _listener.AcceptTcpClient())
                    {
                        Handle(client);
                    }
                }
                catch (SocketException) when (!_running)
                {
                    return;
                }
                catch (Exception exception)
                {
                    _log.LogWarning($"Server gateway request failed: {exception.Message}");
                }
            }
        }

        private void Handle(TcpClient client)
        {
            client.ReceiveTimeout = 2000;
            client.SendTimeout = 2000;
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.ASCII, false, 1024, true))
            {
                string requestLine = reader.ReadLine();
                if (requestLine == null || requestLine.Length > MaximumRequestLineLength)
                {
                    WriteResponse(stream, "400 Bad Request", "{\"error\":\"bad_request\"}");
                    return;
                }

                string authorization = null;
                for (int index = 0; index < MaximumHeaderCount; index++)
                {
                    string header = reader.ReadLine();
                    if (header == null || header.Length > MaximumRequestLineLength)
                    {
                        WriteResponse(stream, "400 Bad Request", "{\"error\":\"bad_request\"}");
                        return;
                    }
                    if (header.Length == 0)
                    {
                        HandleRequest(stream, requestLine, authorization);
                        return;
                    }
                    if (header.StartsWith("Authorization:", StringComparison.OrdinalIgnoreCase))
                    {
                        authorization = header.Substring("Authorization:".Length).Trim();
                    }
                }
                WriteResponse(stream, "431 Request Header Fields Too Large", "{\"error\":\"too_many_headers\"}");
            }
        }

        private void HandleRequest(Stream stream, string requestLine, string authorization)
        {
            string[] parts = requestLine.Split(' ');
            if (parts.Length != 3)
            {
                WriteResponse(stream, "400 Bad Request", "{\"error\":\"bad_request\"}");
                return;
            }

            string path = parts[1].Split('?')[0];
            if (!Authenticate(stream, authorization))
            {
                return;
            }
            if (parts[0] == "GET" && path == "/status")
            {
                WriteResponse(stream, "200 OK", Interlocked.CompareExchange(ref _response, null, null));
                return;
            }
            if (parts[0] == "POST" && path == "/commands/save")
            {
                HandleSave(stream);
                return;
            }
            WriteResponse(stream, "404 Not Found", "{\"error\":\"not_found\"}");
        }

        private bool Authenticate(Stream stream, string authorization)
        {
            if (_token.Length == 0)
            {
                WriteResponse(stream, "503 Service Unavailable", "{\"error\":\"gateway_not_configured\"}");
                return false;
            }
            const string prefix = "Bearer ";
            string suppliedToken = authorization != null && authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? authorization.Substring(prefix.Length) : string.Empty;
            if (!FixedTimeEquals(_token, suppliedToken))
            {
                WriteResponse(stream, "401 Unauthorized", "{\"error\":\"unauthorized\"}");
                return false;
            }
            return true;
        }

        private void HandleSave(Stream stream)
        {
            if (!_queueSave())
            {
                WriteResponse(stream, "409 Conflict", "{\"error\":\"save_already_queued\"}");
                return;
            }
            WriteResponse(stream, "202 Accepted", "{\"accepted\":true,\"command\":\"save\"}");
        }

        private static bool FixedTimeEquals(string expected, string supplied)
        {
            int difference = expected.Length ^ supplied.Length;
            int length = Math.Max(expected.Length, supplied.Length);
            for (int index = 0; index < length; index++)
            {
                char expectedCharacter = index < expected.Length ? expected[index] : (char)0;
                char suppliedCharacter = index < supplied.Length ? supplied[index] : (char)0;
                difference |= expectedCharacter ^ suppliedCharacter;
            }
            return difference == 0;
        }

        private static void WriteResponse(Stream stream, string status, string json)
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            string headers = $"HTTP/1.1 {status}\r\nContent-Type: application/json; charset=utf-8\r\n" +
                             $"Content-Length: {body.Length}\r\nCache-Control: no-store\r\nConnection: close\r\n\r\n";
            byte[] headerBytes = Encoding.ASCII.GetBytes(headers);
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(body, 0, body.Length);
        }

        public void Dispose()
        {
            _running = false;
            _listener?.Stop();
            _listener = null;
            if (_thread != null && _thread != Thread.CurrentThread)
            {
                _thread.Join(2000);
            }
            _thread = null;
        }
    }
}
