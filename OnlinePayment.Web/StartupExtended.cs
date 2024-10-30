using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Globalization;
using AutoMapper;
using System;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using OnlinePayment.Logic.Services;
using OnlinePayment.Logic.Settings;
using Sh.Library.Authentication;
using Sh.Library.MailSender;
using OnlinePayment.Logic.Http;
using OnlinePayment.Logic.DataAccess;
using OnlinePayment.Logic.Model;
using OnlinePayment.Web.ViewModel;

namespace OnlinePayment.Web
{
    public class StartupExtended : Startup
    {

        public StartupExtended(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        { }

        protected override IList<CultureInfo> GetSupportedLanguages()
        {
            return new List<CultureInfo> {
                new CultureInfo("sv-se"),
                new CultureInfo("en-gb"),
            };
        }

        protected override RequestCulture GetDefaultCulture()
        {
            return new RequestCulture("sv-se");
        }

        protected override void CustomServiceConfiguration(IServiceCollection services)
        {
            services.AddTransient<IPaymentServiceExtended, PaymentServiceExtended>();
            services.AddTransient<IPaymentRequestServiceExtended, PaymentRequestServiceExtended>();
            services.AddTransient<IPaymentResponseService, PaymentResponseServiceExtended>();
            services.AddTransient<ISwishHttpClient, SwishHttpClient>();
            services.AddTransient<ISwishHttpService, SwishHttpService>();
            services.AddTransient<IPaymentDataAccessExtended, PaymentDataAccessExtended>();
            services.AddTransient<IKohaService, KohaService>();
            services.AddTransient<IKohaHttpClient, KohaHttpClient>();
            services.AddTransient<IPatronHttpService, PatronHttpService>();
            services.AddTransient<IPatronAccountHttpService, PatronAccountHttpService>();
            services.AddTransient<IPaymentCallbackServiceExtended, PaymentCallbackServiceExtended>();
            services.Configure<KohaApiSettings>(Configuration.GetSection("KohaApi"));
            services.Configure<SwishApiSettings>(Configuration.GetSection("SwishApi"));
            services.Configure<CertificationAuthenticationSettings>(Configuration.GetSection("CertificationAuthentication"));


            //services.AddLibraryStatistics(statisticsHost: Configuration["Statistics:Host"], bearerToken: Configuration["Statistics:BearerToken"]);
            services.AddLibraryAuthentication(authenticationHost: Configuration["Authentication:Host"]);
            //string sharedKeysFolder = Configuration["Application:KeysFolder"];
            //services.AddDataProtection()
            //    .PersistKeysToFileSystem(new DirectoryInfo(sharedKeysFolder))
            //    .SetApplicationName(Configuration["Application:Name"]);
            services.AddLibraryMailSender(mailSenderHost: Configuration["MailSender:Host"], bearerToken: Configuration["MailSender:BearerToken"]);

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .WithMethods("GET")
                               .AllowAnyHeader();
                    });
            });
        }

        protected override void CustomConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseLibraryApiAuthentication();
            app.UseLibraryAuthentication();
            //app.UseLibraryStatistics();
        }

        public override IMapper GetMapper()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                var profile = new MappingConfiguration();
                profile = AddAdditionalMappingConfig(profile);

                mc.AddProfile(profile);
            });

            return mapperConfig.CreateMapper();
        }

        public static MappingConfiguration AddAdditionalMappingConfig(MappingConfiguration profile)
        {
            profile.CreateMap<Payment, SessionViewModel>();
            profile.CreateMap<CallbackRequestModel, PaymentCallback>()
                .ForMember(x => x.Id, opt => opt.Ignore());

            return profile;
        }

    }

    public class CleanUpServiceExtended : CleanUpService
    {
        private const int WEEDING_TIME_IN_DAYS = -14;
        private readonly ILogService logService;

        public CleanUpServiceExtended(IOptions<ApplicationSettings> options,
            ILogService logService) : base(options)
        {
            this.logService = logService;
        }

        public override async Task ProcessCleanUp()
        {
            await base.ProcessCleanUp();
            await DeleteLogs();
        }

        private async Task DeleteLogs()
        {
            var all = await logService.GetAll();
            foreach (var item in all.Where(x => x.CreatedOn < DateTime.Now.AddDays(WEEDING_TIME_IN_DAYS)))
            {
                await logService.Delete(item.Id);
            }
        }
    }
}
