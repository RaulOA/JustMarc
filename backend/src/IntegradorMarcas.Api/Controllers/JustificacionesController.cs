using IntegradorMarcas.Api.Contracts.Requests;
using IntegradorMarcas.Api.Contracts.Responses;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace IntegradorMarcas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class JustificacionesController : ControllerBase
{
    private readonly IJustificacionService _service;
    private readonly IUserContext _userContext;

    public JustificacionesController(IJustificacionService service, IUserContext userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    [HttpPost]
    public async Task<ActionResult<CreateJustificacionResponse>> Create(
        [FromBody] CreateJustificacionRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var dto = new CreateJustificacionDto
        {
            MotivoGeneral = request.MotivoGeneral,
            Detalles = request.Detalles.Select(d => new JustificacionDetalleDto
            {
                TipoJustificacionId = d.TipoJustificacionID,
                FechaMarca = d.FechaMarca,
                ObservacionDetalle = d.ObservacionDetalle
            }).ToList()
        };

        var id = await _service.CreateAsync(user, dto, cancellationToken);

        return CreatedAtAction(nameof(ListMine), new
        {
            id
        }, new CreateJustificacionResponse
        {
            JustificacionID = id,
            EstadoID = EstadoIds.PendienteJefatura,
            EstadoDescripcion = "Pendiente Jefatura"
        });
    }

    [HttpGet("mias")]
    public async Task<ActionResult<IReadOnlyList<JustificacionResumenResponse>>> ListMine(
        [FromQuery] int? estadoId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.ListMineAsync(user, new FiltroJustificacionesDto
        {
            EstadoId = estadoId,
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
}
