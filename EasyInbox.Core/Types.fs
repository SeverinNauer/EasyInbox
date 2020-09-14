namespace EasyInbox.Core

type EmailAddress = private EmailAddress of string
        with member x.Value = let (EmailAddress e) = x in e

module EmailAddress = 
        let create (emailStr: string) = 
            if emailStr.Contains("@") then
                Ok <| EmailAddress(emailStr)
            else
                Error <| sprintf "\"%s\" is not a valid email" emailStr 
