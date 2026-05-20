using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed class GetPermissionCatalogQuery : IQuery<IReadOnlyList<PermissionCatalogItemDto>>
{
  public bool DoNothing { get; set; }
}

public class GetPermissionCatalogHandler()
  : IQueryHandler<GetPermissionCatalogQuery, IReadOnlyList<PermissionCatalogItemDto>>
{
  public async Task<Result<IReadOnlyList<PermissionCatalogItemDto>>> Handle(
    GetPermissionCatalogQuery query,
    CancellationToken cancellationToken)
  {
    return Result.Success(RolesSupport.GetPermissionCatalog());
  }
}
