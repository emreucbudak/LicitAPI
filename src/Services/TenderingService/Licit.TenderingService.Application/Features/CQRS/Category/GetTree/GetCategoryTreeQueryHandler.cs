using FlashMediator;
using Licit.TenderingService.Application.Interfaces;

using CategoryEntity = Licit.TenderingService.Domain.Entities.Category;

namespace Licit.TenderingService.Application.Features.CQRS.Category.GetTree;

public class GetCategoryTreeQueryHandler(
    IUnitOfWork unitOfWork) : IRequestHandler<GetCategoryTreeQueryRequest, GetCategoryTreeQueryResponse>
{
    public async Task<GetCategoryTreeQueryResponse> Handle(GetCategoryTreeQueryRequest request, CancellationToken cancellationToken)
    {
        var categories = await unitOfWork.Categories.GetAllAsync(cancellationToken);
        var childrenByParentId = categories
            .Where(c => c.ParentCategoryId is not null)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Name).ToList());

        var roots = categories
            .Where(c => c.ParentCategoryId is null)
            .OrderBy(c => c.Name)
            .Select(c => MapCategory(c, childrenByParentId))
            .ToList();

        return new GetCategoryTreeQueryResponse(roots);
    }

    private static CategoryTreeItemDto MapCategory(
        CategoryEntity category,
        IReadOnlyDictionary<Guid, List<CategoryEntity>> childrenByParentId)
    {
        var children = childrenByParentId.TryGetValue(category.Id, out var childCategories)
            ? childCategories.Select(c => MapCategory(c, childrenByParentId)).ToList()
            : [];

        return new CategoryTreeItemDto(
            category.Id,
            category.Name,
            category.ParentCategoryId,
            children);
    }
}
