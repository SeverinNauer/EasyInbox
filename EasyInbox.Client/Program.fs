namespace EasyInbox.Client

open Elmish
open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Input
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts
open Live.Avalonia
open Avalonia.FuncUI.DSL
open ControlStyles

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "EasyInbox.Client"
        base.Width <- 1000.0
        base.Height <- 800.0

#if DEBUG
        this.AttachDevTools(KeyGesture(Key.F12))
#endif


        Elmish.Program.mkSimple (fun () -> Root.init) Root.update Root.view
        |> Program.withHost this
        |> Program.run

        
type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Load "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
        this.Styles.Load "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"
        this.Styles.Load "avares://EasyInbox.Client/Styles/BaseStyles.xaml"
        this.Styles.Load "avares://EasyInbox.Client/Styles/Styles.xaml"
        this.Styles.Load "avares://EasyInbox.Client/Styles/BaseTheme.xaml"
        for style in ControlStyles.allStyles do
            this.Styles.Add style

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.MainWindow <- MainWindow()
        | _ -> ()

module Program =

    [<EntryPoint>]
    let main(args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)