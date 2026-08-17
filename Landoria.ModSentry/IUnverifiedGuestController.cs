namespace Landoria.ModSentry
{
    public interface IUnverifiedGuestController
    {
        int ProtocolVersion { get; }
        bool IsReady { get; }
        void OnGuestAdmitted(ZRpc rpc);
        void OnGuestDisconnected(ZRpc rpc);
        void ClearGuests();
    }
}
