
  _    _ __  __ ____  _____           _____ ____  
 | |  | |  \/  |  _ \|  __ \    /\   / ____/ __ \ 
 | |  | | \  / | |_) | |__) |  /  \ | |   | |  | |
 | |  | | |\/| |  _ <|  _  /  / /\ \| |   | |  | |
 | |__| | |  | | |_) | | \ \ / ____ | |___| |__| |
  \____/|_|  |_|____/|_|  \_/_/    \_\_____\____/ 
                                                   
----------------------------------------------------

Umbraco REST API has been installed!

By default CORS is enabled for Umbraco REST Api http calls. To modify the CORS policies for 
the Umbraco Rest API, add this line to your current OWIN startup:

app.ConfigureUmbracoRestApi(new UmbracoRestApiOptions()
	{
		//Modify the CorsPolicy as required
		CorsPolicy = new CorsPolicy()
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                AllowAnyOrigin = true
            }
	});

If you would like to have the Umbraco back office cookie used to authenticate the REST API 
you can add this line of code too:

app.UseUmbracoCookieAuthenticationForRestApi(ApplicationContext.Current);