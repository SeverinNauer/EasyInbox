module User

module Commands =
    
    type CreateUserCommand = {
        EmailAddress: string
        Password: string
    }


module Handlers = 
    open Commands
    open EasyInbox.Persistence.User
    open System

    type CreateUserHandler = CreateUserCommand -> Result<string, string>

    let CreateUserHandler (save: SaveUser): CreateUserHandler = 
        fun command ->   
            let user = { UserId= Guid.NewGuid() ; Email = command.EmailAddress; Password = command.Password }
            save user

            
