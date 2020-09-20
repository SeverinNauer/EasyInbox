namespace EasyInbox.Api

open EasyInbox.Core
open MailKit.Security
open Google.Apis.Auth.OAuth2
open Microsoft.FSharp.Control
open System.Threading
open System.IO
open Google.Apis.Auth
open System


module Authorization = 
    type AuthorizeGmail = EmailAddress -> (EmailAddress * UserCredential) Async

    [<Literal>]
    let private GMAIL_SECRET_PATH = ".\Authorization\Secrets\gmailsecret.apps.googleusercontent.com.json"

    let private refreshGmailAuth (creds: UserCredential) = 
            async {
                match creds.Token.IsExpired(Google.Apis.Util.SystemClock.Default) with
                | true -> 
                    let! t = creds.RefreshTokenAsync(CancellationToken.None) |> Async.AwaitTask 
                    t |> ignore 
                | false ->
                    ()
            }

    let authorizeGmailCommand: AuthorizeGmail = fun mail ->
        async {
            use stream = new FileStream(GMAIL_SECRET_PATH, FileMode.Open, FileAccess.Read)
            let secrets = GoogleClientSecrets.Load(stream).Secrets
            let! creds = 
                GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, [ "https://www.googleapis.com/auth/gmail.readonly"], mail.Value, CancellationToken.None) 
                |> Async.AwaitTask 
            do! refreshGmailAuth creds
            return mail, creds 
        }

        
