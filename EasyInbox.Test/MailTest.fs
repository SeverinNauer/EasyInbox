module MailTest

open Xunit
open EasyInbox

let createEmail field addr = 
    EmailAddress.create field addr
    |> function | Ok r -> r

let testInbox: EmailInbox = {
    Account = {Username = "Gmail account"; Email = createEmail "Account email" "test.easyinbox@gmail.com" }
    Provider = Gmail
    Sender =  [createEmail "Scanner email" "scanner.easyinbox@gmail.com"]
}

[<Fact>]
let ``Read Inbox Count`` () =
    Mail.readForInbox testInbox
