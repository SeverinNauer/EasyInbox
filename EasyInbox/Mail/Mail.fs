module Mail

open MailKit
open EasyInbox.Core
open MimeKit
open System
open Account

type ContactInformation = {
    Sender: EmailAddress
    To: EmailAddress list
    Cc: EmailAddress list option
    Bcc: EmailAddress list option
}

type EmailBody = 
    | Body of MimeEntity
    | HtmlBody of string
    | TextBody of string

type FileType = 
    | Pdf
    | JPEG
    | PNG
    | Other of string //TODO custom type

type FileDetails = {
    DateCreated: DateTime
    DateModified: DateTime
    Size: Int64
}

type File = {
    Type: FileType
    Path: string //TODO custom type
    Details: FileDetails
}

type SortedAttachement = {
    MailId: UniqueId
    File: File
    Tags: string list //TODO tag type
}

type UnsortedAttachement = {
    MailId: UniqueId
    File: File
}

type EmailData = {
    Id: UniqueId
    ContactInformation: ContactInformation
    Subject: string
    Body: EmailBody option
    Date: DateTime
}

type PartialSortedEmail = {
    EmailData: EmailData
    UnsortedAttachments: UnsortedAttachement list
    SortedAttachements: SortedAttachement list
}

type Email = 
    | Sorted of EmailData * SortedAttachement list
    | PartialSorted of PartialSortedEmail
    | Unsorted of EmailData * UnsortedAttachement list

type SortedMails = UniqueId list

type ReadNewMails = SortedMails -> EmailInbox -> Async<(EmailData * UnsortedAttachement list) list>

module Mapping = 
    
    let createContactInformationFromMessage (message: MimeMessage): ContactInformation = 
        let sender = 
            message.From.Mailboxes |> Seq.head |> (fun m -> m.Address) 
            |> EmailAddress.create "Email Sender" |> function | Ok sEmail -> sEmail
        {   Sender = sender
            To = []
            Cc = None
            Bcc = None  } 

    let createEmailDataFromMessage (message: MimeMessage, id: UniqueId): EmailData = {
            Id = id
            ContactInformation = createContactInformationFromMessage message
            Subject = message.Subject
            Body = Some(EmailBody.Body(message.Body))
            Date = message.Date.UtcDateTime
        }
