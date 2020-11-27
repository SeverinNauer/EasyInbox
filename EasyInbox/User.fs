module EasyInbox.User

open System
open EasyInbox.Core.Types

type UserId = private UserId of Guid

module UserId = 
    let create guid = UserId.UserId guid
    let value (UserId id) = id

type Password = private Password of string

module Password = 
    let create pw = Password.Password pw
    let value (Password pw) = pw

type User = {
    UserId: UserId
    EmailAddress: EmailAddress
    Password: Password
}

type CreateUserCommand = { //TODO find better name
    EmailAddress: string
    Password: string
}

type LoginCommand = { //TODO find better name
    EmailAddress: string
    Password: string
}

type HashPassword = string -> Password

type UserCreated = User

type GetUserByEmail = EmailAddress -> AsyncResult<User, string>

type CreateUser = HashPassword -> Command<CreateUserCommand> -> Result<UserCreated,string>

type ValidatePassword = Password -> string -> bool

type IsValidLogin = ValidatePassword -> User -> Command<LoginCommand> -> bool


let createUser: CreateUser = 
    fun hash cmd ->
        let email = EmailAddress.create cmd.Data.EmailAddress
        match email with
        | Ok email -> Ok({
            EmailAddress = email
            UserId = Guid.NewGuid() |> UserId.create 
            Password = cmd.Data.Password |> hash
        })
        | Error err -> Error(err)

let isValidLogin: IsValidLogin = 
    fun validate user cmd ->
        validate user.Password cmd.Data.Password 

