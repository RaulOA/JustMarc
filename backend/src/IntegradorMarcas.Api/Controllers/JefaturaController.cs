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

    /// <summary>
    /// Lista justificaciones pendientes visibles para la jefatura autenticada.
    /// </summary>
    /// <param name="desde">Fecha inicial opcional para filtrar por rango de creacion.</param>
    /// <param name="hasta">Fecha final opcional para filtrar por rango de creacion.</param>
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

    /// <summary>
    /// Obtiene el detalle completo de una justificacion accesible por la jefatura actual.
    /// </summary>
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
                JefaturaID = result.Solicitante.JefaturaId,
                UnidadNombre = result.Solicitante.UnidadNombre
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

    /// <summary>
    /// Resuelve una justificacion mediante la accion indicada por la jefatura.
    /// </summary>
    /// <remarks>
    /// La validacion de accion/comentario y las transiciones de estado se aplican en la capa de servicio.
    /// </remarks>
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

    /// <summary>
    /// Re-resolucion del titular sobre una justificacion resuelta por su delegado (R15, D2).
    /// Solo jefatura con alcance por jerarquia directa puede invocar este endpoint.
    /// </summary>
    /// <remarks>
    /// Endpoint dedicado separado de Resolver para no alterar la semantica de RN-04.
    /// Audita la re-resolucion con trazabilidad completa.
    /// </remarks>
    [HttpPatch("{justificacionId:int}/revisar-titular")]
    public async Task<ActionResult> RevisarTitular(
        int justificacionId,
        [FromBody] ResolverJustificacionRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        await _service.RevisarTitularAsync(user, justificacionId, new ResolverJustificacionDto
        {
            Accion = request.Accion,
            Comentario = request.Comentario
        }, cancellationToken);

        return NoContent();
    }
}
