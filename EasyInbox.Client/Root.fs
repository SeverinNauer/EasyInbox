namespace EasyInbox.Client

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Input
open Avalonia.Layout
open Avalonia.Media.Imaging
open System 
open Avalonia.Platform
open Avalonia
open Avalonia.Media


module Navigation = 

    let Navigation =
        Grid.create [
            Grid.rowDefinitions "* auto"
            Grid.children [
                TabControl.create [
                    TabControl.tabStripPlacement Dock.Left
                    TabControl.padding 0.0
                    TabControl.viewItems [
                        TabItem.create [
                            TabItem.classes ["first"]
                            TabItem.cursor <| Cursor(StandardCursorType.Hand)
                            TabItem.header Icons.MailIcon
                        ]                        
                        TabItem.create [
                            TabItem.cursor <| Cursor(StandardCursorType.Hand)
                            TabItem.header Icons.FileMultiple
                        ]
                    ]
                ]
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
            Grid.rowDefinitions "*"
            Grid.columnDefinitions "auto *"
            Grid.children [
                Navigation.Navigation
                TextBlock.create [
                    TextBlock.text "Test"
                    Grid.column 1
                ]
            ]
        ]
        
