using System.Data;

namespace IntegradorMarcas.Infrastructure.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
