using AutoMapper;
using Microsoft.EntityFrameworkCore;
using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using System.Collections.Immutable;
using Vocabulary.Categories.DataContracts;
using Vocabulary.Categories.Ports;

namespace Vocabulary.Adapters.Persistance.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbContextFactory<VocabularyDbContext> _dbContextFactory;
    private readonly IMapper _mapper;

    public CategoryRepository(IDbContextFactory<VocabularyDbContext> dbContextFactory, IMapper mapper)
    {
        _dbContextFactory = dbContextFactory;
        this._mapper = mapper;
    }

    public Task<Result<Category, string>> FindAsync(Guid categoryId)
    {
        return InternalFindAsync(categoryId).TaskMap(r => r.Map(_mapper.Map<Category>));
    }

    public async Task<ImmutableArray<Category>> GetCategoriesAsync()
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var dbCategories = await dbContext.Categories.ToListAsync();

        return dbCategories.Select(_mapper.Map<Category>).ToImmutableArray();
    }

    public async Task<Result<Unit, string>> RemoveAsync(Guid categoryId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        Models.Category? category = await dbContext.Categories.Include(c => c.Terms).SingleOrDefaultAsync(c => c.Id == categoryId);

        if (category is not null)
        {
            if (!category.Terms.Any())
            {
                dbContext.Categories.Remove(category);
                await dbContext.SaveChangesAsync();
                return Result.UnitOkWith<string>();
            }

            return "Category has terms.".ToError<Unit>();
        }

        return "Category is not in db.".ToError<Unit>();
    }

    private async Task<Result<Models.Category, string>> InternalFindAsync(Guid categoryId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        Models.Category? category = await dbContext.Categories.FindAsync(categoryId);

        if (category is not null)
        {
            return category.ToOkWithStringError();
        }

        return $"Could not find Category by id {categoryId}".ToError<Models.Category>();
    }
}