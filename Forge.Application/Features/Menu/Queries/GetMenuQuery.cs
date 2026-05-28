using Forge.Application.Common;
using Forge.Common.Dtos;
using MediatR;

namespace Forge.Application.Features.Menu.Queries
{
    public sealed record GetMenuQuery() : IRequest<IReadOnlyList<MenuItem>>;

    public sealed class GetMenuQueryHandler : IRequestHandler<GetMenuQuery, IReadOnlyList<MenuItem>>
    {
        private readonly IMenuProvider _menuProvider;

        public GetMenuQueryHandler(IMenuProvider menuProvider)
        {
            _menuProvider = menuProvider;
        }

        public Task<IReadOnlyList<MenuItem>> Handle(GetMenuQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_menuProvider.GetMenu());
        }
    }
}
