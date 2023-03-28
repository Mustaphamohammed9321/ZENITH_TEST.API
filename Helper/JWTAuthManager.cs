using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using RepoDb;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZENITH_TEST.CORE.Helpers;
using ZENITH_TEST.CORE.Models;

namespace ZENITH_TEST.API.Helpers
{
    public interface IJWTAuthManager
    {
        string GenerateJWT(string emailAddress, Guid roleId);
        ClaimsPrincipal GetPrincipal(string token);
        string ValidateToken(string token);
    }

    public class JWTAuthManager : IJWTAuthManager
    {
        private readonly IConfiguration _configuration;
        private readonly IAppSettingsFactory _appSettings;
        protected string _key;

        public JWTAuthManager(string key, IConfiguration configuration, IAppSettingsFactory appSettings)
        {
            _configuration = configuration;
            _appSettings = appSettings;
            _key = key;
        }

        public string GenerateJWT(string emailAddress, Guid roleId)
        {
            //ResponseClass<string> response = new ResponseClass<string>();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //get role from DB
            //using var ctx = new SqlConnection(_configuration.GetValue<string>("ConnectionStrings:ConStr"));
            using var ctx = new SqlConnection(_appSettings.GetConnectionString());

            //claim is used to add identity to JWT token
            var claims = new[]
            {
                 //new Claim(JwtRegisteredClaimNames.Sub, username),
                 new Claim(JwtRegisteredClaimNames.Email, emailAddress),
                 new Claim("role", ctx.Query<Roles>(w => w.RoleId == roleId).FirstOrDefault().Role ?? "User") ,  //get the role from the roleTable
                 //new Claim("role", )
                 new Claim("Date", DateTime.Now.ToString()),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             };

            var token = new JwtSecurityToken(_configuration["JWT:Issuer"],
              _configuration["JWT:Issuer"],
              claims,    //null original value
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var securityKey1 = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
                var credentials1 = new SigningCredentials(securityKey1, SecurityAlgorithms.HmacSha256);

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

                if (jwtToken == null) return null;
                byte[] key = Convert.FromBase64String(securityKey1.ToString());
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public string ValidateToken(string token)
        {
            string username = null;
            ClaimsPrincipal principal = GetPrincipal(token);
            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch (NullReferenceException)
            {
                return null;
            }
            Claim usernameClaim = identity.FindFirst(ClaimTypes.Name);
            username = usernameClaim.Value;
            return username;
        }



        #region Junk
        //public ResponseClass<T> Execute_Command<T>(string query, DynamicParameters sp_params)
        //{
        //    Response<T> response = new Response<T>();
        //    using (IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("default")))
        //    {
        //        if (dbConnection.State == ConnectionState.Closed)
        //            dbConnection.Open();
        //        using var transaction = dbConnection.BeginTransaction();
        //        try
        //        {
        //            response.Data = dbConnection.Query<T>(query, sp_params, commandType: CommandType.StoredProcedure, transaction: transaction).FirstOrDefault();
        //            response.code = sp_params.Get<int>("retVal"); //get output parameter value
        //            response.message = "Success";
        //            transaction.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            transaction.Rollback();
        //            response.code = 500;
        //            response.message = ex.Message;
        //        }
        //    }
        //    return response;
        //}

        //public Response<List<T>> getUserList<T>()
        //{
        //    Response<List<T>> response = new Response<List<T>>();
        //    using IDbConnection db = new SqlConnection(_configuration.GetConnectionString("default"));
        //    string query = "Select userid,username,email,[role],reg_date FROM tbl_users";
        //    response.Data = db.Query<T>(query, null, commandType: CommandType.Text).ToList();
        //    return response;
        //} 
        #endregion

    }
}
