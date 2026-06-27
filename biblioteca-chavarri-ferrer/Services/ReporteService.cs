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


    public void DisponibilidadLibro()
    {
        Console.WriteLine("=== DISPONIBILIDAD DE UN LIBRO ===\n");

        Console.Write("Ingresá ISBN o título del libro: ");
        string busqueda = Console.ReadLine()?.Trim() ?? "";

        var libro = _context.Libros
            .FirstOrDefault(l => l.ISBN == busqueda ||
                                 l.Titulo.ToLower().Contains(busqueda.ToLower()));

        if (libro == null)
        {
            Console.WriteLine("Libro no encontrado.");
            return;
        }


        int copiasTomadas = _context.Prestamos
            .Count(p => p.LibroISBN == libro.ISBN &&
                        (p.EstadoPrestamo.Nombre == "Activo" || p.EstadoPrestamo.Nombre == "Vencido"));

        int copiasDisponibles = libro.CantidadCopias - copiasTomadas;


        var reservasPendientes = _context.Reservas
            .Include(r => r.Socio)
            .Where(r => r.LibroISBN == libro.ISBN && r.EstadoReserva.Nombre == "Pendiente")
            .OrderBy(r => r.FechaReserva)
            .ToList();

        Console.WriteLine($"\n  Título:   {libro.Titulo}");
        Console.WriteLine($"  Autor:    {libro.Autor}");
        Console.WriteLine($"  ISBN:     {libro.ISBN}");
        Console.WriteLine($"  Copias totales:     {libro.CantidadCopias}");
        Console.WriteLine($"  Copias prestadas:   {copiasTomadas}");
        Console.WriteLine($"  Copias disponibles: {copiasDisponibles}");

        if (reservasPendientes.Any())
        {
            Console.WriteLine($"\n  Reservas pendientes ({reservasPendientes.Count}):");
            foreach (var r in reservasPendientes)
            {
                Console.WriteLine($"    • {r.Socio.Nombre} {r.Socio.Apellido} — reservó el {r.FechaReserva:dd/MM/yyyy}");
            }
        }
        else
        {
            Console.WriteLine("\n  Sin reservas pendientes.");
        }

        Console.WriteLine();
    }


    public void HistorialSocio()
    {
        Console.WriteLine("=== HISTORIAL DE UN SOCIO ===\n");

        Console.Write("Ingresá el número de socio: ");
        if (!int.TryParse(Console.ReadLine(), out int nroSocio))
        {
            Console.WriteLine("Número de socio inválido.");
            return;
        }

        var socio = _context.Socios
            .Include(s => s.TipoSocio)
            .FirstOrDefault(s => s.NroSocio == nroSocio);

        if (socio == null)
        {
            Console.WriteLine("Socio no encontrado.");
            return;
        }

        Console.WriteLine($"\nSocio: {socio.Nombre} {socio.Apellido} — Tipo: {socio.TipoSocio.Nombre}");

        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.EstadoPrestamo)
            .Where(p => p.SocioId == nroSocio)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToList();

        Console.WriteLine($"\n--- Préstamos ({prestamos.Count}) ---");
        if (!prestamos.Any())
        {
            Console.WriteLine("  Sin préstamos.");
        }
        else
        {
            foreach (var p in prestamos)
            {
                string devolucion = p.FechaDevolucion.HasValue
                    ? $"Devuelto: {p.FechaDevolucion:dd/MM/yyyy}"
                    : $"Vence: {p.FechaVencimiento:dd/MM/yyyy}";
                string multa = p.Multa.HasValue ? $" | Multa: ${p.Multa:F2}" : "";
                Console.WriteLine($"  • [{p.EstadoPrestamo.Nombre}] {p.Libro.Titulo}");
                Console.WriteLine($"    Prestado: {p.FechaPrestamo:dd/MM/yyyy} — {devolucion}{multa}");
            }
        }

        var reservas = _context.Reservas
            .Include(r => r.Libro)
            .Include(r => r.EstadoReserva)
            .Where(r => r.SocioId == nroSocio)
            .OrderByDescending(r => r.FechaReserva)
            .ToList();

        Console.WriteLine($"\n--- Reservas ({reservas.Count}) ---");
        if (!reservas.Any())
        {
            Console.WriteLine("  Sin reservas.");
        }
        else
        {
            foreach (var r in reservas)
            {
                Console.WriteLine($"  • [{r.EstadoReserva.Nombre}] {r.Libro.Titulo} — {r.FechaReserva:dd/MM/yyyy}");
            }
        }

        Console.WriteLine();
    }
}
