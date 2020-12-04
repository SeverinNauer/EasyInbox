[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Domain.User

open CoreTypes
open System

type CreateUserCommand = { //TODO find better name
    EmailAddress: string
    Password: string
}

type LoginCommand = { //TODO find better name
    EmailAddress: string
    Password: string
}

type HashPassword = string -> Password

type UserCreated = |User of Domain.User

type GetUserByEmail = EmailAddress -> AsyncResult<User, string>

type CreateUser = HashPassword -> Command<CreateUserCommand> -> Result<UserCreated,string>

type ValidatePassword = Password -> string -> bool

type IsValidLogin = ValidatePassword -> User -> Command<LoginCommand> -> bool


let create: CreateUser = 
    fun hash cmd ->
        let email = EmailAddress.create cmd.Data.EmailAddress
        match email with
        | Ok email -> Ok <| UserCreated.User ({
            EmailAddress = email
            UserId = Guid.NewGuid() |> UserId.create 
            Password = cmd.Data.Password |> hash
            }) 
        | Error err -> Error(err)

let isValidLogin: IsValidLogin = 
    fun validate user cmd ->
        validate user.Password cmd.Data.Password 

