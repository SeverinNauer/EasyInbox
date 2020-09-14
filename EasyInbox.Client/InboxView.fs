﻿module InboxView

open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.FuncUI.Components.Hosts
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.Layout
open EasyInbox.Core

type User = {
    EmailAddress: EmailAddress //TODO email as type in shared library
    //TODO Add OAuth Token
    //TODO probably move up to application state
}

type NewAccount = 
    | New of Email:string
    | Invalid of Email:string * Error:string

type State =
    | AuthorizedAccount of User
    | NewAccount of NewAccount

let init = NewAccount(New(""))

type Msg = 
    | Authorize    
    | ChangeNewEmail of Email:string


let update msg state = 
    match (msg, state) with 
    | Authorize, (NewAccount(New(email)) | NewAccount(Invalid(email,_))) -> 
        match EmailAddress.create email with
        | Ok(email) -> State.AuthorizedAccount({EmailAddress = email}) 
        | Error(error) -> State.NewAccount(Invalid(email,error)) 
    | ChangeNewEmail email, _ -> NewAccount(New(email)) 
    | _ -> NewAccount(New(""))

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
                        match acc with 
                        | New email -> 
                            TextBox.create [
                                TextBox.width 200.0
                                TextBox.text email
                                TextBox.onTextChanged (fun text -> dispatch(ChangeNewEmail text))
                                TextBox.margin (0.0, 7.5, 0.0, 7.5)
                            ]
                        | Invalid (email, error) ->
                            TextBox.create [
                                TextBox.width 200.0
                                TextBox.text email
                                TextBox.onTextChanged (fun text -> dispatch(ChangeNewEmail text))
                                TextBox.margin (0.0, 7.5, 0.0, 7.5)
                                TextBox.errors <| ([error :> obj] |> Seq.ofList)
                                TextBox.hasErrors true
                            ]
                        Button.create [
                            Button.content "Add Account"
                            Button.classes [ "primary" ]
                            Button.horizontalAlignment HorizontalAlignment.Right
                            Button.onClick (fun _ -> Authorize |> dispatch )
                        ]
                    ]
                ]
        ]
    ]

type Host() as this = 
    inherit HostControl()
    do 
        Program.mkSimple (fun () -> init) update view
        |> Program.withHost this
        |> Program.run