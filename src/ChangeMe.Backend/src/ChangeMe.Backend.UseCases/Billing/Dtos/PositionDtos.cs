using ChangeMe.Backend.Domain.Aggregates.Billing;

namespace ChangeMe.Backend.UseCases.Billing.Dtos;

public class PositionListItemDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Department { get; set; }
  public bool IsActive { get; set; }
  public int ContractCount { get; set; }
  public bool CanManage { get; set; }
}

public class PositionDetailsDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Department { get; set; }
  public string? Description { get; set; }
  public bool IsActive { get; set; }
  public int ContractCount { get; set; }
  public bool CanManage { get; set; }
  public bool CanDelete { get; set; }
}

public record CreatePositionRequest(string Name, string? Department, string? Description, bool IsActive = true);

public record UpdatePositionRequest(string Name, string? Department, string? Description, bool IsActive);
