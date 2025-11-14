using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Models;
using LogisControlAPI.Data;

namespace LogisControlAPI.Services
{
    public class UtilizadorService
    {

        private readonly LogisControlContext _context;
        private readonly PasswordHasher<string> _passwordHasher;

        public UtilizadorService(LogisControlContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<string>();
        }

        //Verifica se um número de funcionário já existe
        public async Task<bool> VerificarSeExisteNumeroFuncionario(int numFuncionario)
        {
            return await _context.Utilizadores.AnyAsync(u => u.NumFuncionario == numFuncionario);
        }

        //Gera um hash seguro da pass
        public string HashPassword(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
                throw new ArgumentException("A password não pode ser vazia ou nula.");
            return _passwordHasher.HashPassword(null, senha);
        }

        public bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            if (string.IsNullOrWhiteSpace(inputPassword))
                return false;

            return _passwordHasher.VerifyHashedPassword(null, hashedPassword, inputPassword) == PasswordVerificationResult.Success;
        }

        public async Task<bool> ResetPasswordAsync(int numFuncionario, string novaPassword)
        {
            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.NumFuncionario == numFuncionario);

            if (utilizador == null)
                return false;

            utilizador.Password = HashPassword(novaPassword);
            _context.Utilizadores.Update(utilizador);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}