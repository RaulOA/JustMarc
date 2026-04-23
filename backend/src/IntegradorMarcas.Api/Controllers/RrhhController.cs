using IntegradorMarcas.Api.Contracts.Responses;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntegradorMarcas.Api.Controllers;

[ApiController]
[Route("api/rrhh/justificaciones")]
public sealed class RrhhController : ControllerBase
{
    private readonly IJustificacionService _service;
    private readonly IUserContext _userContext;

    public RrhhController(IJustificacionService service, IUserContext userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RrhhJustificacionResumenResponse>>> List(
        [FromQuery] string? funcionario,
        [FromQuery] int? estadoId,
        [FromQuery] string? compania,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.ListRrhhAsync(user, new FiltroRrhhJustificacionesDto
        {
            Funcionario = funcionario,
            EstadoId = estadoId,
            Compania = compania,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        }, cancellationToken);

        return Ok(result.Select(x => new RrhhJustificacionResumenResponse
        {
            JustificacionID = x.JustificacionID,
            MotivoGeneral = x.MotivoGeneral,
            ComentarioResolucion = x.ComentarioResolucion,
            EstadoID = x.EstadoID,
            EstadoDescripcion = x.EstadoDescripcion,
            FechaCreacion = x.FechaCreacion,
            CantidadDetalles = x.CantidadDetalles,
            AprobadorID = x.AprobadorID,
            FechaAprobacion = x.FechaAprobacion,
            FuncionarioID = x.FuncionarioID,
            FuncionarioNombre = x.FuncionarioNombre,
            FuncionarioCedula = x.FuncionarioCedula,
            Compania = x.Compania,
            JefaturaID = x.JefaturaID,
            JefaturaNombre = x.JefaturaNombre,
            TipoPrincipal = x.TipoPrincipal
        }).ToList());
    }
}
