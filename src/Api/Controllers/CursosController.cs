using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using NoSqlU.Api.Models;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CursosController : ControllerBase
{
    private readonly CursosModel _model;

    public CursosController(CursosModel model) => _model = model;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        (await _model.GetAllAsync()).ToActionResult(this);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) =>
        (await _model.GetAsync(id)).ToActionResult(this);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CourseCreateDto dto) =>
        (await _model.CreateAsync(dto)).ToActionResult(this, nameof(Get));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] JsonElement body) =>
        (await _model.UpdateAsync(id, body)).ToActionResult(this);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        (await _model.DeleteAsync(id)).ToActionResult(this);
}
