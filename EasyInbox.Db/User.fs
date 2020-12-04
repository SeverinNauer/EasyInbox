[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Persistence.User

open Context
open System
open Domain
open LinqToDB
open Persistence

let toDomain (dbUser: User): Domain.User = 
    let email = CoreTypes.EmailAddress.create dbUser.Email
    match email with
    | Ok email -> ({
        EmailAddress = email
        Id = UserId.create dbUser.UserId
        Password = Password.create dbUser.Password
    })
    | Error err -> failwith err

let fromDomain (domainUser: Domain.User) = {
        UserId = domainUser.Id |> UserId.value
        Email = domainUser.EmailAddress.Value
        Password = domainUser.Password |> Password.value
    }
        

type SaveUser = Domain.User -> Result<string,string>
type GetById = Guid -> Domain.User option
type GetByEmail = string -> Domain.User option

let private defaultToOption<'a when 'a : equality>  = 
    function
    | user when user = Unchecked.defaultof<'a> -> None 
    | u -> Some u

let GetByEmail: GetByEmail = 
    fun email -> 
        use db = new Connection()
        query {
            for user in db.GetTable<User>() do
            where (user.Email = email)
            exactlyOneOrDefault
        } 
        |> defaultToOption 
        |> Option.map toDomain

let GetById : GetById = 
    fun uId ->
        use db = new Connection()
        query {
            for user in db.GetTable<User>() do
            where (user.UserId = uId)
            exactlyOneOrDefault 
        } 
        |> defaultToOption 
        |> Option.map toDomain

let Save: SaveUser = 
    fun user -> 
        use db = new Connection() 
        try 
            let dbUser = user |> fromDomain
            db.Insert<User>(dbUser) |> ignore
            Ok("Successfully saved")
        with
        | :? Npgsql.PostgresException as ex ->
            match ex.SqlState with 
            | "23505" -> Error("Entity with same data already in database") //TODO figure out error handling 
            | _ -> Error(ex.MessageText)
        | ex -> Error(sprintf "%s:%s" <| ex.GetType().FullName <| ex.Message) 
