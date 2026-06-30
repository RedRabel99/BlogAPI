using BlogAPI.Application.Common.Pagination;
using MockQueryable;

namespace BlogAPI.Application.UnitTests.Pagination;

public class PagedListTests
{
    private static IQueryable<Box> Items(int count) =>
        Enumerable.Range(1, count).Select(i => new Box(i)).ToList().BuildMock();

    [Fact]
    public async Task CreateAsync_AppliesSkipAndTake()
    {
        //Act
        // 25 items, page 2, size 10 -> items 11..20
        var page = await PagedList<Box>.CreateAsync(Items(25), page: 2, pageSize: 10);

        //Assert
        Assert.Equal(10, page.Items.Count);
        Assert.Equal(11, page.Items.First().Value);
        Assert.Equal(20, page.Items.Last().Value);
        Assert.Equal(25, page.TotalCount);
        Assert.Equal(2, page.Page);
        Assert.Equal(10, page.PageSize);
    }

    [Fact]
    public async Task HasNextPage_WhenMoreItemsRemain_IsTrue()
    {
        //Act
        // page 2 of 25 with size 10: 2*10=20 < 25
        var page = await PagedList<Box>.CreateAsync(Items(25), page: 2, pageSize: 10);

        //Assert
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public async Task HasNextPage_OnLastPage_IsFalse()
    {
        //Act
        // page 3 of 25 with size 10: 3*10=30, not < 25
        var page = await PagedList<Box>.CreateAsync(Items(25), page: 3, pageSize: 10);

        //Assert
        Assert.False(page.HasNextPage);
    }

    [Fact]
    public async Task HasPreviousPage_OnFirstPage_IsFalse()
    {
        //Act
        var page = await PagedList<Box>.CreateAsync(Items(25), page: 1, pageSize: 10);

        //Assert
        Assert.False(page.HasPreviousPage);
    }

    [Fact]
    public async Task HasPreviousPage_AfterFirstPage_IsTrue()
    {
        //Act
        var page = await PagedList<Box>.CreateAsync(Items(25), page: 2, pageSize: 10);

        //Assert
        Assert.True(page.HasPreviousPage);
    }

    [Fact]
    public async Task CreateAsync_WithPageBeyondRange_YieldsEmptyItems()
    {
        //Act
        // 25 items, page 99, size 10: Skip past end -> empty, but TotalCount intact
        var page = await PagedList<Box>.CreateAsync(Items(25), page: 99, pageSize: 10);

        //Assert
        Assert.Empty(page.Items);
        Assert.Equal(25, page.TotalCount);
        Assert.False(page.HasNextPage);
        Assert.True(page.HasPreviousPage);
    }
}
