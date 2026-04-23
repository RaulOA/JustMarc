using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Validation;

public static class JustificacionValidator
{
    private static readonly HashSet<string> CompaniasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "CNP",
        "FANAL"
    };

    public static void ValidateCreate(CreateJustificacionDto request)
    {
        if (string.IsNullOrWhiteSpace(request.MotivoGeneral) || request.MotivoGeneral.Length > 500)
        {
            throw new AppException("MotivoGeneral es requerido y no puede exceder 500 caracteres.", 400);
        }

        if (request.Detalles is null || request.Detalles.Count < 1)
        {
            throw new AppException("RN-01: una boleta debe incluir al menos una línea de detalle.", 400);
        }

        foreach (var detalle in request.Detalles)
        {
            if (detalle.TipoJustificacionId <= 0)
            {
                throw new AppException("TipoJustificacionID es requerido.", 400);
            }

            if (detalle.FechaMarca == default)
            {
                throw new AppException("FechaMarca es requerida.", 400);
            }

            if (!string.IsNullOrWhiteSpace(detalle.ObservacionDetalle) && detalle.ObservacionDetalle.Length > 250)
            {
                throw new AppException("ObservacionDetalle no puede exceder 250 caracteres.", 400);
            }
        }
    }

    public static string ValidateAccion(string accion)
    {
        var normalized = accion.Trim().ToUpperInvariant();
        if (normalized is not "APROBAR" and not "RECHAZAR")
        {
            throw new AppException("Accion debe ser APROBAR o RECHAZAR.", 400);
        }

        return normalized;
    }

    public static string? NormalizeComentarioResolucion(string? comentario)
    {
        if (comentario is null)
        {
            return null;
        }

        var normalized = comentario.Trim();
        if (normalized.Length == 0)
        {
            return null;
        }

        if (normalized.Length > 500)
        {
            throw new AppException("Comentario no puede exceder 500 caracteres.", 400);
        }

        return normalized;
    }

    public static void ValidateRangoFechas(DateTime? desde, DateTime? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
        {
            throw new AppException("El rango de fechas es inválido: Desde no puede ser mayor que Hasta.", 400);
        }
    }

    public static void ValidateCompania(string? compania)
    {
        if (string.IsNullOrWhiteSpace(compania))
        {
            return;
        }

        if (!CompaniasPermitidas.Contains(compania.Trim()))
        {
            throw new AppException("Compania inválida. Valores permitidos: CNP o FANAL.", 400);
        }
    }

    public static void ValidateTextoBusqueda(string? funcionario)
    {
        if (!string.IsNullOrWhiteSpace(funcionario) && funcionario.Trim().Length > 150)
        {
            throw new AppException("El texto de búsqueda de funcionario no puede exceder 150 caracteres.", 400);
        }
    }
}
