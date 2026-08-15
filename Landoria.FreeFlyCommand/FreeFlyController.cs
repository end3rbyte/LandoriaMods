using UnityEngine;

namespace Landoria.FreeFlyCommand
{
    internal static class FreeFlyController
    {
        internal const float MaximumDistance = 50f;
        internal const float MaximumSpeed = 40f;

        internal static float ClampSpeed(float speed)
        {
            return Mathf.Clamp(speed, 1f, MaximumSpeed);
        }

        internal static void ClampFrameMovement(GameCamera camera, Vector3 origin, float deltaTime)
        {
            Vector3 movement = camera.transform.position - origin;
            float maximumMovement = MaximumSpeed * deltaTime;
            if (movement.sqrMagnitude > maximumMovement * maximumMovement)
            {
                camera.transform.position = origin + movement.normalized * maximumMovement;
            }
        }

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
