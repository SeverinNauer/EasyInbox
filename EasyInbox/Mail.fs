module Mail

open MailKit.Net.Imap
open MailKit
open EasyInbox
open MailKit.Search
open System.IO
open MimeKit

let writeFile fileName (attachment: MimeKit.MimeEntity) = 
    use stream = File.Create fileName
    match attachment with 
    | :? MimePart as mimePart -> mimePart.Content.DecodeTo(stream)
    | :? MessagePart as messagePart -> messagePart.Message.WriteTo(stream)
    | _ -> failwith "unsupported attachement"
   

let readForInbox (inbox: EmailInbox) = 
    use client = new ImapClient()
    client.Connect("imap.gmail.com",993,true)

    let auth = Authentication.authenticate inbox.Provider inbox.Account |> Async.RunSynchronously
    client.Authenticate(auth)
    client.Inbox.Open FolderAccess.ReadOnly |> ignore

    let mailIds = client.Inbox.Search(SearchQuery.FromContains(inbox.Sender |> List.head |> fun s -> s.Value))

    let mails = mailIds |> Seq.map (fun id -> client.Inbox.GetMessage(id)) |> Seq.toList

    let attachments = 
        mails 
        |> List.map (fun m -> m.Attachments) |> List.reduce Seq.append

    attachments |> Seq.iteri (fun i att -> writeFile (sprintf "D:\\%s")  att )

    client.Disconnect true