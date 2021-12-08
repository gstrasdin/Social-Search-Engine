using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Social.SearchEngine.Services.Iframely;
using Social.SearchEngine.Services.Twitter;

namespace Social.SearchEngine
{
	public class Startup
	{
		private readonly IConfiguration _configuration;
	
		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews();
			services.AddSingleton<HttpClient>();
			services.AddSingleton<TwitterService>();
			services.AddSingleton<IframelyService>();

			/* Iframely configuration */
			var iframelySection = _configuration.GetSection("Iframely");
			var iframelyConfig = new IframelyConfiguration();
			iframelySection.Bind(iframelyConfig);
			services.AddSingleton(iframelyConfig);

			/* Twitter configuration */
			var twitterSection = _configuration.GetSection("Twitter");
			var twitterConfig = new TwitterConfiguration();
			twitterSection.Bind(twitterConfig);
			services.AddSingleton(twitterConfig);

			
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
