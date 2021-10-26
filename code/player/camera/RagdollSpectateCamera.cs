using Sandbox;

namespace TTTReborn.Player.Camera
{
    public class RagdollSpectateCamera : SpectateRagdollCamera, IObservationCamera
    {
        public Vector3 GetViewPosition()
        {
            return Pos;
        }
    }
}
