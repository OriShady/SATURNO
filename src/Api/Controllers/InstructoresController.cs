using Microsoft.AspNetCore.Mvc;
using NoSqlU.Api.Models;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstructoresController : ControllerBase
{
    private readonly InstructoresModel _model;

    public InstructoresController(InstructoresModel model) => _model = model;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        (await _model.GetAllAsync()).ToActionResult(this);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) =>
        (await _model.GetAsync(id)).ToActionResult(this);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InstructorInput model) =>
        (await _model.CreateAsync(model)).ToActionResult(this, nameof(Get));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] InstructorInput model) =>
        (await _model.UpdateAsync(id, model)).ToActionResult(this);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        (await _model.DeleteAsync(id)).ToActionResult(this);
}
