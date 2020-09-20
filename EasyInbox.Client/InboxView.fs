module InboxView

open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.FuncUI.Components.Hosts
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.Layout
open EasyInbox.Core
open Google.Apis.Auth.OAuth2
open CreateAccount
open Avalonia.FuncUI.Builder

type User = {
    EmailAddress: EmailAddress 
    Token: UserCredential
}

type NewAccount = 
    | New of Email:string
    | Invalid of Email:string * Error:string

type State =
    | NewAccount 
    | Inbox of User

let init = NewAccount

type Msg = 
    | AddNew of User:User    


let update msg state = 
    match msg with 
    | AddNew user -> Inbox(user)

let view (state: State) dispatch = 
    Grid.create [
        Grid.rowDefinitions "*"
        Grid.columnDefinitions "*"
        Grid.children [
            match state with
            | Inbox user -> 
                TextBlock.create [
                    TextBlock.text user.EmailAddress.Value
                ] 
            | NewAccount ->
                ViewBuilder.Create<CreateAccount.Host>([])
        ]
    ]

type Host() as this = 
    inherit HostControl()
    do 
        Program.mkSimple (fun () -> init) update view
        |> Program.withHost this
        |> Program.run