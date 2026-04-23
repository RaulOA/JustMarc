using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IAuditEventRepository
{
    Task LogEventAsync(AuditEventEntry entry, CancellationToken cancellationToken);
}
