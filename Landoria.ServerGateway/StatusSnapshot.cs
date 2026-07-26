using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Bootstrap;

namespace Landoria.ServerGateway
{
    internal static class StatusSnapshot
    {
        internal const string NotReadyJson = "{\"ready\":false}";
        private const int VanillaMaxPlayers = 10;
        private const int ExpandedServerDefaultMaxPlayers = 20;

        internal static string CreateJson()
        {
            ZNet network = ZNet.instance;
            if (network == null || !network.IsServer() || ZNet.World == null)
            {
                return NotReadyJson;
            }

            List<ZNet.PlayerInfo> playerList = network.GetPlayerList();
            StringBuilder json = new StringBuilder(256);
            json.Append("{\"ready\":true,\"serverName\":").Append(JsonString(ReadArgument("-name")));
            json.Append(",\"worldName\":").Append(JsonString(network.GetWorldName()));
            json.Append(",\"playerCount\":").Append(playerList.Count);
            json.Append(",\"maxPlayers\":").Append(GetMaximumPlayers());
            json.Append(",\"day\":").Append(GetDay(network));
            json.Append(",\"worldCreatedAt\":").Append(JsonString(Environment.GetEnvironmentVariable("WORLD_CREATED_AT")));
            json.Append(",\"players\":[");
            AppendPlayers(json, playerList);
            json.Append("],\"playerDetails\":[");
            AppendPlayerDetails(json, playerList);
            return json.Append("]}").ToString();
        }

        private static void AppendPlayers(StringBuilder json, List<ZNet.PlayerInfo> players)
        {
            for (int index = 0; index < players.Count; index++)
            {
                if (index > 0)
                {
                    json.Append(',');
                }
                json.Append(JsonString(players[index].m_name));
            }
        }

        private static void AppendPlayerDetails(StringBuilder json, List<ZNet.PlayerInfo> players)
        {
            for (int index = 0; index < players.Count; index++)
            {
                if (index > 0)
                {
                    json.Append(',');
                }
                string platformUserId = players[index].m_userInfo.m_id.ToString();
                json.Append("{\"name\":").Append(JsonString(players[index].m_name));
                json.Append(",\"platformUserId\":").Append(JsonString(platformUserId));
                json.Append(",\"steamId\":").Append(JsonString(GetSteamId(platformUserId)));
                json.Append(",\"isAdmin\":").Append(ZNet.instance.PlayerIsAdmin(players[index].m_userInfo.m_id)
                    ? "true" : "false").Append('}');
            }
        }

        private static string GetSteamId(string platformUserId)
        {
            const string prefix = "Steam_";
            return platformUserId != null && platformUserId.StartsWith(prefix, StringComparison.Ordinal)
                ? platformUserId.Substring(prefix.Length) : null;
        }

        private static int GetDay(ZNet network)
        {
            return EnvMan.instance == null ? 0 : EnvMan.instance.GetDay(network.GetTimeSeconds());
        }

        private static int GetMaximumPlayers()
        {
            string configured = ReadArgument("--maxplayer");
            if (int.TryParse(configured, out int value) && value > 0)
            {
                return Math.Min(value, 100);
            }
            return Chainloader.PluginInfos.ContainsKey("Landoria.ExpandedServer")
                ? ExpandedServerDefaultMaxPlayers : VanillaMaxPlayers;
        }

        private static string ReadArgument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index + 1 < arguments.Length; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[index + 1];
                }
            }
            return null;
        }

        private static string JsonString(string value)
        {
            if (value == null)
            {
                return "null";
            }
            StringBuilder escaped = new StringBuilder(value.Length + 2).Append('"');
            foreach (char character in value)
            {
                AppendEscaped(escaped, character);
            }
            return escaped.Append('"').ToString();
        }

        private static void AppendEscaped(StringBuilder escaped, char character)
        {
            switch (character)
            {
                case '"': escaped.Append("\\\""); break;
                case '\\': escaped.Append("\\\\"); break;
                case '\b': escaped.Append("\\b"); break;
                case '\f': escaped.Append("\\f"); break;
                case '\n': escaped.Append("\\n"); break;
                case '\r': escaped.Append("\\r"); break;
                case '\t': escaped.Append("\\t"); break;
                default:
                    escaped.Append(character < 32 ? $"\\u{(int)character:x4}" : character.ToString());
                    break;
            }
        }
    }
}
