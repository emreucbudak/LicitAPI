namespace Licit.TenderingService.Application.Features.CQRS.Category.GetTree;

public record GetCategoryTreeQueryResponse(
    List<CategoryTreeItemDto> Categories
);

public record CategoryTreeItemDto(
    Guid Id,
    string Name,
    Guid? ParentCategoryId,
    List<CategoryTreeItemDto> Children
);
