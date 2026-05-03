using IntegradorMarcas.Api.Contracts.Requests;
using IntegradorMarcas.Api.Contracts.Responses;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntegradorMarcas.Api.Controllers;

[ApiController]
[Route("api/admin/aprobaciones")]
public sealed class AdminAprobacionesController : ControllerBase
{
    private readonly IAdminAprobacionesService _service;
    private readonly IUserContext _userContext;

    public AdminAprobacionesController(IAdminAprobacionesService service, IUserContext userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    /// <summary>
    /// Lista jerarquias de aprobacion con filtros administrativos opcionales.
    /// </summary>
    [HttpGet("jerarquias")]
    public async Task<ActionResult<IReadOnlyList<AdminJerarquiaResponse>>> ListJerarquias(
        [FromQuery] int? aprobadorUsuarioId,
        [FromQuery] int? estadoRegistroId,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.ListJerarquiasAsync(user, aprobadorUsuarioId, estadoRegistroId, cancellationToken);

        return Ok(result.Select(x => new AdminJerarquiaResponse
        {
            JerarquiaAprobacionID = x.JerarquiaAprobacionId,
            AprobadorUsuarioID = x.AprobadorUsuarioId,
            EstructuraOrganizacionalID = x.EstructuraOrganizacionalId,
            NivelAprobacion = x.NivelAprobacion,
            TipoRelacion = x.TipoRelacion,
            EstadoRegistroID = x.EstadoRegistroId,
            VigenciaDesde = x.VigenciaDesde,
            VigenciaHasta = x.VigenciaHasta
        }).ToList());
    }

    /// <summary>
    /// Crea una jerarquia de aprobacion.
    /// </summary>
    /// <remarks>
    /// Las reglas de consistencia (vigencia, duplicidad y permisos) se validan en la capa de servicio.
    /// </remarks>
    [HttpPost("jerarquias")]
    public async Task<ActionResult<AdminJerarquiaResponse>> CreateJerarquia(
        [FromBody] CreateJerarquiaRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var correlationId = HttpContext.TraceIdentifier;
        var created = await _service.CreateJerarquiaAsync(user, new CreateJerarquiaDto
        {
            AprobadorUsuarioId = request.AprobadorUsuarioID,
            EstructuraOrganizacionalId = request.EstructuraOrganizacionalID,
            NivelAprobacion = request.NivelAprobacion,
            TipoRelacion = request.TipoRelacion,
            VigenciaDesde = request.VigenciaDesde,
            VigenciaHasta = request.VigenciaHasta
        }, correlationId, cancellationToken);

        return Ok(new AdminJerarquiaResponse
        {
            JerarquiaAprobacionID = created.JerarquiaAprobacionId,
            AprobadorUsuarioID = created.AprobadorUsuarioId,
            EstructuraOrganizacionalID = created.EstructuraOrganizacionalId,
            NivelAprobacion = created.NivelAprobacion,
            TipoRelacion = created.TipoRelacion,
            EstadoRegistroID = created.EstadoRegistroId,
            VigenciaDesde = created.VigenciaDesde,
            VigenciaHasta = created.VigenciaHasta
        });
    }

    /// <summary>
    /// Actualiza una jerarquia de aprobacion existente.
    /// </summary>
    [HttpPatch("jerarquias/{jerarquiaAprobacionId:int}")]
    public async Task<ActionResult<AdminJerarquiaResponse>> UpdateJerarquia(
        int jerarquiaAprobacionId,
        [FromBody] UpdateJerarquiaRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var correlationId = HttpContext.TraceIdentifier;
        var updated = await _service.UpdateJerarquiaAsync(user, jerarquiaAprobacionId, new UpdateJerarquiaDto
        {
            AprobadorUsuarioId = request.AprobadorUsuarioID,
            EstructuraOrganizacionalId = request.EstructuraOrganizacionalID,
            NivelAprobacion = request.NivelAprobacion,
            TipoRelacion = request.TipoRelacion,
            EstadoRegistroId = request.EstadoRegistroID,
            VigenciaDesde = request.VigenciaDesde,
            VigenciaHasta = request.VigenciaHasta
        }, correlationId, cancellationToken);

        return Ok(new AdminJerarquiaResponse
        {
            JerarquiaAprobacionID = updated.JerarquiaAprobacionId,
            AprobadorUsuarioID = updated.AprobadorUsuarioId,
            EstructuraOrganizacionalID = updated.EstructuraOrganizacionalId,
            NivelAprobacion = updated.NivelAprobacion,
            TipoRelacion = updated.TipoRelacion,
            EstadoRegistroID = updated.EstadoRegistroId,
            VigenciaDesde = updated.VigenciaDesde,
            VigenciaHasta = updated.VigenciaHasta
        });
    }

    /// <summary>
    /// Cambia el estado de registro de una jerarquia de aprobacion.
    /// </summary>
    [HttpPatch("jerarquias/{jerarquiaAprobacionId:int}/estado")]
    public async Task<ActionResult> ToggleJerarquiaEstado(
        int jerarquiaAprobacionId,
        [FromBody] ToggleEstadoRegistroRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var correlationId = HttpContext.TraceIdentifier;
        await _service.ToggleJerarquiaEstadoAsync(user, jerarquiaAprobacionId, new ToggleEstadoRegistroDto
        {
            EstadoRegistroId = request.EstadoRegistroID
        }, correlationId, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Lista delegaciones de aprobacion con filtros por delegante, delegado, estado y fecha de vigencia.
    /// </summary>
    [HttpGet("delegaciones")]
    public async Task<ActionResult<IReadOnlyList<AdminDelegacionResponse>>> ListDelegaciones(
        [FromQuery] int? deleganteUsuarioId,
        [FromQuery] int? delegadoUsuarioId,
        [FromQuery] int? estadoRegistroId,
        [FromQuery] DateTime? vigenteEnFecha,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.ListDelegacionesAsync(
            user,
            deleganteUsuarioId,
            delegadoUsuarioId,
            estadoRegistroId,
            vigenteEnFecha,
            cancellationToken);

        return Ok(result.Select(x => new AdminDelegacionResponse
        {
            DelegacionAprobacionID = x.DelegacionAprobacionId,
            DeleganteUsuarioID = x.DeleganteUsuarioId,
            DelegadoUsuarioID = x.DelegadoUsuarioId,
            JerarquiaAprobacionID = x.JerarquiaAprobacionId,
            Motivo = x.Motivo,
            EstadoRegistroID = x.EstadoRegistroId,
            VigenciaDesde = x.VigenciaDesde,
            VigenciaHasta = x.VigenciaHasta
        }).ToList());
    }

    /// <summary>
    /// Crea una delegacion de aprobacion.
    /// </summary>
    /// <remarks>
    /// Las validaciones de vigencia y consistencia referencial se ejecutan en la capa de servicio.
    /// </remarks>
    [HttpPost("delegaciones")]
    public async Task<ActionResult<AdminDelegacionResponse>> CreateDelegacion(
        [FromBody] CreateDelegacionRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var correlationId = HttpContext.TraceIdentifier;
        var created = await _service.CreateDelegacionAsync(user, new CreateDelegacionDto
        {
            DeleganteUsuarioId = request.DeleganteUsuarioID,
            DelegadoUsuarioId = request.DelegadoUsuarioID,
            JerarquiaAprobacionId = request.JerarquiaAprobacionID,
            Motivo = request.Motivo,
            VigenciaDesde = request.VigenciaDesde,
            VigenciaHasta = request.VigenciaHasta
        }, correlationId, cancellationToken);

        return Ok(new AdminDelegacionResponse
        {
            DelegacionAprobacionID = created.DelegacionAprobacionId,
            DeleganteUsuarioID = created.DeleganteUsuarioId,
            DelegadoUsuarioID = created.DelegadoUsuarioId,
            JerarquiaAprobacionID = created.JerarquiaAprobacionId,
            Motivo = created.Motivo,
            EstadoRegistroID = created.EstadoRegistroId,
            VigenciaDesde = created.VigenciaDesde,
            VigenciaHasta = created.VigenciaHasta
        });
    }

    /// <summary>
    /// Actualiza una delegacion de aprobacion existente.
    /// </summary>
    [HttpPatch("delegaciones/{delegacionAprobacionId:int}")]
    public async Task<ActionResult<AdminDelegacionResponse>> UpdateDelegacion(
        int delegacionAprobacionId,
        [FromBody] UpdateDelegacionRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var correlationId = HttpContext.TraceIdentifier;
        var updated = await _service.UpdateDelegacionAsync(user, delegacionAprobacionId, new UpdateDelegacionDto
        {
            DeleganteUsuarioId = request.DeleganteUsuarioID,
            DelegadoUsuarioId = request.DelegadoUsuarioID,
            JerarquiaAprobacionId = request.JerarquiaAprobacionID,
            Motivo = request.Motivo,
            EstadoRegistroId = request.EstadoRegistroID,
            VigenciaDesde = request.VigenciaDesde,
            VigenciaHasta = request.VigenciaHasta
        }, correlationId, cancellationToken);

        return Ok(new AdminDelegacionResponse
        {
            DelegacionAprobacionID = updated.DelegacionAprobacionId,
            DeleganteUsuarioID = updated.DeleganteUsuarioId,
            DelegadoUsuarioID = updated.DelegadoUsuarioId,
            JerarquiaAprobacionID = updated.JerarquiaAprobacionId,
            Motivo = updated.Motivo,
            EstadoRegistroID = updated.EstadoRegistroId,
            VigenciaDesde = updated.VigenciaDesde,
            VigenciaHasta = updated.VigenciaHasta
        });
    }

    /// <summary>
    /// Cambia el estado de registro de una delegacion de aprobacion.
    /// </summary>
    [HttpPatch("delegaciones/{delegacionAprobacionId:int}/estado")]
    public async Task<ActionResult> ToggleDelegacionEstado(
        int delegacionAprobacionId,
        [FromBody] ToggleEstadoRegistroRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var correlationId = HttpContext.TraceIdentifier;
        await _service.ToggleDelegacionEstadoAsync(user, delegacionAprobacionId, new ToggleEstadoRegistroDto
        {
            EstadoRegistroId = request.EstadoRegistroID
        }, correlationId, cancellationToken);

        return NoContent();
    }
}
