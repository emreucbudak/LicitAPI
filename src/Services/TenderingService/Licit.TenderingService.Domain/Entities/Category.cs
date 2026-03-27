using Licit.TenderingService.Domain.Common;

namespace Licit.TenderingService.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = null!;
    public Guid? ParentCategoryId { get; private set; }

    public Category? ParentCategory { get; private set; }
    public ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public ICollection<Tender> Tenders { get; private set; } = new List<Tender>();

    private Category() { }

    public Category(string name, Guid? parentCategoryId = null)
    {
        Name = name;
        ParentCategoryId = parentCategoryId;
    }
}
