using be_rallyhubv2.contract.abstractions.pagination;

namespace be_rallyhubv2.tests.unit.abstractions;

public class PaginationTests
{
    [Theory]
    [InlineData(0, 0, 1, 10)]      // 0 -> 1, pageSize 0 -> mặc định 10
    [InlineData(-3, 500, 1, 100)]  // âm -> 1, vượt trần -> 100
    [InlineData(2, 20, 2, 20)]     // hợp lệ giữ nguyên
    public void Request_values_are_clamped(int page, int pageSize, int expectedPage, int expectedSize)
    {
        var request = new PaginationRequest() { Page = page, PageSize = pageSize };

        Assert.Equal(expectedPage, request.Page);
        Assert.Equal(expectedSize, request.PageSize);
    }

    [Fact]
    public void Skip_is_calculated_from_page_and_size()
        => Assert.Equal(20, new PaginationRequest { Page = 3, PageSize = 10 }.Skip);

    [Fact]
    public void Paged_result_flags_are_calculated()
    {
        var page = new PagedResult<int>(new[] { 1 }, Page: 2, PageSize: 10, TotalCount: 25);

        Assert.Equal(3, page.TotalPages);
        Assert.True(page.HasPreviousPage);
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public void Empty_result_has_no_next_page()
        => Assert.False(new PagedResult<int>(Array.Empty<int>(), 1, 10, 0).HasNextPage);
}