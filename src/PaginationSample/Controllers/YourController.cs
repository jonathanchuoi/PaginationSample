using AutoFixture;
using Microsoft.AspNetCore.Mvc;

namespace PaginationSample.Controllers;

[ApiController]
[Route("[controller]")]
public class YourController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromServices] IYourRepository repository, [FromQuery] YourDto dto)
    {
        if (dto.Page < 1 || dto.Size < 1)
        {
            return BadRequest("Invalid page or size parameter");
        }

        var data = await repository.GetDataAsync(dto.Page, dto.Size);
        var total = await repository.GetTotalAsync();

        var result = new PagedResult<Person>(data, total, dto.Page, dto.Size, Request);

        return Ok(result);
    }
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Data { get; }
    public Links Links { get; }
    public Meta Meta { get; }
    
    
    public PagedResult(IReadOnlyList<T> data, int total, int page, int size, HttpRequest request)
    {
        var totalPages = (int)Math.Ceiling((double)total / size);

        Links = new Links(request, totalPages, page, size);
        Meta = new Meta(total, data.Count, size, page, totalPages);

        Data = data;
    }
}

public class Links
{
    public string? Self { get; }
    public string? First { get; }
    public string? Last { get; }
    public string? Prev { get; }
    public string? Next { get; }

    public Links(HttpRequest request, int totalPages, int currentPage, int pageSize)
    {
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}";

        Self = ConstructUrl(baseUrl, currentPage, pageSize);
        First = ConstructUrl(baseUrl, 1, pageSize);
        Last = ConstructUrl(baseUrl, totalPages, pageSize);
        Prev = currentPage > 1 ? ConstructUrl(baseUrl, currentPage - 1, pageSize) : null;
        Next = currentPage < totalPages ? ConstructUrl(baseUrl, currentPage + 1, pageSize) : null;
    }


    private static string ConstructUrl(string baseUrl, int page, int size)
    {
        return $"{baseUrl}?{nameof(page)}={page}&{nameof(size)}={size}";
    }
}

public class Meta
{
    public int Total { get; }
    public int Count { get; }
    public int PerPage { get; }
    public int CurrentPage { get; }
    public int TotalPages { get; }

    public Meta(int total, int count, int perPage, int currentPage, int totalPages)
    {
        Total = total;
        Count = count;
        PerPage = perPage;
        CurrentPage = currentPage;
        TotalPages = totalPages;
    }
}

public class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class YourDto
{

    public int Page { get; set; }
    public int Size { get; set; }
}

public interface IYourRepository
{
    Task<List<Person>> GetDataAsync(int page, int size);
    Task<int> GetTotalAsync();

}
public class YourRepository: IYourRepository
{
    private readonly List<Person> _people;

    public YourRepository()
    {    var fixture = new Fixture();
        _people = fixture.CreateMany<Person>(1000).ToList();
    }
    public Task<List<Person>> GetDataAsync(int page, int size)
    {
        var paginatedData = _people.Skip((page - 1) * size).Take(size).ToList();
        return Task.FromResult(paginatedData);
    }

    public Task<int> GetTotalAsync()
    {
        var query = _people.AsQueryable();
        return Task.FromResult(query.Count());
    }
}