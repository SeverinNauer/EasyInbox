module CreateAccount

open EasyInbox.Core
open Google.Apis.Auth.OAuth2
open Elmish
open EasyInbox.Api
open Avalonia.FuncUI.DSL
open Avalonia.Controls 
open Avalonia.Layout
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.Elmish
open Avalonia.Input
open Icons 
open EasyInbox.Api.CreateAccount
open Avalonia.FuncUI.Components

type User = {
    EmailAddress: EmailAddress 
    Token: UserCredential
    //TODO probably move up to application state
}

type AuthorizedAccount = {
    User: User
    Inbox: EmailInbox seq
    SelectedIndex: int option
}

type NewAccount = 
    | New of Email:string
    | Invalid of Email:string * Error:string

type State =
    | AuthorizedAccount of AuthorizedAccount 
    | IsLoading
    | NewAccount of NewAccount


let init = NewAccount(New("")) 

type Msg = 
    | Authorize of Email:string    
    | ChangeNewEmail of Email:string
    | SetAuthorized of Email: EmailAddress * token: UserCredential
    | AddInbox of AuthorizedAccount 

let update msg state = 
    match msg with 
    | Authorize(email) -> 
        match EmailAddress.create email with
        | Ok(email) ->
            IsLoading, Cmd.OfAsync.either Authorization.authorizeGmailCommand email (fun (mail, token) -> SetAuthorized(mail,token))  (fun ex -> ChangeNewEmail(email.Value)) 
        | Error(error) -> State.NewAccount(Invalid(email,error)), Cmd.none 
    | ChangeNewEmail email -> NewAccount(New(email)), Cmd.none 
    | AddInbox(account) -> 
        State.AuthorizedAccount {account with Inbox = account.Inbox  |> Seq.append [{Sender = ""; Name = sprintf "Inbox %d" <| (account.Inbox |> Seq.length); Description = "" }]; SelectedIndex = Some(account.Inbox |> Seq.length)  }, Cmd.none 
    | SetAuthorized(email,token) ->
        State.AuthorizedAccount {User = { EmailAddress = email ; Token = token }; Inbox = []; SelectedIndex = None}, Cmd.none

let h1 = [
    TextBlock.fontSize <| double 18
]

let header title attrs = 
    StackPanel.create [
        yield! attrs 
        StackPanel.children [
            TextBlock.create [
                TextBlock.text title 
                TextBlock.margin (20.0, 20.0, 10.0, 10.0)
                yield! h1
            ]
        ]
    ]

let viewBoxItem inboxItem = 
    StackPanel.create [
        StackPanel.children [
            TextBlock.create [
                TextBlock.text inboxItem.Name
            ]
        ]
    ]

let view state dispatch =
    Grid.create [
        Grid.rowDefinitions "auto * auto *"
        Grid.columnDefinitions "* auto *"
        Grid.children [
            match state with
            | AuthorizedAccount (account) -> 
                header "Create account / Add Inbox" [Grid.columnSpan 3]
                Grid.create [
                    Grid.row 1
                    Grid.rowSpan 3
                    Grid.columnDefinitions "* 3*"
                    Grid.columnSpan 3
                    Grid.margin 20.0
                    Grid.children [
                        Grid.create [
                            Grid.rowDefinitions "auto *"
                            Grid.column 0
                            Grid.rowSpan 2
                            Grid.children [
                                DockPanel.create [
                                    DockPanel.children [
                                        TextBlock.create [
                                            TextBlock.fontSize 16.0
                                            TextBlock.text "Inbox"
                                            TextBlock.margin(10.0, 15.0)
                                            TextBlock.verticalAlignment VerticalAlignment.Center
                                            TextBlock.dock Dock.Left
                                        ]
                                        Button.create [
                                            Button.margin(10.0, 15.0)
                                            Button.content AddIcon 
                                            Button.onClick ((fun _ -> AddInbox(account) |> dispatch), SubPatchOptions.OnChangeOf(account))
                                            Button.horizontalAlignment HorizontalAlignment.Right
                                            Button.cursor <| Cursor(StandardCursorType.Hand)
                                            Button.dock Dock.Right
                                        ]
                                    ]
                                ]
                                ListBox.create [
                                    Grid.row 1
                                    ListBox.dataItems account.Inbox
                                    ListBox.selectedIndex <| (account.SelectedIndex |> Option.defaultValue -1) 
                                    ListBox.itemTemplate (DataTemplateView<EmailInbox>.create(viewBoxItem))
                                ]
                            ]
                        ] 
                        match account.SelectedIndex with 
                        | Some index ->
                            let selectedItem = account.Inbox |> Seq.item index
                            Grid.create [
                                Grid.column 1
                                Grid.columnDefinitions "* auto *"
                                Grid.children [
                                    Border.create [
                                        Border.padding 20.0
                                        Border.background "#414244"
                                        Grid.column 1
                                        Border.child (
                                            StackPanel.create [
                                                StackPanel.background "#414244"
                                                StackPanel.children [
                                                    TextBox.create [
                                                        TextBox.width 200.0
                                                        TextBox.text selectedItem.Name
                                                    ]
                                                ]
                                            ]
                                        )
                                    ]
                                    
                                ]
                            ]
                            
                        | None -> yield! []
                    ]
                ]
            | NewAccount(acc) ->
                header "Create account" [Grid.columnSpan 3] 
                StackPanel.create [
                    Grid.row 2
                    Grid.column 1
                    StackPanel.children [
                        TextBlock.create [
                            TextBlock.text "Email address"
                        ]
                        let (email, error) = 
                            match acc with 
                            | New email -> (email, None)
                            | Invalid(email, error) -> (email, Some(error))
                        TextBox.create [
                            TextBox.width 200.0
                            TextBox.text email
                            TextBox.onTextChanged (fun text -> dispatch(ChangeNewEmail text))
                            match error with 
                            | Some error -> yield! [TextBox.errors [error]]
                            | None -> yield! []
                            TextBox.margin (0.0, 7.5, 0.0, 7.5)
                        ]
                        Button.create [
                            Button.content "Authorize Account"
                            Button.classes [ "primary" ]
                            Button.horizontalAlignment HorizontalAlignment.Right
                            Button.onClick ((fun _ -> Authorize(email) |> dispatch ), SubPatchOptions.Always)
                        ]
                    ]
                ]
            | IsLoading ->
                ProgressBar.create [
                    Grid.column 1 
                    Grid.row 2
                    ProgressBar.isIndeterminate true
                ]
        ]            
    ]
     
type Host() as this = 
    inherit HostControl()
    do 
        Program.mkProgram (fun () -> init,Cmd.none) update view
        |> Program.withHost this
        |> Program.run
