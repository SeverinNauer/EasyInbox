module Authentication

open Microsoft.IdentityModel.Tokens
open System.Security.Claims
open EasyInbox.Persistence.Types
   

module Jwt = 
    open System.Text
    open System.IdentityModel.Tokens.Jwt
    open System

    let mutable private jwtSecret: string option = Option.None

    type Microsoft.Extensions.DependencyInjection.IServiceCollection with
        member x.SetJwtSecret(secret)= jwtSecret <- Some secret

    let generateJwtToken (user: User) =
        match jwtSecret with 
        | Some key -> 
            let tokenHandler = JwtSecurityTokenHandler()
            let key = Encoding.ASCII.GetBytes(key)
            let descriptor = 
                SecurityTokenDescriptor(
                    Subject = ClaimsIdentity(
                        [Claim(ClaimTypes.Email, user.Email); 
                        Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())]
                    ), 
                    Expires = System.Nullable(DateTime.UtcNow.AddMinutes(15.0)), 
                    SigningCredentials= SigningCredentials(SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
                )
            let token = tokenHandler.CreateToken(descriptor)
            tokenHandler.WriteToken token
        | None -> failwith "You need to provide a jwtKey in order to generate a token"