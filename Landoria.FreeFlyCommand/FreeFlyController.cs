using UnityEngine;

namespace Landoria.FreeFlyCommand
{
    internal static class FreeFlyController
    {
        internal const float MaximumDistance = 50f;

        internal static void Toggle()
        {
            if (!FreeFlyAuthorization.IsAuthorized || GameCamera.instance == null)
            {
                return;
            }

            GameCamera.instance.ToggleFreeFly();
        }

        internal static void Disable()
        {
            if (GameCamera.instance != null && GameCamera.InFreeFly())
            {
                GameCamera.instance.ToggleFreeFly();
            }
        }

        internal static void ClampToPlayer(GameCamera camera)
        {
            Player player = Player.m_localPlayer;
            if (!FreeFlyAuthorization.IsAuthorized || !GameCamera.InFreeFly() || player == null)
            {
                return;
            }

            Vector3 offset = camera.transform.position - player.transform.position;
            if (offset.sqrMagnitude > MaximumDistance * MaximumDistance)
            {
                camera.transform.position = player.transform.position +
                    offset.normalized * MaximumDistance;
            }
        }
    }
}
