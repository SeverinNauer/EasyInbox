module MailTest

open Xunit
open Mail


[<Fact>]
let ``Read Inbox Count`` () =
    Mail.readAllMail "test.easyinbox@gmail.com"
