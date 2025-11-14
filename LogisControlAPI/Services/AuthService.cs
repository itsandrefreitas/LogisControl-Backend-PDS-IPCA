using LogisControlAPI.Auxiliar;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LogisControlAPI.Services
{
    public class AuthService
    {
        public string GenerateToken(int utilizadorId,int numFuncionario, string role)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(AuthSettings.PrivateKey);//class AuthSettings criada
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature);    //algoritmo HMAC-SHA256 

            //Inf para criar o JWT: Subject, Expires, SigningCredentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = GenerateClaims(utilizadorId,numFuncionario, role),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = credentials,
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        /// <summary>
        /// Define Claims (Profiles)
        /// Each role is added as a separate declaration of type ClaimTypes.Role
        /// </summary>
        /// <param name="numFuncionario"></param>
        /// <param name="role></param>
        /// <returns></returns>
        private static ClaimsIdentity GenerateClaims(int utilizadorId, int numFuncionario, string role)
        {
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim("id", utilizadorId.ToString())); // ID do utilizador
            claims.AddClaim(new Claim(ClaimTypes.SerialNumber, numFuncionario.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.Role, role));

            return claims;
        }
    }
}
