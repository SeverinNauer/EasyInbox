namespace EasyInbox.CQService

module UserCommands =
    
    type CreateUserCommand = {
        EmailAddress: string
        Password: string
    }

    type LoginCommand = {
        EmailAddress: string
        Password: string
    }


module UserHandlers = 
    open EasyInbox.Persistence.UserRepository
    open UserCommands
    open EasyInbox.Persistence.Types
    open System
    open BCrypt.Net

    type CreateUserHandler = CreateUserCommand -> Result<string, string>

    let CreateUserHandler (save: SaveUser): CreateUserHandler = 
        fun command ->   
            { UserId = Guid.NewGuid()  
              Email = command.EmailAddress 
              Password = BCrypt.HashPassword(command.Password) } 
              |> save |> Ok

module UserHelpers = 
    open EasyInbox.Persistence.UserRepository
    open BCrypt.Net
    open UserCommands

    let ValidateUser (getUserFromDb: GetByEmail) (command: LoginCommand) =
        let dbUser = getUserFromDb command.EmailAddress 
        match dbUser with 
        | Some user -> BCrypt.Verify(command.Password, user.Password)
        | None -> false

