using System;
using System.Collections;
using UnityEngine;

namespace Landoria.CharacterVault
{
    internal sealed class VoluntaryDisconnectCoordinator : IDisposable
    {
        private const float ConfirmationTimeoutSeconds = 30f;
        private bool _allowApplicationQuit;
        private bool _allowLogout;
        private Game _game;
        private bool _logoutSave;
        private bool _logoutStartScene;
        private string _requestId;
        private VoluntaryExitKind _exitKind;

        internal VoluntaryDisconnectCoordinator()
        {
            Application.wantsToQuit += AllowApplicationQuit;
        }

        internal bool AllowLogout(Game game, bool save, bool changeToStartScene)
        {
            if (_allowLogout)
            {
                _allowLogout = false;
                CharacterVaultPlugin.Log.LogInfo(
                    "Allowing voluntary logout after the final character save was accepted.");
                return true;
            }

            if (!save || !Start(VoluntaryExitKind.Logout, game, save, changeToStartScene))
            {
                return true;
            }

            return false;
        }

        internal void RecordSaveCommitted(string requestId, long revision)
        {
            if (requestId != _requestId)
            {
                return;
            }

            CharacterVaultPlugin.Log.LogMessage(
                $"Final voluntary disconnect save {requestId} accepted at revision {revision}.");
            VoluntaryExitKind exitKind = _exitKind;
            Game game = _game;
            bool logoutSave = _logoutSave;
            bool logoutStartScene = _logoutStartScene;
            ClearPendingRequest();
            CharacterVaultPlugin.Transfers.SuppressRedundantDisconnectUpload();
            if (exitKind == VoluntaryExitKind.ApplicationQuit)
            {
                _allowApplicationQuit = true;
                CharacterVaultPlugin.Log.LogInfo("Allowing application quit after the confirmed save.");
                Application.Quit();
                return;
            }

            _allowLogout = true;
            game.Logout(logoutSave, logoutStartScene);
        }

        internal void RecordConnectionLost()
        {
            if (_requestId == null)
            {
                return;
            }

            CharacterVaultPlugin.Log.LogWarning(
                $"Connection was lost while final save {_requestId} was pending; confirmation is impossible.");
            ClearPendingRequest();
        }

        internal bool AllowMenuQuit()
        {
            if (_allowApplicationQuit)
            {
                return true;
            }

            bool delayed = Start(VoluntaryExitKind.ApplicationQuit, Game.instance, true, false);
            if (delayed)
            {
                CharacterVaultPlugin.Log.LogMessage(
                    "Intercepted the in-game Quit action; waiting for the final save acceptance.");
            }
            return !delayed;
        }

        public void Dispose()
        {
            Application.wantsToQuit -= AllowApplicationQuit;
            ClearPendingRequest();
        }

        private bool AllowApplicationQuit()
        {
            if (_allowApplicationQuit)
            {
                CharacterVaultPlugin.Log.LogInfo("Application quit authorization consumed.");
                return true;
            }

            return !Start(VoluntaryExitKind.ApplicationQuit, Game.instance, true, false);
        }

        private bool Start(VoluntaryExitKind kind, Game game, bool save, bool startScene)
        {
            if (_requestId != null)
            {
                CharacterVaultPlugin.Log.LogWarning(
                    $"Ignored another voluntary exit request while final save {_requestId} is pending.");
                return true;
            }

            string requestId = "disconnect-" + Guid.NewGuid().ToString("N");
            if (CharacterVaultPlugin.Transfers?.BeginFinalDisconnectSave(requestId) != true)
            {
                return false;
            }

            _requestId = requestId;
            _exitKind = kind;
            _game = game;
            _logoutSave = save;
            _logoutStartScene = startScene;
            CharacterVaultPlugin.Log.LogMessage(
                $"Delayed voluntary {Describe(kind)} until final save {requestId} is committed.");
            CharacterVaultPlugin.Instance.Run(WaitForConfirmation(requestId));
            return true;
        }

        private IEnumerator WaitForConfirmation(string requestId)
        {
            float deadline = Time.realtimeSinceStartup + ConfirmationTimeoutSeconds;
            while (_requestId == requestId && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            if (_requestId != requestId)
            {
                yield break;
            }

            CharacterVaultPlugin.Log.LogError(
                $"Canceled voluntary {Describe(_exitKind)} because final save {requestId} " +
                $"was not confirmed within {ConfirmationTimeoutSeconds:0} seconds.");
            ClearPendingRequest();
        }

        private void ClearPendingRequest()
        {
            _requestId = null;
            _game = null;
        }

        private static string Describe(VoluntaryExitKind kind)
        {
            return kind == VoluntaryExitKind.ApplicationQuit ? "application quit" : "logout";
        }
    }

    internal enum VoluntaryExitKind
    {
        Logout,
        ApplicationQuit
    }
}
