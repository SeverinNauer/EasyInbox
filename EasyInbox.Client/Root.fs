namespace EasyInbox.Client

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Input
open Avalonia.FuncUI.Builder


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
           Grid.rowDefinitions "*"
           Grid.children [
               TabControl.create [
                   TabControl.tabStripPlacement Dock.Left
                   TabControl.padding 0.0
                   TabControl.viewItems [
                       TabItem.create [
                           TabItem.classes ["first"]
                           TabItem.cursor <| Cursor(StandardCursorType.Hand)
                           TabItem.header Icons.MailIcon
                           TabItem.content (ViewBuilder.Create<InboxView.Host>([ Grid.row 0 ; Grid.column 0]))
                       ]                        
                       TabItem.create [
                           TabItem.cursor <| Cursor(StandardCursorType.Hand)
                           TabItem.header Icons.FileMultiple
                       ]
                   ]
               ]
           ]
        ]
        
