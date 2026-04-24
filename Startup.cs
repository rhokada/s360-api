using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApi.Helpers;
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
