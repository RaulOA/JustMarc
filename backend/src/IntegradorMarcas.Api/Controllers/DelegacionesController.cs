using IntegradorMarcas.Api.Contracts.Responses;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntegradorMarcas.Api.Controllers;

/// <summary>
/// F-004: vistas del delegado (mi-funcion R11/R12, mi-registro R16/R17/R18).
/// Solo lectura para ROL_JEFE que actua como delegado.
/// R18 se garantiza por ausencia de rutas de mutacion en este controller.
/// </summary>
[ApiController]
[Route("api/delegaciones")]
public sealed class DelegacionesController : ControllerBase
{
    private readonly IDelegacionConsultaService _service;
    private readonly IUserContext _userContext;

    public DelegacionesController(IDelegacionConsultaService service, IUserContext userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    /// <summary>
    /// Devuelve las delegaciones activas y vigentes recibidas por el usuario autenticado (ROL_JEFE).
    /// Incluye quien la asigno (titular), la vigencia del permiso y el alcance de estructuras (R11/R12).
    /// </summary>
    [HttpGet("mi-funcion")]
    public async Task<ActionResult<IReadOnlyList<DelegacionFuncionResponse>>> GetMiFuncion(CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.GetMiFuncionAsync(user, cancellationToken);

        return Ok(result.Select(x => new DelegacionFuncionResponse
        {
            DelegacionAprobacionID = x.DelegacionAprobacionId,
            TitularUsuarioID = x.TitularUsuarioId,
            TitularNombre = x.TitularNombre,
            VigenciaDesde = x.VigenciaDesde,
            VigenciaHasta = x.VigenciaHasta,
            Motivo = x.Motivo,
            AlcanceEstructuras = x.AlcanceEstructuras,
            JerarquiaAprobacionID = x.JerarquiaAprobacionId
        }).ToList());
    }

    /// <summary>
    /// Devuelve el registro de solo lectura de justificaciones tramitadas como delegado
    /// dentro del periodo de la delegacion correspondiente (R16/R17).
    /// R18: no existe ruta de mutacion en este controller.
    /// </summary>
    [HttpGet("mi-registro")]
    public async Task<ActionResult<IReadOnlyList<DelegacionRegistroResponse>>> GetMiRegistro(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.GetMiRegistroAsync(user, new FiltroJustificacionesDto
        {
            Desde = desde,
            Hasta = hasta
        }, cancellationToken);

        return Ok(result.Select(x => new DelegacionRegistroResponse
        {
            JustificacionID = x.JustificacionId,
            MotivoGeneral = x.MotivoGeneral,
            ComentarioResolucion = x.ComentarioResolucion,
            EstadoID = x.EstadoId,
            EstadoDescripcion = x.EstadoDescripcion,
            FechaCreacion = x.FechaCreacion,
            FechaAprobacion = x.FechaAprobacion,
            SolicitanteUsuarioID = x.SolicitanteUsuarioId,
            SolicitanteNombre = x.SolicitanteNombre,
            DelegacionAprobacionID = x.DelegacionAprobacionId,
            TitularUsuarioID = x.TitularUsuarioId,
            TitularNombre = x.TitularNombre
        }).ToList());
    }
}
