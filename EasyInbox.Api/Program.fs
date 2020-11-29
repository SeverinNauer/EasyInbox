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
            let! user = ctx.BindJsonAsync<LoginCommand>() 
            let dbUser = UserRepository.GetByEmail user.EmailAddress
            let isValid = UserHelpers.ValidateUser UserRepository.GetByEmail user
            match isValid, dbUser with
            | true, Some dbUser -> 
                let token = Authentication.Jwt.generateJwtToken dbUser 
                return! Successful.OK ({| token = token |}) next ctx
            | _, _ -> 
                return! RequestErrors.UNAUTHORIZED "Unauthorized"  "Incorrect Login Data" "Incorrect Login Data" next ctx
        }

let getGoogleFlow () = 
    use stream = new FileStream(@".\Secrets\gmailsecret.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read)
    let secrets = GoogleClientSecrets.Load(stream).Secrets
    new GoogleAuthorizationCodeFlow(
        GoogleAuthorizationCodeFlow.Initializer(
            DataStore = FileDataStore("EasyInbox.Google.Drive"),
            ClientSecrets = secrets,
            Scopes = [DriveService.Scope.Drive]
        ) 
    )
let getUserId (ctx: HttpContext) = 
    ctx.User.Claims 
    |> Seq.tryFind (fun c -> c.Type = ClaimTypes.NameIdentifier) 
    |> function 
        | Some claim -> Some claim.Value 
        | None -> None
let addStorageHandler: HttpHandler = 
    fun (next: HttpFunc)(ctx: HttpContext) ->
        task {
           let idOp = getUserId ctx 
           match idOp with 
           | Some id -> 
                use flow = getGoogleFlow () 
                let! res = AuthorizationCodeWebApp(flow,"http://localhost:3000/gdrivecallback","gdrivestorage").AuthorizeAsync(id ,CancellationToken.None) 
                return! Successful.OK (res.RedirectUri) next ctx 
           | None -> 
                return! RequestErrors.BAD_REQUEST "Invalid Authorization" next ctx
        }

type GoogleDriveCallback = {
    State: string
    Code: string
}



let googleDriveCallback: HttpHandler = 
    fun next ctx ->
        task {
            let id = ctx |> getUserId |> Option.defaultValue ""
            let! {State = state; Code = code} = ctx.BindJsonAsync<GoogleDriveCallback>() 
            let uri = ctx.Request.Scheme + "://" + ctx.Request.Host.Value + ctx.Request.Path 
            use flow = getGoogleFlow ()
            flow.ExchangeCodeForTokenAsync(id,code,"http://localhost:3000/gdrivecallback", CancellationToken.None).Result |> ignore
            let result = AuthWebUtility.ExtracRedirectFromState(flow.DataStore, id, state).Result
            return! redirectTo false result next ctx
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

let authorize = requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let webApp =
    choose [
        GET >=> choose [
            route "/" >=> text "Hello World"
        ]
        POST >=> choose [
            route "/account/login" >=> loginHandler
            route "/account/create" >=> bindJson<CreateUserCommand> (fun com -> (UserHandlers.CreateUserHandler UserRepository.SaveUser com) |> Successful.OK)
        ]
        subRoute "/storage" authorize >=>
            choose [
                POST >=> choose [
                    route "/add" >=> addStorageHandler
                    route "/add/gdrive/callback" >=> googleDriveCallback
                ]
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