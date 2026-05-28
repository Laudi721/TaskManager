namespace Forge.Database.SoftDelete
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }

        DateTime? DeletedTime { get; set; }
    }
}
