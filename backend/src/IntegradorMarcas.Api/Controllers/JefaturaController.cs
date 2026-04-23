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
            ComentarioResolucion = x.ComentarioResolucion,
            EstadoID = x.EstadoId,
            EstadoDescripcion = x.EstadoDescripcion,
            FechaCreacion = x.FechaCreacion,
            CantidadDetalles = x.CantidadDetalles,
            AprobadorID = x.AprobadorId,
            FechaAprobacion = x.FechaAprobacion
        }).ToList());
    }

    [HttpGet("{justificacionId:int}")]
    public async Task<ActionResult<JustificacionDetalleCompletaResponse>> GetDetalle(
        int justificacionId,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.GetDetalleJefaturaAsync(user, justificacionId, cancellationToken);

        return Ok(new JustificacionDetalleCompletaResponse
        {
            Encabezado = new JustificacionResumenResponse
            {
                JustificacionID = result.Encabezado.JustificacionId,
                MotivoGeneral = result.Encabezado.MotivoGeneral,
                ComentarioResolucion = result.Encabezado.ComentarioResolucion,
                EstadoID = result.Encabezado.EstadoId,
                EstadoDescripcion = result.Encabezado.EstadoDescripcion,
                FechaCreacion = result.Encabezado.FechaCreacion,
                CantidadDetalles = result.Detalles.Count,
                AprobadorID = result.Encabezado.AprobadorId,
                FechaAprobacion = result.Encabezado.FechaAprobacion
            },
            Solicitante = new UsuarioResumenResponse
            {
                UsuarioID = result.Solicitante.UsuarioId,
                NombreCompleto = result.Solicitante.NombreCompleto,
                Cedula = result.Solicitante.Cedula,
                Correo = result.Solicitante.Correo,
                Compania = result.Solicitante.Compania,
                UnidadID = result.Solicitante.UnidadId,
                JefaturaID = result.Solicitante.JefaturaId
            },
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
                    JefaturaID = result.Aprobador.JefaturaId
                },
            Detalles = result.Detalles.Select(d => new JustificacionDetalleLineaResponse
            {
                DetalleID = d.DetalleID,
                TipoJustificacionID = d.TipoJustificacionID,
                TipoJustificacionDescripcion = d.TipoJustificacionDescripcion,
                FechaMarca = d.FechaMarca,
                ObservacionDetalle = d.ObservacionDetalle
            }).ToList()
        });
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
