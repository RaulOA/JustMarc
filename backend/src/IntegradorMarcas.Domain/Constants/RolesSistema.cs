namespace IntegradorMarcas.Domain.Constants;

public static class RolesSistema
{
    public const string RolFunc = "ROL_FUNC";
    public const string RolJefe = "ROL_JEFE";
    public const string RolRrhh = "ROL_RRHH";

    public static bool EsFuncionario(string? rol)
    {
        var normalized = (rol ?? string.Empty).Trim().ToUpperInvariant();
        return normalized is RolFunc or "FUNCIONARIO" or "1";
    }

    public static bool EsJefatura(string? rol)
    {
        var normalized = (rol ?? string.Empty).Trim().ToUpperInvariant();
        return normalized is RolJefe or "JEFATURA" or "2";
    }

    public static bool EsRrhh(string? rol)
    {
        var normalized = (rol ?? string.Empty).Trim().ToUpperInvariant();
        return normalized is RolRrhh or "RRHH" or "3";
    }
}
