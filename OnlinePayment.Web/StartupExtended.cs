using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using AutoMapper;
using System;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using OnlinePayment.Logic.Services;
using OnlinePayment.Logic.Settings;
using OnlinePayment.Logic.Http;

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
            services.AddTransient<ISwishHttpClient, SwishHttpClient>();
            services.AddTransient<ISwishHttpService, SwishHttpService>();
            services.AddTransient<ISwishQrCodeHttpService, SwishQrCodeHttpService>();
            //services.Configure<KohaApiSettings>(Configuration.GetSection("KohaApiSettings"));
            services.Configure<SwishApiSettings>(Configuration.GetSection("SwishApi"));
            services.Configure<SwishQrCodeApiSettings>(Configuration.GetSection("SwishQrCodeApi"));
            services.Configure<CertificationAuthenticationSettings>(Configuration.GetSection("CertificationAuthentication"));

            //services.Configure<KohaApiSettings>(Configuration.GetSection("KohaApiSettings"));

        }

        protected override void CustomConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
            //profile.CreateMap<TFrom, TTo>();

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
