namespace Forge.Desk.WebApi.Configuration
{
    public interface IMenuProvider
    {
        IReadOnlyList<MenuItem> GetMenu();
    }
}
