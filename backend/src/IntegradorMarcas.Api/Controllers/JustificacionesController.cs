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
            ObservacionDetalle = x.ObservacionDetalle,
            ComentarioResolucion = x.ComentarioResolucion,
            EstadoID = x.EstadoId,
            EstadoDescripcion = x.EstadoDescripcion,
            FechaCreacion = x.FechaCreacion,
            CantidadDetalles = x.CantidadDetalles,
            AprobadorID = x.AprobadorId,
            FechaAprobacion = x.FechaAprobacion
        }).ToList());
    }

    [HttpGet("aprobador-actual")]
    public async Task<ActionResult<CurrentApproverResponse>> GetCurrentApprover(CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.GetCurrentApproverAsync(user, cancellationToken);

        return Ok(new CurrentApproverResponse
        {
            SolicitanteUsuarioID = result.SolicitanteUsuarioId,
            Origen = result.Origen,
            DeleganteUsuarioID = result.DeleganteUsuarioId,
            DeleganteNombre = result.DeleganteNombre,
            Aprobador = result.Aprobador is null
                ? null
                : new UsuarioResumenResponse
                {
                    UsuarioID = result.Aprobador.UsuarioId,
                    NombreCompleto = result.Aprobador.NombreCompleto,
                    Cedula = result.Aprobador.Cedula,
                    Correo = result.Aprobador.Correo,
                    Compania = result.Aprobador.Compania,
                    UnidadID = result.Aprobador.UnidadId,
                    UnidadNombre = result.Aprobador.UnidadNombre,
                    JefaturaID = result.Aprobador.JefaturaId
                }
        });
    }

    [HttpGet("{justificacionId:int}/lineas")]
    public async Task<ActionResult<IReadOnlyList<JustificacionDetalleLineaResponse>>> ListMineLineas(
        int justificacionId,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.ListMineLineasAsync(user, justificacionId, cancellationToken);

        return Ok(result.Select(d => new JustificacionDetalleLineaResponse
        {
            DetalleID = d.DetalleID,
            TipoJustificacionID = d.TipoJustificacionID,
            TipoJustificacionDescripcion = d.TipoJustificacionDescripcion,
            FechaMarca = d.FechaMarca,
            ObservacionDetalle = d.ObservacionDetalle
        }).ToList());
    }

    [HttpGet("historico")]
    public async Task<ActionResult<IReadOnlyList<RrhhJustificacionResumenResponse>>> ListHistorico(
        [FromQuery] string? funcionario,
        [FromQuery] int? estadoId,
        [FromQuery] string? compania,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.ListHistoricoAsync(user, new FiltroRrhhJustificacionesDto
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
