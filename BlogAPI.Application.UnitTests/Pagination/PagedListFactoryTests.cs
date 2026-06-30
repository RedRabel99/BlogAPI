using BlogAPI.Application.Common.Pagination;
using Microsoft.Extensions.Options;
using MockQueryable;

namespace BlogAPI.Application.UnitTests.Pagination;

public class PagedListFactoryTests
{
    private static PagedListFactory Factory() =>
        new(Options.Create(new PaginationOptions
        {
            DefaultPage = 1,
            DefaultPageSize = 5,
            MinPageSize = 1,
            MaxPageSize = 50
        }));

    private static IQueryable<Box> Items(int count) =>
        Enumerable.Range(1, count).Select(i => new Box(i)).ToList().BuildMock();

    [Fact]
    public async Task CreateAsync_WithNullArgs_FallsBackToDefaults()
    {
        //Act
        var page = await Factory().CreateAsync(Items(100), page: null, pageSize: null);

        //Assert
        Assert.Equal(1, page.Page);
        Assert.Equal(5, page.PageSize);
    }

    [Fact]
    public async Task CreateAsync_WithExplicitArgs_UsesThemCorrectly()
    {
        //Act
        var page = await Factory().CreateAsync(Items(100), page: 2, pageSize: 3);

        //Assert
        Assert.Equal(2, page.Page);
        Assert.Equal(3, page.PageSize);
        Assert.True(page.HasNextPage);
    }
}
