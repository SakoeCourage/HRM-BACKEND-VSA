namespace HRM_BACKEND_VSA.Contracts
{
    public class NotificationContract
    {
    }

    public record NotificationRecord<TValue>(TValue Data, string NotifiableType, Guid NotifiableId) where TValue : class;

}
