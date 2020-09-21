namespace EasyInbox.Api

module CreateAccount = 

    open EasyInbox.Core

    type EmailInbox = {
        Sender: string
        Name: string //TODO string50
        Description: string //TODO string200
    }

    type Inbox = 
        | EmailInbox of EmailInbox

    type AccountInfo = {
        User: EmailAddress
        Inbox: Inbox
    }

    type CreateAccountCommand = AccountInfo -> Result<string,string>

    let CreateAccountCommand: CreateAccountCommand = fun accInfo ->
        //TODO implement persistent storage
        Ok("success")
