using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApi.Helpers;
using WebApi.Jobs;
using WebApi.Services;
using WebApi.Services.Interfaces;
using System;
using System.Threading.Tasks; // <<<<< Adicione esta linha

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers().AddNewtonsoftJson();

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // Adicionando a connectionStings
            var connectionStrings = Configuration.GetSection("ConnectionStrings");
            services.Configure<ConnectionStrings>(connectionStrings);

            // Adicionando a ConfigEmail
            var ConfigEmail = Configuration.GetSection("ConfigEmail");
            services.Configure<ConfigEmailBase>(ConfigEmail);
            
            // Adicionando a ReCaptcha
            var ConfigReCaptcha = Configuration.GetSection("RecaptchaConfig");
            services.Configure<RecaptchaConfig>(ConfigReCaptcha);

            // Adicionando a StorageConfig
            var storageConfig = Configuration.GetSection("StorageConfig");
            services.Configure<StorageConfig>(storageConfig);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                // >>>>>>>>>>>>>>>>>>>>>> INÍCIO DO NOVO CÓDIGO A SER ADICIONADO <<<<<<<<<<<<<<<<<<<<
                x.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // >>>>>> COLOQUE SEU BREAKPOINT AQUI <<<<<<
                        Console.WriteLine($"[JWT ERROR] Authentication failed: {context.Exception.GetType().Name} - {context.Exception.Message}");
                        // Inspecione 'context.Exception' para detalhes
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // >>>>>> COLOQUE SEU BREAKPOINT AQUI <<<<<<
                        Console.WriteLine("[JWT INFO] Token validated successfully.");
                        // Inspecione 'context.Principal' para ver as claims do usuário
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        // Este é opcional, mas útil para verificar se o token está chegando no cabeçalho
                        // string authorization = context.Request.Headers["Authorization"].FirstOrDefault();
                        // Console.WriteLine($"[JWT INFO] Token received in request: {authorization}");
                        return Task.CompletedTask;
                    }
                };
                // >>>>>>>>>>>>>>>>> FIM DO NOVO CÓDIGO A SER ADICIONADO <<<<<<<<<<<<<<<<<
            });

            // configure DI for application services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ReCaptchaService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<ISgpService, SgpService>();
            services.AddScoped<IAppService, AppService>(); // <<<< AQUI!
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IQuestionOptionService, QuestionOptionService>();
            services.AddScoped<ISurveyService, SurveyService>();
            services.AddScoped<ISurveyQuestionService, SurveyQuestionService>();
            services.AddScoped<ISurveySupService, SurveySupService>();
            services.AddScoped<ISurveyTypeService, SurveyTypeService>();
            services.AddScoped<IAdmUserService, AdmUserService>();
            services.AddScoped<IAdmHierarchyService, AdmHierarchyService>();
            services.AddScoped<IAdmHierarchyTeamService, AdmHierarchyTeamService>();
            services.AddScoped<IAdmAddressService, AdmAddressService>();
            services.AddScoped<IAdmCompanyService, AdmCompanyService>();
            services.AddScoped<IAdmCompanyDeptService, AdmCompanyDeptService>();
            services.AddScoped<IAdmDeptUserService, AdmDeptUserService>();
            services.AddScoped<IAdmCustomerService, AdmCustomerService>();
            services.AddScoped<IAdmCustomerSellerService, AdmCustomerSellerService>();
            services.AddScoped<IDataImportProLogService, DataImportProLogService>();
            services.AddScoped<IDataImportSellersService, DataImportSellersService>();
            services.AddScoped<IAdmPageService, AdmPageService>();
            services.AddScoped<IAdmRoleService, AdmRoleService>();
            services.AddScoped<IAdmRolePermissionService, AdmRolePermissionService>();
            services.AddScoped<IAdmRoleUserService, AdmRoleUserService>();
            services.AddScoped<IDashIndicadoresService, DashIndicadoresService>();
            services.AddScoped<IDashIndicadoresNotasService, DashIndicadoresNotasService>();
            services.AddScoped<IAiPromptService, AiPromptService>();
            services.AddScoped<ITranscricaoAudioService, TranscricaoAudioService>();
            services.AddScoped<IScoreSellerService, ScoreSellerService>();

            // Adicionando FirefliesConfig
            var firefliesConfig = Configuration.GetSection("FirefliesConfig");
            services.Configure<FirefliesConfig>(firefliesConfig);

            // Adicionando OpenAiConfig
            var openAiConfig = Configuration.GetSection("OpenAiConfig");
            services.Configure<OpenAiConfig>(openAiConfig);

            // Adicionando VisionApiConfig
            var visionApiConfig = Configuration.GetSection("VisionApiConfig");
            services.Configure<VisionApiConfig>(visionApiConfig);

            // Adicionando WhatsAppConfig
            var whatsAppConfig = Configuration.GetSection("WhatsAppConfig");
            services.Configure<WhatsAppConfig>(whatsAppConfig);

            // Hangfire com SQL Server (schema criado manualmente pelo DBA via SqlScripts/Hangfire_Schema.sql)
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(
                    Configuration.GetConnectionString("Default") ?? Configuration["ConnectionStrings:Default"],
                    new Hangfire.SqlServer.SqlServerStorageOptions
                    {
                        PrepareSchemaIfNecessary = true
                    }));

            services.AddHangfireServer();
            services.AddScoped<VisionImportJob>();
            services.AddScoped<GenerateSellerQuestionnaireEmailsJob>();
            services.AddScoped<AudioTranscriptionSubmissionJob>();
            services.AddScoped<AudioTranscriptionCollectionJob>();
            services.AddScoped<MsgSendEmailJob>();
            services.AddScoped<MsgSendWhatsAppJob>();
            services.AddScoped<AutomaticFupJob>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage(); // Isso mostrará a exceção detalhada no navegador para desenvolvimento
                                             // Seu middleware de log
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"[DEBUG LOG] Requisição Recebida: {context.Request.Method} {context.Request.Path}");
                await next();
            });

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = [new HangfireBasicAuthFilter("adminhang", "123hang")],
                DashboardTitle = "S360 - Jobs"
            });

            RecurringJob.AddOrUpdate<VisionImportJob>(
                "vision-import",
                job => job.ExecutarImportacaoVisionAsync(),
                Cron.Daily(3, 0) // roda todo dia às 03:00 — pode disparar manualmente a qualquer hora
            );

            RecurringJob.AddOrUpdate<GenerateSellerQuestionnaireEmailsJob>(
                "generate-seller-questionnaire-emails",
                job => job.ExecutarGenerateSellerQuestionnaireEmailsAsync(),
                Cron.Daily(8, 0) // roda todo dia às 03:00 — pode disparar manualmente a qualquer hora
            );

            RecurringJob.AddOrUpdate<MsgSendEmailJob>(
                "msg-send-email",
                job => job.ExecutarAsync(),
                Cron.Daily(9, 0)
            );

            RecurringJob.AddOrUpdate<MsgSendWhatsAppJob>(
                "msg-send-whatsapp",
                job => job.ExecutarAsync(),
                Cron.Daily(9, 0)
            );

            RecurringJob.AddOrUpdate<AutomaticFupJob>(
                "automatic-fup",
                job => job.ExecutarAsync(),
                Cron.Daily(8, 0)
            );

            RecurringJob.AddOrUpdate<AudioTranscriptionSubmissionJob>(
                "audio-transcription-submission",
                job => job.ExecutarAsync(),
                 Cron.Daily(3, 0) //"0 */2 * * *" // a cada 2 horas
            );

            RecurringJob.AddOrUpdate<AudioTranscriptionCollectionJob>(
                "audio-transcription-collection",
                job => job.ExecutarAsync(),
                Cron.Daily(3, 0) // a cada hora nos :30 (defasado da submissão)
            );
            

            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
