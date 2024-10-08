dotnet new sln -n OdontoPredict
dotnet new classlib -n OdontoPredict.Domain
dotnet new classlib -n OdontoPredict.Application
dotnet new classlib -n OdontoPredict.Infrastructure
dotnet new webapi -n OdontoPredict.API
dotnet sln add OdontoPredict.Domain/OdontoPredict.Domain.csproj
dotnet sln add OdontoPredict.Application/OdontoPredict.Application.csproj
dotnet sln add OdontoPredict.Infrastructure/OdontoPredict.Infrastructure.csproj
dotnet sln add OdontoPredict.API/OdontoPredict.API.csproj
dotnet add OdontoPredict.Application/OdontoPredict.Application.csproj reference OdontoPredict.Domain/OdontoPredict.Domain.csproj
dotnet add OdontoPredict.Infrastructure/OdontoPredict.Infrastructure.csproj reference OdontoPredict.Domain/OdontoPredict.Domain.csproj




namespace OdontoPredict.Domain.Entities
{
    public class Paciente
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string CPF { get; set; }
        public DateTime DataNascimento { get; set; }
        public List<Consulta> Consultas { get; set; }
    }

    public class Consulta
    {
        public Guid Id { get; set; }
        public Guid PacienteId { get; set; }
        public Paciente Paciente { get; set; }
        public DateTime DataConsulta { get; set; }
        public string Observacoes { get; set; }
        public List<Tratamento> Tratamentos { get; set; }
    }

    public class Tratamento
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Custo { get; set; }
    }
}




namespace OdontoPredict.Domain.Interfaces
{
    public interface IRepositorioPaciente
    {
        Task<Paciente> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Paciente>> ObterTodosAsync();
        Task AdicionarAsync(Paciente paciente);
        Task AtualizarAsync(Paciente paciente);
        Task RemoverAsync(Guid id);
    }

    public interface IRepositorioConsulta
    {
        Task<Consulta> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Consulta>> ObterPorPacienteAsync(Guid pacienteId);
        Task AdicionarAsync(Consulta consulta);
        Task AtualizarAsync(Consulta consulta);
        Task RemoverAsync(Guid id);
    }
}



namespace OdontoPredict.Domain.Services
{
    public class PacienteService
    {
        public bool PodeReceberAlerta(Paciente paciente)
        {
            // Lógica de negócio para verificar se o paciente deve receber um alerta
            return paciente.Consultas.Count >= 5; // Exemplo simples
        }
    }
}




namespace OdontoPredict.Application.Services
{
    public class PacienteServiceApp
    {
        private readonly IRepositorioPaciente _repositorioPaciente;

        public PacienteServiceApp(IRepositorioPaciente repositorioPaciente)
        {
            _repositorioPaciente = repositorioPaciente;
        }

        public async Task<PacienteDto> ObterPacientePorIdAsync(Guid id)
        {
            var paciente = await _repositorioPaciente.ObterPorIdAsync(id);
            return new PacienteDto
            {
                Id = paciente.Id,
                Nome = paciente.Nome,
                CPF = paciente.CPF
            };
        }

        // Outros casos de uso
    }
}





namespace OdontoPredict.Application.DTOs
{
    public class PacienteDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string CPF { get; set; }
    }
}





using Microsoft.EntityFrameworkCore;
using OdontoPredict.Domain.Entities;

namespace OdontoPredict.Infrastructure.Data
{
    public class OdontoPredictContext : DbContext
    {
        public OdontoPredictContext(DbContextOptions<OdontoPredictContext> options) : base(options) {}

        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Tratamento> Tratamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapeamentos personalizados
            modelBuilder.Entity<Paciente>().ToTable("Pacientes");
            modelBuilder.Entity<Consulta>().ToTable("Consultas");
            modelBuilder.Entity<Tratamento>().ToTable("Tratamentos");
        }
    }
}




namespace OdontoPredict.Infrastructure.Repositories
{
    public class RepositorioPaciente : IRepositorioPaciente
    {
        private readonly OdontoPredictContext _context;

        public RepositorioPaciente(OdontoPredictContext context)
        {
            _context = context;
        }

        public async Task<Paciente> ObterPorIdAsync(Guid id)
        {
            return await _context.Pacientes.FindAsync(id);
        }

        public async Task<IEnumerable<Paciente>> ObterTodosAsync()
        {
            return await _context.Pacientes.ToListAsync();
        }

        public async Task AdicionarAsync(Paciente paciente)
        {
            await _context.Pacientes.AddAsync(paciente);
            await _context.SaveChangesAsync();
        }

        // Outros métodos CRUD
    }
}




public class ErroHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErroHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            // Lidar com exceções e retornar erro apropriado
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsync(new { Message = "Ocorreu um erro inesperado." }.ToString());
    }
}