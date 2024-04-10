namespace widf

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Avalonia.Media

type MainWindow() =
    inherit HostWindow()
    do
        base.Title <- "WPILib Installer Downloader"
        base.Content <- Views.mainView ()
        base.TransparencyLevelHint <- [WindowTransparencyLevel.AcrylicBlur]
        base.Background <- SolidColorBrush(Colors.White, 0.6)


type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add (FluentTheme())
        this.RequestedThemeVariant <- Styling.ThemeVariant.Light

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
            // .UseSkia()
            .StartWithClassicDesktopLifetime(args)