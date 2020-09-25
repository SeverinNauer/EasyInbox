namespace EasyInbox.Client

open Elmish
open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Input
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts
open Live.Avalonia
open Avalonia

type MainControl() as this =
    inherit HostControl()
    do


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

    interface ILiveView with
        member __.CreateView(window: Avalonia.Controls.Window) =
            if window.DataContext = null then do
                window.DataContext <- null
            window.AttachDevTools(KeyGesture(Key.F12))
            window.Title <- "EasyInbox"
            window.Height <- 800.0
            window.Width <- 1000.0
            MainControl() :> obj

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            let window = new LiveViewHost(this, fun msg -> printfn "%s" msg);
            window.StartWatchingSourceFilesForHotReloading();
            window.Show();
            base.OnFrameworkInitializationCompleted()
        | _ -> ()

module Program =

    [<EntryPoint>]
    let main(args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)