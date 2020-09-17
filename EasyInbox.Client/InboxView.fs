module InboxView

open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.FuncUI.Components.Hosts
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.Layout
open EasyInbox.Core
open EasyInbox.Api
open MailKit.Security
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Controls

type User = {
    EmailAddress: EmailAddress 
    Token: SaslMechanismOAuth2
    //TODO probably move up to application state
}

type NewAccount = 
    | New of Email:string
    | Invalid of Email:string * Error:string

type State =
    | AuthorizedAccount of User
    | IsLoading
    | NewAccount of NewAccount

let init = NewAccount(New(""))

type Msg = 
    | Authorize of Email:string    
    | ChangeNewEmail of Email:string
    | SetAuthorized of Email: EmailAddress * token: SaslMechanismOAuth2


let update msg state: State * Cmd<Msg> = 
    match msg with 
    | Authorize(email) -> 
        match EmailAddress.create email with
        | Ok(email) ->
            IsLoading, Cmd.OfAsync.either Authorization.authorizeGmail email (fun (mail, token) -> SetAuthorized(mail,token))  (fun ex -> ChangeNewEmail(email.Value))
        | Error(error) -> State.NewAccount(Invalid(email,error)), Cmd.none 
    | ChangeNewEmail email -> NewAccount(New(email)), Cmd.none 
    | SetAuthorized(email,token) ->
        State.AuthorizedAccount {EmailAddress = email; Token = token}, Cmd.none

let view (state: State) dispatch = 
    Grid.create [
        Grid.rowDefinitions "* auto *"
        Grid.columnDefinitions "* auto *"
        Grid.children [
            match state with
            | AuthorizedAccount user -> 
                TextBlock.create [
                    Grid.column 1
                    TextBlock.text user.EmailAddress.Value
                ]
            | NewAccount(acc) ->
                StackPanel.create [
                    Grid.row 1
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
                            Button.content "Add Account"
                            Button.classes [ "primary" ]
                            Button.horizontalAlignment HorizontalAlignment.Right
                            Button.onClick ((fun _ -> Authorize(email) |> dispatch ), SubPatchOptions.Always)
                        ]
                    ]
                ]
            | IsLoading ->
                StackPanel.create [
                    Grid.row 1
                    Grid.column 1
                    StackPanel.children [
                        TextBlock.create [
                            TextBlock.text "loading..."
                        ]
                        ProgressBar.create [
                            ProgressBar.isIndeterminate true
                        ]
                    ]
                ]
        ]
    ]

type Host() as this = 
    inherit HostControl()
    do 
        Program.mkProgram (fun () -> init, Cmd.none) update view
        |> Program.withHost this
        |> Program.run