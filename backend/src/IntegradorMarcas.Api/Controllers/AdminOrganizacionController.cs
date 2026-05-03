using IntegradorMarcas.Api.Contracts.Requests;
using IntegradorMarcas.Api.Contracts.Responses;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntegradorMarcas.Api.Controllers;

[ApiController]
[Route("api/admin/organizacion")]
public sealed class AdminOrganizacionController : ControllerBase
{
    private readonly IAdminOrganizacionService _service;
    private readonly IUserContext _userContext;

    public AdminOrganizacionController(IAdminOrganizacionService service, IUserContext userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    [HttpGet("dependencias")]
    public async Task<ActionResult<IReadOnlyList<AdminDependenciaResponse>>> ListDependencias(
        [FromQuery] int? estadoRegistroId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.ListDependenciasAsync(user, estadoRegistroId, search, cancellationToken);

        return Ok(result.Select(MapDependenciaResponse).ToList());
    }

    [HttpPatch("dependencias/{estructuraOrganizacionalId:int}")]
    public async Task<ActionResult<AdminDependenciaResponse>> UpdateDependencia(
        int estructuraOrganizacionalId,
        [FromBody] UpdateDependenciaRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var correlationId = HttpContext.TraceIdentifier;
        var updated = await _service.UpdateDependenciaAsync(user, estructuraOrganizacionalId, new UpdateDependenciaDto
        {
            Nombre = request.Nombre,
            EstructuraPadreId = request.EstructuraPadreID,
            EstadoRegistroId = request.EstadoRegistroID
        }, correlationId, cancellationToken);

        return Ok(MapDependenciaResponse(updated));
    }

    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<AdminUsuarioAsignacionResponse>>> ListUsuarios(
        [FromQuery] int? rolId,
        [FromQuery] int? unidadId,
        [FromQuery] int? jefaturaId,
        [FromQuery] bool? esActivo,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var result = await _service.ListUsuariosAsync(user, rolId, unidadId, jefaturaId, esActivo, search, cancellationToken);

        return Ok(result.Select(MapUsuarioResponse).ToList());
    }

    [HttpPatch("usuarios/{usuarioId:int}/asignacion")]
    public async Task<ActionResult<AdminUsuarioAsignacionResponse>> UpdateUsuarioAsignacion(
        int usuarioId,
        [FromBody] UpdateUsuarioAsignacionRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var correlationId = HttpContext.TraceIdentifier;
        var updated = await _service.UpdateUsuarioAsignacionAsync(user, usuarioId, new UpdateUsuarioAsignacionDto
        {
            RolId = request.RolID,
            UnidadId = request.UnidadID,
            JefaturaId = request.JefaturaID
        }, correlationId, cancellationToken);

        return Ok(MapUsuarioResponse(updated));
    }

    [HttpPatch("usuarios/{usuarioId:int}/estado")]
    public async Task<ActionResult<AdminUsuarioAsignacionResponse>> UpdateUsuarioEstado(
        int usuarioId,
        [FromBody] UpdateUsuarioEstadoRequest request,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        var correlationId = HttpContext.TraceIdentifier;
        var updated = await _service.UpdateUsuarioEstadoAsync(user, usuarioId, new UpdateUsuarioEstadoDto
        {
            EsActivo = request.EsActivo
        }, correlationId, cancellationToken);

        return Ok(MapUsuarioResponse(updated));
    }

    private static AdminDependenciaResponse MapDependenciaResponse(AdminDependenciaDto x)
    {
        return new AdminDependenciaResponse
        {
            EstructuraOrganizacionalID = x.EstructuraOrganizacionalId,
            Nombre = x.Nombre,
            CodigoOrigen = x.CodigoOrigen,
            EstructuraPadreID = x.EstructuraPadreId,
            EstadoRegistroID = x.EstadoRegistroId,
            VigenciaDesde = x.VigenciaDesde,
            VigenciaHasta = x.VigenciaHasta
        };
    }

    private static AdminUsuarioAsignacionResponse MapUsuarioResponse(AdminUsuarioAsignacionDto x)
    {
        return new AdminUsuarioAsignacionResponse
        {
            UsuarioID = x.UsuarioId,
            Cedula = x.Cedula,
            NombreCompleto = x.NombreCompleto,
            CorreoElectronico = x.CorreoElectronico,
            JefaturaID = x.JefaturaId,
            JefaturaNombre = x.JefaturaNombre,
            UnidadID = x.UnidadId,
            RolID = x.RolId,
            RolDescripcion = x.RolDescripcion,
            EsActivo = x.EsActivo
        };
    }
}
