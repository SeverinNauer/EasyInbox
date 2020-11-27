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
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text
open System.Security.Claims
open EasyInbox
open Authentication.Jwt
open BCrypt.Net
open EasyInbox.Core.Types
open EasyInbox.Persistence
open EasyInbox.User



let authorizedHandler: HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let list = ctx.User.Claims |> Seq.tryFind (fun c -> c.Type = ClaimTypes.Name)
        Successful.OK(list.Value.Value) next ctx

let helloHandler (name: string) = 
    text <| sprintf "Hello %s" name

let signinHandler: HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let list = ctx.User.Claims |> Seq.toList
        Successful.OK("Sucess") next ctx

let createCommand data = {
    Timestamp = new DateTime()
    Data = data
    Id = Guid.NewGuid()
}

let loginHandler: HttpHandler = 
    fun (next: HttpFunc)(ctx: HttpContext) ->
        task {
            let! user = ctx.BindJsonAsync<User.LoginCommand>()
            
            let validate password passwordStr = 
                BCrypt.Verify(passwordStr, password |> User.Password.value)
            
            let dbUser = UserRepository.GetByEmail user.EmailAddress
            match dbUser with
            | Some dbUser -> 
                let isValid = 
                    User.isValidLogin 
                        validate 
                        (dbUser) 
                        (createCommand <| user) 
                match isValid with
                | true -> 
                    let token = Authentication.Jwt.generateJwtToken user 
                    return! Successful.OK ({| token = token |}) next ctx
                | false -> 
                    return! RequestErrors.UNAUTHORIZED "Unauthorized"  "Incorrect Email or Password" "" next ctx
            | None ->
                return! RequestErrors.UNAUTHORIZED "Unauthorized"  "Incorrect Email or Password" "" next ctx
        }

let hashPassword pwd = 
    BCrypt.HashPassword(pwd) 
    |> User.Password.create

let createUser: HttpHandler = 
    fun next ctx ->
        task {
            let! user = ctx.BindJsonAsync<User.CreateUserCommand>()
            let userCmd = createCommand user
            let res = User.createUser hashPassword userCmd
            match res with
            | Ok user -> 
                UserRepository.SaveUser user |> ignore
                return! Successful.OK ("Successfully Saved") next ctx
            | Error err -> 
                return! RequestErrors.BAD_REQUEST err next ctx
        }

let authorize =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> text "Hello World"
                route "/secured" >=> authorize >=> authorizedHandler
                routef "/hello/%s" helloHandler
            ]
        POST >=> choose [
            route "/account/login" >=> loginHandler
            route "/account/create" >=> createUser
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
    builder.WithOrigins("http://localhost:3000")
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

let jwtBearerOptions (secret: string) (cfg : JwtBearerOptions) =
   let key = Encoding.ASCII.GetBytes secret
   cfg.SaveToken <- true
   cfg.IncludeErrorDetails <- true
   cfg.TokenValidationParameters <- 
        TokenValidationParameters(
            ValidateIssuerSigningKey = true, 
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        )


let configureServices (services : IServiceCollection) =
    let provider = services.BuildServiceProvider()
    let secret =  provider.GetService<IConfiguration>().GetSection("Authentication:Jwt").["Secret"]
    let bearerOptions = jwtBearerOptions <| secret 
    Builder.setSettings() 
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services.AddAuthentication(fun o -> 
            o.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            o.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme
        ).AddJwtBearer(Action<JwtBearerOptions> bearerOptions) |> ignore
    services.SetJwtSecret(secret)

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