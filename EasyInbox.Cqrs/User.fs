namespace EasyInbox.CQService.User

module Commands =
    
    type CreateUserCommand = {
        EmailAddress: string
        Password: string
    }

    type LoginCommand = {
        EmailAddress: string
        Password: string
    }


module Handlers = 
    open Commands
    open EasyInbox.Persistence.User.Repository
    open EasyInbox.Persistence.Types
    open System
    open BCrypt.Net

    //TODO encrypt password
    type CreateUserHandler = CreateUserCommand -> Result<string, string>

    let CreateUserHandler (save: SaveUser): CreateUserHandler = 
        fun command ->   
            let user = { UserId = Guid.NewGuid() ; Email = command.EmailAddress; Password = BCrypt.HashPassword(command.Password) }
            save user

module Helpers = 
    open EasyInbox.Persistence.User.Repository
    open BCrypt.Net
    open Commands

    let ValidateUser (getUserFromDb: GetByEmail) (command: LoginCommand) =
        let dbUser = getUserFromDb command.EmailAddress 
        match dbUser with 
        | Some user -> BCrypt.Verify(command.Password, user.Password)
        | None -> false

