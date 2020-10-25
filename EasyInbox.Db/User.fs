namespace EasyInbox.Persistence.User

open System

type User = {
    UserId: Guid
    Email: string
    Password: string
}

type SaveUser = User -> Result<string,string>
type GetById = Guid -> User option 


module Impl =

    open Context
    open LinqToDB

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
            | _ as ex -> Error(sprintf "%s:%s" <| ex.GetType().FullName <| ex.Message) 
