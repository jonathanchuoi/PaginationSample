using AutoFixture;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace PaginationSample.Controllers;

[ApiController]
[Route("[controller]")]
public class Your2Controller : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromServices] IYour2Repository repository, [FromQuery] YourDto dto)
    {
        if (dto.Page < 1 || dto.Size < 1)
        {
            return BadRequest("Invalid page or size parameter");
        }

        var pagedList = await repository.GetPagedListAsync(dto.Page, dto.Size);
        var response = new
        {
            Data = pagedList,
            Meta = new
            {
                TotalCount = pagedList.TotalItemCount,
                PageCount = pagedList.PageCount,
                CurrentPage = pagedList.PageNumber,
                PageSize = pagedList.PageSize,
                HasNextPage = pagedList.HasNextPage,
                HasPreviousPage = pagedList.HasPreviousPage
            },
            Links = new
            {
                First = $"{Request.GetEncodedUrl()}?page=1&size={dto.Size}",
                Last = $"{Request.GetEncodedUrl()}?page={pagedList.PageCount}&size={dto.Size}",
                Next = pagedList.HasNextPage ? $"{Request.GetEncodedUrl()}?page={pagedList.PageNumber + 1}&size={dto.Size}" : null,
                Previous = pagedList.HasPreviousPage ? $"{Request.GetEncodedUrl()}?page={pagedList.PageNumber - 1}&size={dto.Size}" : null
            }


        };

        return Ok(response);
    }
}

public class Your2Dto
{
    public int Page { get; set; }
    public int Size { get; set; }
}

public interface IYour2Repository
{
    Task<IPagedList<Person>> GetPagedListAsync(int page, int size);
}

public class Your2Repository: IYour2Repository
{
    private readonly List<Person> _people;

    public Your2Repository()
    {
        var fixture = new Fixture();
        _people = fixture.CreateMany<Person>(1000).ToList();
    }
    
    public async Task<IPagedList<Person>> GetPagedListAsync(int page, int size)
    {
        return await _people.ToPagedListAsync(page, size);
    }
}