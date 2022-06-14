using AutoMapper;
using Microsoft.EntityFrameworkCore;
using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using System.Collections.Immutable;
using Vocabulary.Categories.DataContracts;
using Vocabulary.Categories.Ports;
using Vocabulary.DataContracts.Types;

namespace Vocabulary.Adapters.Persistance.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbContextFactory<VocabularyDbContext> _dbContextFactory;
    private readonly IMapper _mapper;

    public CategoryRepository(IDbContextFactory<VocabularyDbContext> dbContextFactory, IMapper mapper)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TCategory"></typeparam>
    /// <returns></returns>
    public async Task<IEnumerable<NavCategory>> GetNavCategoriesAsync<TCategory>()
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var dbCategories = await dbContext.Categories.Include(c => c.Terms).ToArrayAsync();

        return dbCategories.Select(c => 
            NavCategoryModule.create(
                c.Id,
                c.Name,
                c.Terms.Select(
                    t => TermNameModule.create(t.Id, t.Name, t.AdditionalName)))
            );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    public Task<Result<Category>> FindAsync(Guid categoryId)
    {
        return InternalFindAsync(categoryId).TaskMap(r => r.Map(_mapper.Map<Category>));
    }

    public async Task<Result> RemoveAsync(Guid categoryId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        Models.Category? category = await dbContext.Categories.Include(c => c.Terms).SingleOrDefaultAsync(c => c.Id == categoryId);

        if (category is not null)
        {
            if (!category.Terms.Any())
            {
                dbContext.Categories.Remove(category);
                await dbContext.SaveChangesAsync();
                return Result.Success();
            }

            return Result.Failure("Category has terms.");
        }

        return Result.Failure("Category is not in db.");
    }

    private async Task<Result<Models.Category>> InternalFindAsync(Guid categoryId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        Models.Category? category = await dbContext.Categories.FindAsync(categoryId);

        if (category is not null)
        {
            return category.ToSuccessResult();
        }

        return Result<Models.Category>.Failure($"Could not find Category by id {categoryId}");
    }


}