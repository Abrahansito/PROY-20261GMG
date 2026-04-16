using Microsoft.EntityFrameworkCore;
using SGMG.Data;
using SGMG.Models;

namespace SGMG.Repository.RepositoryImpl
{
    public class OrdenLaboratorioRepositoryImpl : IOrdenLaboratorioRepository
    {
        private readonly ApplicationDbContext _context;

        public OrdenLaboratorioRepositoryImpl(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrdenLaboratorio>> GetOrdenesByPacienteAsync(int idPaciente)
        {
            return await _context.OrdenesLaboratorio
                .Include(o => o.Paciente)
                .Include(o => o.Medico)
                .Where(o => o.IdPaciente == idPaciente)
                .OrderByDescending(o => o.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<OrdenLaboratorio?> GetOrdenByIdAsync(int idOrden)
        {
            return await _context.OrdenesLaboratorio
                .Include(o => o.Paciente)
                .Include(o => o.Medico)
                .FirstOrDefaultAsync(o => o.IdOrden == idOrden);
        }

        public async Task AddOrdenAsync(OrdenLaboratorio orden)
        {
            orden.FechaSolicitud = DateTime.UtcNow;
            await _context.OrdenesLaboratorio.AddAsync(orden);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrdenAsync(OrdenLaboratorio orden)
        {
            _context.OrdenesLaboratorio.Update(orden);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GenerarNumeroOrdenAsync()
        {
            var ultimaOrden = await _context.OrdenesLaboratorio
                .OrderByDescending(o => o.IdOrden)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;
            if (ultimaOrden != null)
            {
                var numero = ultimaOrden.NumeroOrden.Replace("#ORD", "");
                if (int.TryParse(numero, out int num))
                {
                    siguienteNumero = num + 1;
                }
            }

            return $"#ORD{siguienteNumero:D3}";
        }
    }
}