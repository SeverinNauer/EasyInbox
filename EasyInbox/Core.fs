module Core

type EmailAdress = EmailAdress of string

type Account = {
    Username: string
    Email: EmailAdress
}

type EmailProvider = 
    | GMX
    | Exchange
    | Gmail
    
type EmailInbox = {
    Account: Account
    Provider: EmailProvider
    ScannerEmail: EmailAdress
}

type NetworkAdress = NetworkAdress of string

type Inbox = 
    | NetworkAdress of NetworkAdress
    | Email of EmailInbox
    | PeerToPeer