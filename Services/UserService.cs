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
using Newtonsoft.Json;

namespace WebApi.Services
{
    public interface IUserService
    {
        User Authenticate(string user, string password, string appCd);
        User NewTempPassword(string user, string appCd);
        User ChangePassword(int userId, string password, string newpassword, string appCd);
        dynamic GetPermissions(int userId);
        IEnumerable<User> GetAll();
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {
            new User { UserId = 1, Email = "Test", Name = "Test", Password = "test" }
        };

        private readonly AppSettings _appSettings;
        private readonly IOptions<ConfigEmailBase> _configEmail;
        private readonly ConnectionStrings _connectionStrings;

        public UserService(IOptions<AppSettings> appSettings, IOptions<ConfigEmailBase> configEmail, IOptions<ConnectionStrings> connectionStrings)
        {
            _appSettings = appSettings.Value;
            _configEmail = configEmail;
            _connectionStrings = connectionStrings.Value;
        }

        public User Authenticate(string user, string password, string appId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_UserAuthenticated";

                    var userInfo = con.Query<User>(query, new
                    {
                        user = user,
                        appId = appId,
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
                            new Claim("Name", userInfo.Name),
                            new Claim("Email", userInfo.Email ??""),
                            new Claim("SubjectId", userInfo.UserId.ToString()),
                        }),
                        Expires = DateTime.UtcNow.AddMinutes(_appSettings.TimeSession), //tempo de validade do token jwt
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    var retUser = new User()
                    {
                        UserId = userInfo.UserId,
                        Name = userInfo.Name,
                        Email = user,
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

        public User NewTempPassword(string user, string appId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_UserNewTempPassword";

                    var userInfo = con.Query<User>(query, new
                    {
                        user = user,
                        appId = appId
                    }, commandType: CommandType.StoredProcedure).SingleOrDefault();

                    // return null if user not found
                    if (userInfo == null)
                        return null;

                // --- Lógica de Envio de E-mail ---

                    // 1. Verificar e desserializar a propriedade 'Msg'
                    if (string.IsNullOrEmpty(userInfo.Msg))
                        {
                    //        Console.WriteLine($"WARNING: A propriedade 'Msg' do usuário '{userInfo.Email}' está vazia. Não é possível enviar o e-mail de redefinição.");
                            // Você pode optar por lançar uma exceção, ou apenas retornar userInfo sem enviar e-mail.
                            return userInfo;
                        }

                    var configEmail = this._configEmail.Value;

                    // 2. Substituir o placeholder <<TEMP_PASSWORD>> no corpo da mensagem
                    var item = JsonConvert.DeserializeObject<dynamic>(Convert.ToString(userInfo.Msg));
                    string emailBody = item.Message;
                    emailBody = emailBody.Replace("<<TEMP_PASSWORD>>", userInfo.Password);

                    // 4. Enviar o e-mail usando os dados extraídos
                //    Mail.Send(Convert.ToString(item.To), Convert.ToString(item.NameTo), "", "", Convert.ToString(item.Subject), Convert.ToString(item.Message), confEmail);
                    Mail.Send(Convert.ToString(user), Convert.ToString(userInfo.Name),  "", "", Convert.ToString(item.Subject), Convert.ToString(emailBody), configEmail );

                //    Console.WriteLine($"INFO: E-mail de senha temporária enviado para '{userInfo.Email}'.");

                    return userInfo;

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

        public User ChangePassword(int userId, string password, string newpassword, string appCd)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_UserChangePassword";
                    var userInfo = con.Query<User>(query, new
                    {
                        userId = userId,
                        appId = appCd,
                        password = password,
                        newpassword = newpassword
                    }, commandType: CommandType.StoredProcedure).SingleOrDefault();
                    return userInfo;
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

        public dynamic GetPermissions(int userId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_UserPermissions";
                    var ret = con.Query(query, new
                    {
                        userId = userId
                    }, commandType: CommandType.StoredProcedure).ToList();
                    return ret;
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