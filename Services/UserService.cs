using Dapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password, string appId);
        User AuthenticateSGP(string user, string password);
        IEnumerable<User> GetAll();
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {
            new User { Id = 1, FirstName = "Test", LastName = "User", Username = "test", Password = "test" }
        };

        private readonly AppSettings _appSettings;

        private readonly ConnectionStrings _connectionStrings;

        public UserService(IOptions<AppSettings> appSettings, IOptions<ConnectionStrings> connectionStrings)
        {
            _appSettings = appSettings.Value;
            _connectionStrings = connectionStrings.Value;
        }

        public User Authenticate(string user, string password, string appId)
        {

            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    var passwordCript = Util.CalculateMD5Hash(("AppleStore@promo-clicks.com").ToLower(), password);
                    con.Open();
                    var query = "SP_WebPromo_CustomerAuthenticated";

                    var userInfo = con.Query<Customer>(query, new
                    {
                        user = user,
                        appId = appId,
                        passwordCript = passwordCript
                    }, commandType: CommandType.StoredProcedure).SingleOrDefault();

                    // return null if user not found
                    if (userInfo == null)
                        return null;

                    // authentication successful so generate jwt token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim("Name", userInfo.CustomerName.ToString()),
                            new Claim("Email", userInfo.Email != null ? userInfo.Email.ToString() : ""),
                            new Claim("SubjectId", userInfo.CustomerId.ToString()),
                        }),
                        Expires = DateTime.UtcNow.AddMinutes(_appSettings.TimeSession), //tempo de validade do token jwt
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    var retUser = new User()
                    {
                        Id = userInfo.CustomerId.Value,
                        FirstName = userInfo.CustomerName,
                        LastName = userInfo.LastName,
                        Username = user,
                        Password = password,
                        Token = ""
                    };
                    retUser.Token = tokenHandler.WriteToken(token);

                    return retUser.WithoutPassword();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                }
            }


        }

        public User AuthenticateSGP(string user, string password)
        {

            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_CustomerAuthenticated";

                    var userInfo = con.Query<CustomerSGP>(query, new
                    {
                        user = user,
                        password = password
                    }, commandType: CommandType.StoredProcedure).SingleOrDefault();

                    // return null if user not found
                    if (userInfo == null)
                        return null;

                    // authentication successful so generate jwt token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim("Name", userInfo.Name.ToString()),
                            new Claim("Email", userInfo.Email != null ? userInfo.Email.ToString() : ""),
                            new Claim("SubjectId", userInfo.UserId.ToString()),
                            new Claim("CompanyId", userInfo.CompanyId.ToString()),
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    var retUser = new User()
                    {
                        Id = userInfo.UserId.Value,
                        FirstName = userInfo.Name,
                        ProfileId = userInfo.ProfileId,
                        CompanyId = userInfo.CompanyId,
                        Username = user,
                        Password = password,
                        Token = ""
                    };
                    retUser.Token = tokenHandler.WriteToken(token);

                    return retUser.WithoutPassword();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                }
            }


        }

        public IEnumerable<User> GetAll()
        {
            return _users.WithoutPasswords();
        }
    }
}