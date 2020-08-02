module Mail

open MailKit.Net.Imap
open MailKit
open MailKit.Security
open Google.Apis.Auth.OAuth2
open System.Threading
open System.IO

[<Literal>]
let GMAIL_SECRET_PATH = ".\Secrets\gmailsecret.apps.googleusercontent.com.json"


let getAuth email = 
    use stream = new FileStream(GMAIL_SECRET_PATH, FileMode.Open, FileAccess.Read)
    let secrets = GoogleClientSecrets.Load(stream).Secrets
    GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, [ "https://mail.google.com/" ], email, CancellationToken.None) 
    |> Async.AwaitTask 
    |> Async.RunSynchronously

let refreshAuth (creds: UserCredential) = 
    if creds.Token.IsExpired(Google.Apis.Util.SystemClock.Default) then
        creds.RefreshTokenAsync(CancellationToken.None) |> Async.AwaitTask |> Async.RunSynchronously |> Some
    else
        None


let readAllMail mailAddress = 
    use client = new ImapClient()
    client.Connect("imap.gmail.com",993,true)

    let creds = getAuth mailAddress

    let refreshResult = refreshAuth creds

    match refreshResult with 
    | Some res when not res -> failwith "Could not refresh token"
    | _ ->  let auth = SaslMechanismOAuth2(creds.UserId,creds.Token.AccessToken)

            client.Authenticate(auth)
            let inbox = client.Inbox
            inbox.Open FolderAccess.ReadOnly |> ignore

            printfn "Total Messages %d" inbox.Count

            client.Disconnect true