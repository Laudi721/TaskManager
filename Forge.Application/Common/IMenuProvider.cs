using Forge.Common.Dtos;

namespace Forge.Application.Common
{
    public interface IMenuProvider
    {
        IReadOnlyList<MenuItem> GetMenu();
    }
}
