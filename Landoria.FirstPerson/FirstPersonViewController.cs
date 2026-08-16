using UnityEngine;

namespace Landoria.FirstPerson
{
    internal static class FirstPersonViewController
    {
        private const float EyeForwardOffset = 0.1f;

        internal static void Apply(GameCamera camera, Player player)
        {
            if (!camera || !player || player.IsAttached() || player.InCutscene())
            {
                return;
            }

            Quaternion cameraRotation = camera.transform.rotation;
            Vector3 lookDirection = camera.transform.forward;
            Vector3 bodyDirection = Vector3.ProjectOnPlane(lookDirection, Vector3.up);
            if (bodyDirection.sqrMagnitude > Mathf.Epsilon)
            {
                player.SetLookDir(lookDirection);
                player.transform.rotation = Quaternion.LookRotation(bodyDirection, Vector3.up);
            }

            camera.transform.position =
                player.GetEyePoint() + lookDirection * EyeForwardOffset;
            camera.transform.rotation = cameraRotation;
        }
    }
}
