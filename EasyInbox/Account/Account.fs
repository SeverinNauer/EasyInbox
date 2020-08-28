module Account

open EasyInbox.Core
open MailKit.Security

type AuthorizedAccount = {
    Email: EmailAddress
    Token: SaslMechanismOAuth2
}

type Account = 
    | Unauthorized of EmailAddress
    | Authorized of AuthorizedAccount

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

type Authenticate = EmailAddress -> Async<AuthorizedAccount>