namespace Domain

open System
open CoreTypes

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