module Inbox

open MailKit.Net.Imap
open MailKit
open MailKit.Search
open System.IO
open MimeKit
open Account
open Mail

let tempPath = 
    Path.Combine(Path.GetTempPath(), "EasyInbox")

let ensurePath path = 
    match Directory.Exists path with
    | true -> ()
    | _ -> 
        try 
            path |> Directory.CreateDirectory |> ignore
        with
            | :? _ -> failwith <| sprintf "Can not create Directory with path '%s'" path

let writeFile (fileName: string) (attachment:MimeEntity) =
    Directory.CreateDirectory (Path.GetDirectoryName(fileName)) |> ignore
    use stream = File.Create fileName
    match attachment with
    | :? MimePart as mimePart -> mimePart.Content.DecodeTo(stream)
    | :? MessagePart as messagePart -> messagePart.Message.WriteTo(stream)
    | _ -> failwith "unsupported attachement"

let getTempFilePath id (attachement: MimeEntity) =
    ensurePath tempPath
    Path.Combine(tempPath, id, (attachement :?> MimePart).FileName)


let GetImapSettings = function
    | Gmail -> ("imap.gmail.com", 993)
    | _ -> failwith "unimplemented provider"

let readNewMails: ReadNewMails = fun sortedMails -> fun inbox -> 
    async {
        let (host,port) = GetImapSettings inbox.Provider
        use client = new ImapClient()
        client.Connect(host, port, true)
        match inbox.Account with
        | Authorized acc ->
            do! client.AuthenticateAsync(acc.Token) |> Async.AwaitTask
            client.Inbox.Open FolderAccess.ReadOnly |> ignore
            let! ids = client.Inbox.SearchAsync(SearchQuery.FromContains(inbox.Sender |> List.head |> fun s -> s.Value)) |> Async.AwaitTask 

            let messages = 
                ids 
                |> Seq.filter(fun mId -> not(sortedMails 
                |> List.contains mId)) 
                |> Seq.map(fun id -> (id,client.Inbox.GetMessage(id))) 
            //do! client.DisconnectAsync(true) |> Async.AwaitTask 
            return messages 
                   |> Seq.map(fun (id,m) ->  
                       (Mail.Mapping.createEmailDataFromMessage(m,id),
                           m.Attachments 
                           |> Seq.map (fun att -> (getTempFilePath <| id.ToString() <| att, att))
                           |> Seq.map (fun (fName, att) -> 
                                writeFile fName att
                                let file = FileInfo(fName)
                                {
                                    MailId = id
                                    File = {
                                        Type = Pdf
                                        Path = fName
                                        Details = {
                                            DateCreated = file.CreationTime
                                            DateModified = file.LastAccessTime
                                            Size = file.Length
                                        }
                                    }
                                }
                            ) |> Seq.toList)
                   ) |> Seq.toList

        | _ -> return failwith "Unauthorized account"
    }
