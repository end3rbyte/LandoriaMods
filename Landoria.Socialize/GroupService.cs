using System;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.Socialize
{
    internal static class GroupService
    {
        internal const string RequestRpc = "Landoria_Social_GroupRequest";
        internal const string ResponseRpc = "Landoria_Social_GroupResponse";
        private static ZRoutedRpc registeredRpc;
        private static float nextStateRequest;

        internal static void Update()
        {
            EnsureRpcs();
            if (ZNet.instance == null || ZRoutedRpc.instance == null)
            {
                return;
            }
            if (ZNet.instance.IsServer())
            {
                GroupStorage.TryLoad();
            }
            if (Player.m_localPlayer != null && Time.unscaledTime >= nextStateRequest)
            {
                nextStateRequest = Time.unscaledTime + 1f;
                SendRequest("state", "");
            }
        }

        internal static void Reset()
        {
            registeredRpc = null;
            nextStateRequest = 0f;
            GroupStorage.Reset();
            GroupState.ClearAll();
            DecayProtection.Reset();
            PiecePermissions.Reset();
        }

        internal static bool IsLocalPlayerInGroup()
        {
            return Game.instance != null &&
                   GroupState.LocalMembers.Contains(Game.instance.GetPlayerProfile().GetPlayerID());
        }

        internal static void SendChat(string message)
        {
            if (!IsLocalPlayerInGroup())
            {
                Chat.instance?.AddString("You are not in a group.");
                return;
            }
            SendRequest("chat", message);
        }

        internal static void SendRequest(string action, string argument)
        {
            EnsureRpcs();
            if (ZRoutedRpc.instance == null || Game.instance == null || Player.m_localPlayer == null)
            {
                return;
            }
            ZPackage package = new ZPackage();
            package.Write(action);
            package.Write(Game.instance.GetPlayerProfile().GetPlayerID());
            package.Write(Game.instance.GetPlayerProfile().GetName());
            package.Write(argument ?? "");
            ZRoutedRpc.instance.InvokeRoutedRPC(RequestRpc, package);
        }

        internal static void Dispatch(long sender, long playerId, string playerName, string action, string argument)
        {
            GroupState.PeerPlayers[sender] = playerId;
            switch (action)
            {
                case "state": SendSnapshot(sender, playerId); break;
                case "invite": Invite(sender, playerId, playerName, argument); break;
                case "accept": Accept(sender, playerId, playerName, argument); break;
                case "reject": Reject(sender, playerId, argument); break;
                case "leave": Leave(sender, playerId); break;
                case "remove": Remove(sender, playerId, argument); break;
                case "promote": Promote(sender, playerId, argument); break;
                case "info": SendInfo(sender, playerId); break;
                case "chat": SendGroupChat(sender, playerId, argument); break;
            }
        }

        internal static void ReadResponse(ZPackage package)
        {
            string type = package.ReadString();
            if (type == "message")
            {
                Chat.instance?.AddString(package.ReadString());
            }
            else if (type == "snapshot")
            {
                ReadSnapshot(package);
            }
            else if (type == "invite")
            {
                ShowInvite(package.ReadString(), package.ReadString());
            }
        }

        private static void EnsureRpcs()
        {
            if (!RpcRegistry.RegisterIfChanged(ref registeredRpc, RegisterRpcs))
            {
                return;
            }
            GroupStorage.Reset();
            GroupState.ClearAll();
            DecayProtection.Reset();
            PiecePermissions.Reset();
        }

        private static void RegisterRpcs(ZRoutedRpc rpc)
        {
            rpc.Register<ZPackage>(RequestRpc, GroupRpc.RPC_Request);
            rpc.Register<ZPackage>(ResponseRpc, GroupRpc.RPC_Response);
        }

        private static void Invite(long sender, long inviter, string inviterName, string targetName)
        {
            long targetPeer = FindPeerByName(targetName);
            if (!TryGetPlayerForPeer(targetPeer, out long target))
            {
                SendMessage(sender, "Player not found or not ready.");
                return;
            }
            if (!CanInvite(sender, inviter, target))
            {
                return;
            }
            GroupState.Invitations[target] = inviter;
            ZPackage response = NewResponse("invite");
            response.Write(inviter.ToString());
            response.Write(inviterName);
            ZRoutedRpc.instance.InvokeRoutedRPC(targetPeer, ResponseRpc, response);
            SendMessage(sender, "Group invitation sent.");
        }

        private static bool CanInvite(long sender, long inviter, long target)
        {
            SocialGroup inviterGroup = GroupState.GetGroup(inviter);
            if (GroupState.PlayerGroups.ContainsKey(target))
            {
                SendMessage(sender, "That player is already in a group.");
                return false;
            }
            if (inviterGroup != null && inviterGroup.Leader != inviter)
            {
                SendMessage(sender, "Only the group leader can invite players.");
                return false;
            }
            if (inviterGroup != null && inviterGroup.Members.Count >= GroupState.MaximumSize)
            {
                SendMessage(sender, "Your group is full.");
                return false;
            }
            return inviter != target;
        }

        private static void Accept(long sender, long playerId, string playerName, string inviterText)
        {
            if (!long.TryParse(inviterText, out long inviter) ||
                !GroupState.Invitations.TryGetValue(playerId, out long expected) || expected != inviter)
            {
                SendMessage(sender, "That group invitation is no longer valid.");
                return;
            }
            SocialGroup group = GetOrCreateGroup(inviter);
            if (group == null || group.Members.Count >= GroupState.MaximumSize)
            {
                SendMessage(sender, "That group is no longer available.");
                return;
            }
            group.Members[playerId] = playerName;
            GroupState.PlayerGroups[playerId] = group.Id;
            GroupState.Invitations.Remove(playerId);
            SaveAndBroadcast(group, playerName + " joined the group.");
        }

        private static SocialGroup GetOrCreateGroup(long inviter)
        {
            SocialGroup group = GroupState.GetGroup(inviter);
            if (group != null)
            {
                return group.Leader == inviter ? group : null;
            }
            group = new SocialGroup { Id = GroupState.GetNextGroupId(), Leader = inviter };
            group.Members[inviter] = GetPlayerName(inviter);
            GroupState.Groups[group.Id] = group;
            GroupState.PlayerGroups[inviter] = group.Id;
            return group;
        }

        private static void Reject(long sender, long playerId, string inviterText)
        {
            GroupState.Invitations.Remove(playerId);
            SendMessage(sender, "Group invitation rejected.");
            if (long.TryParse(inviterText, out long inviter))
            {
                SendMessage(FindPeer(inviter), GetPlayerName(playerId) + " rejected the group invitation.");
            }
        }

        private static void Leave(long sender, long playerId)
        {
            SocialGroup group = GroupState.GetGroup(playerId);
            if (group == null)
            {
                SendMessage(sender, "You are not in a group.");
                return;
            }
            string name = group.Members[playerId];
            group.Members.Remove(playerId);
            GroupState.PlayerGroups.Remove(playerId);
            NormalizeAfterRemoval(group);
            GroupStorage.Save();
            SendMessage(sender, "You left the group.");
            Broadcast(group, name + " left the group.");
            BroadcastSnapshots(group);
        }

        private static void Remove(long sender, long actor, string targetName)
        {
            SocialGroup group = GroupState.GetGroup(actor);
            long target = FindMember(group, targetName);
            if (!ValidateLeaderAction(sender, actor, targetName, group, target)
                || !ValidateRemoveTarget(sender, actor, target))
            {
                return;
            }
            string name = group.Members[target];
            group.Members.Remove(target);
            GroupState.PlayerGroups.Remove(target);
            NormalizeAfterRemoval(group);
            GroupStorage.Save();
            SendMessage(FindPeer(target), "You were removed from the group.");
            Broadcast(group, name + " was removed from the group.");
            BroadcastSnapshots(group);
        }

        private static void Promote(long sender, long actor, string targetName)
        {
            SocialGroup group = GroupState.GetGroup(actor);
            long target = FindMember(group, targetName);
            if (!ValidateLeaderAction(sender, actor, targetName, group, target)
                || !ValidatePromoteTarget(sender, actor, target))
            {
                return;
            }
            group.Leader = target;
            GroupStorage.Save();
            Broadcast(group, group.Members[target] + " is now the group leader.");
            BroadcastSnapshots(group);
        }

        private static bool ValidateLeaderAction(
            long sender, long actor, string targetName, SocialGroup group, long target)
        {
            if (group == null)
            {
                SendMessage(sender, "You are not in a group.");
                return false;
            }
            if (group.Leader != actor)
            {
                SendMessage(sender, "Only the group leader can do that.");
                return false;
            }
            if (target == 0L)
            {
                SendMessage(sender, "Player not found in your group: " + targetName);
                return false;
            }
            return true;
        }

        private static bool ValidateRemoveTarget(long sender, long actor, long target)
        {
            if (target != actor) return true;
            SendMessage(sender, "You cannot remove yourself.");
            return false;
        }

        private static bool ValidatePromoteTarget(long sender, long actor, long target)
        {
            if (target != actor) return true;
            SendMessage(sender, "You are already group leader.");
            return false;
        }

        private static void NormalizeAfterRemoval(SocialGroup group)
        {
            if (group.Members.Count <= 1)
            {
                foreach (long member in group.Members.Keys)
                {
                    GroupState.PlayerGroups.Remove(member);
                    SendMessage(FindPeer(member), "The group was disbanded.");
                    SendSnapshot(FindPeer(member), member);
                }
                GroupState.Groups.Remove(group.Id);
                group.Members.Clear();
                return;
            }
            if (!group.Members.ContainsKey(group.Leader))
            {
                foreach (long member in group.Members.Keys)
                {
                    group.Leader = member;
                    break;
                }
            }
        }

        private static void SendGroupChat(long sender, long actor, string message)
        {
            SocialGroup group = GroupState.GetGroup(actor);
            if (group == null)
            {
                SendMessage(sender, "You are not in a group.");
                return;
            }
            if (!HasOtherOnlineMember(group, actor))
            {
                SendMessage(sender, "No other group member is connected.");
                return;
            }
            Broadcast(group, ChatFormatting.FormatGroup(group.Members[actor], message));
        }

        private static bool HasOtherOnlineMember(SocialGroup group, long actor)
        {
            foreach (long member in group.Members.Keys)
            {
                if (member != actor && FindPeer(member) != 0L)
                {
                    return true;
                }
            }
            return false;
        }

        private static void SendInfo(long sender, long playerId)
        {
            SocialGroup group = GroupState.GetGroup(playerId);
            if (group == null)
            {
                SendMessage(sender, "You are not in a group.");
                return;
            }
            List<string> lines = new List<string> { "Group members:" };
            foreach (KeyValuePair<long, string> member in group.Members)
            {
                string online = FindPeer(member.Key) != 0L ? "Connected" : "Disconnected";
                string leader = member.Key == group.Leader ? " - Group Leader" : "";
                lines.Add(member.Value + " - " + online + leader);
            }
            SendMessage(sender, string.Join("\n", lines));
        }

        private static void SaveAndBroadcast(SocialGroup group, string message)
        {
            GroupStorage.Save();
            Broadcast(group, message);
            BroadcastSnapshots(group);
            SocializePlugin.Log.LogInfo(message);
        }

        private static void BroadcastSnapshots(SocialGroup group)
        {
            if (group == null)
            {
                return;
            }
            foreach (long member in group.Members.Keys)
            {
                SendSnapshot(FindPeer(member), member);
            }
        }

        private static void SendSnapshot(long peer, long playerId)
        {
            if (peer == 0L)
            {
                return;
            }
            SocialGroup group = GroupState.GetGroup(playerId);
            ZPackage response = NewResponse("snapshot");
            response.Write(group != null ? group.Leader : 0L);
            response.Write(group != null ? group.Members.Count : 0);
            if (group != null)
            {
                foreach (KeyValuePair<long, string> member in group.Members)
                {
                    response.Write(member.Key);
                    response.Write(member.Value);
                    GroupMapSharing.WritePosition(response, member.Key);
                }
            }
            DecayProtection.WriteState(response);
            PiecePermissions.WriteState(response);
            ZRoutedRpc.instance.InvokeRoutedRPC(peer, ResponseRpc, response);
        }

        private static void ReadSnapshot(ZPackage package)
        {
            package.ReadLong();
            GroupState.LocalMembers.Clear();
            GroupMapSharing.Clear();
            int count = package.ReadInt();
            for (int index = 0; index < count; index++)
            {
                long playerId = package.ReadLong();
                string playerName = package.ReadString();
                GroupState.LocalMembers.Add(playerId);
                GroupMapSharing.ReadPosition(package, playerId, playerName);
            }
            DecayProtection.ReadState(package);
            PiecePermissions.ReadState(package);
        }

        private static void ShowInvite(string inviterId, string inviterName)
        {
            UnifiedPopup.Push(new YesNoPopup(
                "Group invitation",
                inviterName + " invited you to a group.",
                () => RespondToInvite("accept", inviterId),
                () => RespondToInvite("reject", inviterId),
                localizeText: false));
        }

        private static void RespondToInvite(string action, string inviterId)
        {
            UnifiedPopup.Pop();
            SendRequest(action, inviterId);
        }

        private static void Broadcast(SocialGroup group, string message)
        {
            if (group == null)
            {
                return;
            }
            foreach (long member in group.Members.Keys)
            {
                SendMessage(FindPeer(member), message);
            }
        }

        private static void SendMessage(long peer, string message)
        {
            if (peer == 0L)
            {
                Chat.instance?.AddString(message);
                return;
            }
            ZPackage response = NewResponse("message");
            response.Write(message ?? "");
            ZRoutedRpc.instance.InvokeRoutedRPC(peer, ResponseRpc, response);
        }

        private static ZPackage NewResponse(string type)
        {
            ZPackage package = new ZPackage();
            package.Write(type);
            return package;
        }

        private static long FindMember(SocialGroup group, string name)
        {
            if (group == null)
            {
                return 0L;
            }
            foreach (KeyValuePair<long, string> member in group.Members)
            {
                if (string.Equals(member.Value, name, StringComparison.OrdinalIgnoreCase))
                {
                    return member.Key;
                }
            }
            return 0L;
        }

        private static long FindPeerByName(string name)
        {
            if (ZNet.instance == null)
            {
                return 0L;
            }
            foreach (ZNet.PlayerInfo player in ZNet.instance.GetPlayerList())
            {
                if (string.Equals(player.m_name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return player.m_characterID.UserID;
                }
            }
            return 0L;
        }

        private static bool TryGetPlayerForPeer(long peer, out long playerId)
        {
            playerId = 0L;
            return peer != 0L && GroupState.PeerPlayers.TryGetValue(peer, out playerId);
        }

        private static long FindPeer(long playerId)
        {
            foreach (KeyValuePair<long, long> mapping in GroupState.PeerPlayers)
            {
                if (mapping.Value == playerId && ZNet.instance != null &&
                    ZNet.instance.GetPeer(mapping.Key) is ZNetPeer peer && peer.IsReady())
                {
                    return mapping.Key;
                }
            }
            return 0L;
        }

        private static string GetPlayerName(long playerId)
        {
            SocialGroup group = GroupState.GetGroup(playerId);
            if (group != null && group.Members.TryGetValue(playerId, out string name))
            {
                return name;
            }
            foreach (KeyValuePair<long, long> mapping in GroupState.PeerPlayers)
            {
                if (mapping.Value == playerId && ZNet.instance.GetPeer(mapping.Key) is ZNetPeer peer)
                {
                    return peer.m_playerName;
                }
            }
            return playerId.ToString();
        }
    }
}
