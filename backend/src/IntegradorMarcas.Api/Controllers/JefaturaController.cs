using IntegradorMarcas.Api.Contracts.Requests;
using IntegradorMarcas.Api.Contracts.Responses;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntegradorMarcas.Api.Controllers;

[ApiController]
[Route("api/jefatura/justificaciones")]
public sealed class JefaturaController : ControllerBase
{
    private readonly IJustificacionService _service;
    private readonly IUserContext _userContext;

    public JefaturaController(IJustificacionService service, IUserContext userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    [HttpGet("pendientes")]
    public async Task<ActionResult<IReadOnlyList<JustificacionResumenResponse>>> ListPendientes(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.ListPendientesJefaturaAsync(user, new FiltroJustificacionesDto
        {
            Desde = desde,
            Hasta = hasta
        }, cancellationToken);

        return Ok(result.Select(x => new JustificacionResumenResponse
        {
            JustificacionID = x.JustificacionId,
            MotivoGeneral = x.MotivoGeneral,
            EstadoID = x.EstadoId,
            EstadoDescripcion = x.EstadoDescripcion,
            FechaCreacion = x.FechaCreacion,
            CantidadDetalles = x.CantidadDetalles,
            AprobadorID = x.AprobadorId,
            FechaAprobacion = x.FechaAprobacion
        }).ToList());
    }

    [HttpPatch("{justificacionId:int}/resolver")]
    public async Task<ActionResult> Resolver(
        int justificacionId,
        [FromBody] ResolverJustificacionRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        await _service.ResolverAsync(user, justificacionId, new ResolverJustificacionDto
        {
            Accion = request.Accion,
            Comentario = request.Comentario
        }, cancellationToken);

        return NoContent();
    }
}
