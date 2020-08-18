module EasyInbox

type EmailAddress = private EmailAddress of string
    with member x.Value = let (EmailAddress e) = x in e

module EmailAddress = 
    let create field (emailStr: string) = 
        if emailStr.Contains("@") then
            Ok <| EmailAddress(emailStr)
        else
            Error <| sprintf "Invalid email address for field: %s" field

    let value = function | EmailAddress mail -> mail

type Account = {
    Username: string
    Email: EmailAddress
}

type EmailProvider = 
    | GMX
    | Exchange
    | Gmail
    
type EmailInbox = {
    Account: Account
    Provider: EmailProvider
    Sender: EmailAddress list
}

type NetworkAdress = NetworkAdress of string

type Inbox = 
    | NetworkAdress of NetworkAdress
    | Email of EmailInbox
    | PeerToPeer