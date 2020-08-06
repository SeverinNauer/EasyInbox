namespace EasyInbox.Client

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia
open Avalonia.Media
open Avalonia.Controls.Shapes
open Avalonia.Input

module MenuItems = 
    let testItems: Avalonia.FuncUI.Types.IView list = [
        MenuItem.create [
            MenuItem.header "Test"
        ]
    ]

module Root = 
    
    type Page =
        | Settings

    let init = Settings

    type Msg = 
        Navigate of Page

    let update msg state : Page = 
        match msg with 
        | Navigate p -> p

    let view (state: Page) dispatch = 
        Grid.create [
            
            Grid.children [
                TextBlock.create [
                    TextBlock.text "Test"
                ]
            ]
        ]
        
