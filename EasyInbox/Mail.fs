module Mail

open MailKit.Net.Imap
open MailKit
open MailKit.Security
open Google.Apis.Auth.OAuth2
open System.Threading
open System.IO


let getAuth email = 
    use stream = new FileStream(@"D:\dev\secrets\client_secret_51444050718-r03e10jrr33rhrc352sis1joenk836g5.apps.googleusercontent.com.json",FileMode.Open,FileAccess.Read)
    let secrets = GoogleClientSecrets.Load(stream).Secrets
    GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, [ "https://mail.google.com/" ], email, CancellationToken.None) |> Async.AwaitTask |> Async.RunSynchronously



let readAllMail = 
    use client = new ImapClient()
    client.Connect("imap.gmail.com",993,true)

    let creds = getAuth "test@gmail.com"
    let auth = SaslMechanismOAuth2(creds.UserId,creds.Token.AccessToken)

    client.Authenticate(auth)
    let inbox = client.Inbox
    inbox.Open FolderAccess.ReadOnly |> ignore

    printfn "Total Messages %d" inbox.Count

    client.Disconnect true