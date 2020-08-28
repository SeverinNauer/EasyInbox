module MailTest

open Xunit
open EasyInbox.Core
open Account

let createEmail field addr = 
    (EmailAddress.create field addr)
    |> function | Ok r -> r

let testInbox: EmailInbox = {
    Account = Unauthorized(createEmail "Account email" "test.easyinbox@gmail.com")
    Provider = Gmail
    Sender =  [createEmail "Scanner email" "scanner.easyinbox@gmail.com"]
}

[<Fact>]
let ``Read new Mails`` () =
    async {
        let! autorizedAccount = Authentication.authenticate Gmail <| createEmail "Account email" "test.easyinbox@gmail.com"
        let authorizedInbox = {
           Account = Authorized(autorizedAccount)
           Provider = Gmail
           Sender =  [createEmail "Scanner email" "scanner.easyinbox@gmail.com"]
        }
        let! res = Inbox.readNewMails [] authorizedInbox
        return ()
    }
    