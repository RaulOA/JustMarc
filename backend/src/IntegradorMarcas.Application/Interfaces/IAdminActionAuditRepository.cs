using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IAdminActionAuditRepository
{
    Task LogActionAsync(AdminActionAuditEntry entry, CancellationToken cancellationToken);
}
