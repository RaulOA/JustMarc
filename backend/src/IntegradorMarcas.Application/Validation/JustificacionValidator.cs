using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Validation;

public static class JustificacionValidator
{
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
}
