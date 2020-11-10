namespace EasyInbox.Persistence

open System

module UserRepository =
    open Context
    open LinqToDB
    open EasyInbox.Persistence.Types

    type SaveUser = User -> Result<string,string>
    type GetById = Guid -> User option
    type GetByEmail = string -> User option

    let private defaultToOption<'a when 'a : equality>  = 
        function
        | user when user = Unchecked.defaultof<'a> -> None 
        | _ as u -> Some u

    let GetByEmail: GetByEmail = 
        fun email -> 
            use db = new Connection()
            query {
                for user in db.GetTable<User>() do
                where (user.Email = email)
                exactlyOneOrDefault
            } |> defaultToOption

    let GetById : GetById = 
        fun uId ->
            use db = new Connection()
            query {
               for user in db.GetTable<User>() do
               where (user.UserId = uId)
               exactlyOneOrDefault 
            } |> defaultToOption

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
