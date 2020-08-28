module Authentication

open MailKit.Security
open System
open Google.Apis.Auth.OAuth2
open System.IO
open System.Threading
open Account

module private GmailAuthentication = 

    [<Literal>]
    let private GMAIL_SECRET_PATH = ".\Account\Secrets\gmailsecret.apps.googleusercontent.com.json"

    let private refreshGmailAuth (creds: UserCredential) = 
            async {
                match creds.Token.IsExpired(Google.Apis.Util.SystemClock.Default) with
                | true -> 
                    creds.RefreshTokenAsync(CancellationToken.None) |> Async.AwaitTask |> Async.RunSynchronously |> ignore
                | false ->
                    ()
            }


    let authenticate: Authenticate = fun mail ->
        async {
            use stream = new FileStream(GMAIL_SECRET_PATH, FileMode.Open, FileAccess.Read)
            let secrets = GoogleClientSecrets.Load(stream).Secrets
            let! creds = 
                GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, [ "https://mail.google.com/" ], mail.Value, CancellationToken.None) 
                |> Async.AwaitTask 
            do! refreshGmailAuth creds
            return  { Email = mail ; Token = SaslMechanismOAuth2(creds.UserId, creds.Token.AccessToken) }
        }


let authenticate provider account = 
    match provider with
    | Gmail -> GmailAuthentication.authenticate account
    | _ -> raise <| NotImplementedException("provider for mail not implemented")