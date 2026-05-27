//MODELO DE JOB

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Jobs
{
    public class GenerateSellerQuestionnaireEmailsJob
    {
        private readonly ConnectionStrings _connectionStrings;
        private readonly VisionApiConfig _apiConfig;

        public GenerateSellerQuestionnaireEmailsJob(IOptions<ConnectionStrings> connectionStrings, IOptions<VisionApiConfig> apiConfig)
        {
            _connectionStrings = connectionStrings.Value;
            _apiConfig = apiConfig.Value;
        }

        public async Task ExecutarGenerateSellerQuestionnaireEmailsAsync()
        {

            using var con = new SqlConnection(_connectionStrings.Default);
            con.Open();
            try
            {
                con.Execute("SP_ProcessMsgsToSend", commandType: CommandType.StoredProcedure , commandTimeout: 1800);
            }
            catch 
            {                
                throw;
            }
            finally
            {
                con.Close();
            }
        }
    }
    
}
