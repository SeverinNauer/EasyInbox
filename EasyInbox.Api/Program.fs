module EasyInbox.Api.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Authentication.Google
open Microsoft.AspNetCore.Authentication
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Authentication.Cookies


let helloHandler (name: string) = 
    text <| sprintf "Hello %s" name

let challenge (scheme : string) (redirectUrl: string) : HttpHandler =
     fun (next : HttpFunc) (ctx : HttpContext) ->
         task {
             do! ctx.ChallengeAsync(
                     scheme,
                     AuthenticationProperties(RedirectUri=redirectUrl))
             return! next ctx
         }

let signinHandler: HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let list = ctx.User.Claims |> Seq.toList
        Successful.OK("Sucess") next ctx

type UserLogin = {
    EmailAddress: string
    Password: string
}

let loginHandler: HttpHandler = 
    fun (next: HttpFunc)(ctx: HttpContext) ->
        task {
            let! user = ctx.BindJsonAsync<UserLogin>()
            return! Successful.OK ("Successfully logged in for user " + user.EmailAddress) next ctx
        }

let authenticate : HttpHandler =
   requiresAuthentication <| RequestErrors.BAD_REQUEST ""

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> text "Hello world"
                routef "/hello/%s" helloHandler
                route "/google-login" >=> challenge GoogleDefaults.AuthenticationScheme "/google-callback"
                route "/google-callback" >=> authenticate >=> signinHandler
            ]
        POST >=> choose [
            route "/login" >=> loginHandler
            route "/google-login" >=> Successful.OK("Success")
        ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.EnvironmentName with
    | "Development" -> app.UseDeveloperExceptionPage()
    | _ -> app.UseGiraffeErrorHandler(errorHandler))
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseAuthentication()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    let provider = services.BuildServiceProvider()
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services.AddAuthentication(fun o -> o.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddGoogle(fun options -> 
            let section: IConfigurationSection = provider.GetService<IConfiguration>().GetSection("Authentication:Google")
            options.ClientId <- section.["ClientId"]
            options.ClientSecret <-section.["ClientSecret"]
        ) |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0