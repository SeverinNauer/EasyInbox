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

[<AutoOpen>]
module Extensions =

    type Bitmap with
        static member Create (s: string) : Bitmap =
            let uri =
                if s.StartsWith("/")
                then Uri("avares://EasyInbox.Client/Assets" + s, UriKind.RelativeOrAbsolute)
                else Uri(s, UriKind.RelativeOrAbsolute);

        
            let assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            new Bitmap(assets.Open( uri));

module Navigation = 

    let Navigation =
        let brush =  Bitmap.Create "/Images/TabBackground.jpg" |> ImageBrush
        brush.Stretch <- Stretch.UniformToFill
        Grid.create [
            Grid.rowDefinitions "* auto"
            Grid.children [
                TabControl.create [
                    TabControl.background "#414244"
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
        
