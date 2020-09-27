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
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Controls
open Avalonia.Media
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Media
open Avalonia.FuncUI.DSL
open Avalonia.Media
open Avalonia.Media
open Avalonia.Media

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
    | ChangeInbox of EmailInbox

let (|IsAuthorizedAccount|_|) = function 
    | AuthorizedAccount acc -> Some(acc)
    | _                     -> None

let update msg state = 
    match msg, state with 
    | Authorize(email),_ -> 
        match EmailAddress.create email with
        | Ok(email) ->
            IsLoading, Cmd.OfAsync.either Authorization.authorizeGmailCommand email (fun (mail, token) -> SetAuthorized(mail,token))  (fun ex -> ChangeNewEmail(email.Value)) 
        | Error(error) -> State.NewAccount(Invalid(email,error)), Cmd.none 
    | ChangeNewEmail email,_ -> NewAccount(New(email)), Cmd.none 
    | AddInbox(account),_ -> 
        State.AuthorizedAccount {account with Inbox = account.Inbox  |> Seq.append [{Sender = ""; Name = sprintf "Inbox %d" <| (account.Inbox |> Seq.length); Description = "" }]; SelectedIndex = Some(account.Inbox |> Seq.length)  }, Cmd.none 
    | ChangeInbox(newInbox), IsAuthorizedAccount acc ->
        State.AuthorizedAccount {acc with Inbox = acc.Inbox |> Seq.mapi (fun index inbox -> if index = acc.SelectedIndex.Value then newInbox else inbox )}, Cmd.none
    | SetAuthorized(email,token),_ ->
        State.AuthorizedAccount {User = { EmailAddress = email ; Token = token }; Inbox = []; SelectedIndex = None}, Cmd.none
    | _,_ -> failwith "Invalid update message"

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
        StackPanel.horizontalAlignment HorizontalAlignment.Stretch
        StackPanel.children [
            TextBlock.create [
                TextBlock.margin 7.0
                TextBlock.text inboxItem.Name
            ]
        ]
    ]

let brush = 
    let lBrush = SolidColorBrush(Color.Parse("#FFFFFF"),0.6)
    lBrush.Opacity <- 0.5
    lBrush



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
                        Border.create [
                            Grid.column 0
                            Grid.rowSpan 2
                            Border.borderBrush "#414244"
                            Border.cornerRadius 3.0
                            Border.borderThickness 1.0
                            Border.child (
                                Grid.create [
                                    Grid.rowDefinitions "auto *"
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
                                            ListBox.borderBrush "Transparent"
                                            ListBox.padding 0.0
                                            ListBox.borderThickness 0.0
                                            ListBox.selectedIndex <| (account.SelectedIndex |> Option.defaultValue -1) 
                                            ListBox.itemTemplate (DataTemplateView<EmailInbox>.create(viewBoxItem))
                                        ]
                                    ]
                                ]) 
                        ]
                       
                        match account.SelectedIndex with 
                        | Some index ->
                            let selectedItem = account.Inbox |> Seq.item index
                            Grid.create [
                                Grid.column 1
                                Grid.columnDefinitions "* auto *"
                                Grid.children [
                                    Border.create [
                                        Border.margin (0.0, 100.0)
                                        Border.padding 20.0
                                        Border.borderBrush "#414244"
                                        Border.borderThickness 1.0
                                        Border.cornerRadius 3.0
                                        Border.verticalAlignment VerticalAlignment.Top
                                        Grid.column 1
                                        Border.child (
                                            StackPanel.create [
                                                StackPanel.children [
                                                    TextBlock.create [
                                                        TextBlock.text "Name"
                                                    ]
                                                    TextBox.create [
                                                        TextBox.width 200.0
                                                        TextBox.margin (0.0,5.0,0.0, 10.0)
                                                        TextBox.text selectedItem.Name
                                                    ]
                                                    TextBlock.create [
                                                       TextBlock.text "Email address"
                                                    ]
                                                    TextBox.create [
                                                       TextBox.width 200.0
                                                       TextBox.margin (0.0,5.0,0.0, 10.0)
                                                       TextBox.borderThickness 2.0
                                                       TextBox.text selectedItem.Sender
                                                    ]
                                                    TextBlock.create [
                                                       TextBlock.text "Description"
                                                    ]
                                                    TextBox.create [
                                                       TextBox.width 200.0
                                                       TextBox.height 70.0
                                                       TextBox.margin (0.0, 5.0, 0.0, 15.0)
                                                       TextBox.textWrapping TextWrapping.Wrap
                                                       TextBox.acceptsReturn true
                                                       TextBox.text selectedItem.Description
                                                    ]
                                                    Button.create [
                                                        Button.classes ["primary"]
                                                        Button.width 70.0
                                                        Button.horizontalAlignment HorizontalAlignment.Right
                                                        Button.onClick ((fun _ -> ChangeInbox(selectedItem) |> dispatch),SubPatchOptions.Always)
                                                        Button.content "Save"
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
