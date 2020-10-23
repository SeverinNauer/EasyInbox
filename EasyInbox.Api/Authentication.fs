module Authentication

open Microsoft.IdentityModel.Tokens
open System.Security.Claims
open System



module Types = 
   type UserLogin = {
      EmailAddress: string
      Password: string
   }

open Types


module Jwt = 
    open System.Text
    open System.IdentityModel.Tokens.Jwt

    // there is probably a cleaner way to read the secret from the options
    let mutable jwtSecret: string option = Option.None

    
    let generateJwtToken (user: UserLogin) =
        match jwtSecret with 
        | Some key -> 
            let tokenHandler = JwtSecurityTokenHandler()
            let key = Encoding.ASCII.GetBytes(key)
            let descriptor = 
                SecurityTokenDescriptor(
                    Subject = ClaimsIdentity([Claim(ClaimTypes.Name,user.EmailAddress)]), 
                    Expires = System.Nullable(DateTime.UtcNow.AddMinutes(15.0)), 
                    SigningCredentials= SigningCredentials(SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
                )
            let token = tokenHandler.CreateToken(descriptor)
            tokenHandler.WriteToken token
        | None -> failwith "You need to provide a jwtKey in order to generate a token"