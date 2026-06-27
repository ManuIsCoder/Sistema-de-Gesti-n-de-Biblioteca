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

    public void LibrosMasPrestados()
    {
        Console.WriteLine("=== TOP 5 LIBROS MÁS PRESTADOS ===\n");

        var top5 = _context.Libros
            .Select(l => new
            {
                l.Titulo,
                l.Autor,
                l.Genero,
                TotalPrestamos = l.Prestamos.Count()
            })
            .OrderByDescending(l => l.TotalPrestamos)
            .Take(5)
            .ToList();

        if (!top5.Any())
        {
            Console.WriteLine("No hay préstamos registrados.");
            return;
        }

        int posicion = 1;
        foreach (var libro in top5)
        {
            Console.WriteLine($"  {posicion}. {libro.Titulo} — {libro.Autor} [{libro.Genero}]");
            Console.WriteLine($"     Total de préstamos: {libro.TotalPrestamos}");
            posicion++;
        }

        Console.WriteLine();
    }


    public void SociosConMultasPendientes()
    {
        Console.WriteLine("=== SOCIOS CON MULTAS PENDIENTES ===\n");


        var socios = _context.Socios
            .Include(s => s.TipoSocio)
            .Where(s => s.Prestamos.Any(p => p.Multa != null && p.Multa > 0))
            .Select(s => new
            {
                s.NroSocio,
                s.Nombre,
                s.Apellido,
                TipoSocio = s.TipoSocio.Nombre,
                TotalMulta = s.Prestamos
                    .Where(p => p.Multa != null && p.Multa > 0)
                    .Sum(p => p.Multa ?? 0)
            })
            .OrderByDescending(s => s.TotalMulta)
            .ToList();

        if (!socios.Any())
        {
            Console.WriteLine("No hay socios con multas registradas.");
            return;
        }

        foreach (var s in socios)
        {
            Console.WriteLine($"  Socio Nro {s.NroSocio} — {s.Nombre} {s.Apellido} [{s.TipoSocio}]");
            Console.WriteLine($"  Total multas: ${s.TotalMulta:F2}");
            Console.WriteLine();
        }
    }


    public void PrestamosVencidos()
    {
        Console.WriteLine("=== PRÉSTAMOS VENCIDOS ===\n");

        DateTime hoy = DateTime.Today;

        var vencidos = _context.Prestamos
            .Include(p => p.Socio)
            .Include(p => p.Libro)
            .Where(p => p.FechaVencimiento < hoy && p.FechaDevolucion == null)
            .OrderBy(p => p.FechaVencimiento)
            .ToList();

        if (!vencidos.Any())
        {
            Console.WriteLine("No hay préstamos vencidos.");
            return;
        }

        foreach (var p in vencidos)
        {
            int diasDemora = (hoy - p.FechaVencimiento).Days;
            Console.WriteLine($"  Socio: {p.Socio.Nombre} {p.Socio.Apellido} (Nro {p.SocioId})");
            Console.WriteLine($"  Libro: {p.Libro.Titulo}");
            Console.WriteLine($"  Venció: {p.FechaVencimiento:dd/MM/yyyy} — {diasDemora} día(s) de demora");
            Console.WriteLine();
        }

        Console.WriteLine($"Total vencidos: {vencidos.Count}");
    }



}
