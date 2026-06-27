using Microsoft.EntityFrameworkCore;
using biblioteca_chavarri_ferrer.Data;

namespace biblioteca_chavarri_ferrer.Services;

public class ReporteService
{
    private readonly BibliotecaContext _context;

    public ReporteService(BibliotecaContext context)
    {
        _context = context;
    }

}
