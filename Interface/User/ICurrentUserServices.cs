namespace study4_be.Interface.User
{
    public interface ICurrentUserServices
    {
        string? UserId { get; }
        string? IpAddress { get; }
    }
}
