namespace EasyInbox.Persistence

module Types = 

    open System

    type User = {
      UserId: Guid
      Email: string
      Password: string
    }
