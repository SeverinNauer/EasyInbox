namespace EasyInbox.Persistence

open System
open EasyInbox.User

module Mapper = 
    open EasyInbox.Persistence
    open EasyInbox.Core
    let toDomain (dbUser: Types.User) = 
        let email = Types.EmailAddress.create dbUser.Email
        match email with
        | Ok email -> ({
            EmailAddress = email
            UserId = UserId.create dbUser.UserId
            Password = Password.create dbUser.Password
        })
        | Error err -> failwith err
        

module UserRepository =
    open Context
    open LinqToDB

    type SaveUser = User -> Result<string,string>
    type GetById = Guid -> User option
    type GetByEmail = string -> User option

    let private defaultToOption<'a when 'a : equality>  = 
        function
        | user when user = Unchecked.defaultof<'a> -> None 
        | u -> Some u

    let GetByEmail: GetByEmail = 
        fun email -> 
            use db = new Connection()
            query {
                for user in db.GetTable<Types.User>() do
                where (user.Email = email)
                exactlyOneOrDefault
            } 
            |> defaultToOption 
            |> Option.map Mapper.toDomain

    let GetById : GetById = 
        fun uId ->
            use db = new Connection()
            query {
               for user in db.GetTable<Types.User>() do
               where (user.UserId = uId)
               exactlyOneOrDefault 
            } 
            |> defaultToOption 
            |> Option.map Mapper.toDomain

    let SaveUser: SaveUser = 
        fun user -> 
            use db = new Connection() 
            try 
                db.Insert(user) |> ignore
                Ok("Successfully saved")
            with
            | :? Npgsql.PostgresException as ex ->
                match ex.SqlState with 
                | "23505" -> Error("Entity with same data already in database") //TODO figure out error handling 
                | _ -> Error(ex.MessageText)
            | ex -> Error(sprintf "%s:%s" <| ex.GetType().FullName <| ex.Message) 
