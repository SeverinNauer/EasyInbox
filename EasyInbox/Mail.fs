module Mail

open MailKit.Net.Imap
open MailKit
open EasyInbox
open MailKit.Search

let readForInbox (inbox: EmailInbox) = 
    use client = new ImapClient()
    client.Connect("imap.gmail.com",993,true)

    let auth = Authentication.authenticate inbox.Provider inbox.Account |> Async.RunSynchronously
    client.Authenticate(auth)
    client.Inbox.Open FolderAccess.ReadOnly |> ignore

    let mailIds = client.Inbox.Search(SearchQuery.FromContains(inbox.Sender |> List.head |> fun s -> s.Value))

    let mails = mailIds |> Seq.map (fun id -> client.Inbox.GetMessage(id)) |> Seq.toList

    client.Disconnect true