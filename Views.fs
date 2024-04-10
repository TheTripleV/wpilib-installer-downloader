namespace widf

open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout

open System.Runtime.InteropServices

type Screen =
    | StartScreen
    | DownloadScreen
    | CustomDownloadScreen

type StateStore =
    { Screen: IWritable<Screen> }

    static member Init () =

        { Screen = new State<_>(CustomDownloadScreen) }


// [<RequireQualifiedAccess>]
module StateStore =
    let shared = StateStore.Init ()

// module Views =
//     let a = 2

// [<AbstractClass; Sealed>]
module Views =
    open Avalonia.Interactivity
    let startView () = 
        Component.create ("startView", fun ctx ->
            Grid.create [
                Grid.rowDefinitions "*,Auto"
                Grid.margin 25
                Grid.children [
                    StackPanel.create [
                        StackPanel.verticalAlignment VerticalAlignment.Center
                        StackPanel.children [
                            let logo
                                = new Avalonia.Media.Imaging.Bitmap("Assets/wpilib-128.png")

                            Image.create [
                                Image.source logo
                                //
                                Image.stretch Avalonia.Media.Stretch.None
                            ]

                            TextBlock.create [
                                TextBlock.text "Welcome to the WPILib Installer Downloader!"
                                //
                                TextBlock.fontSize 25
                                TextBlock.verticalAlignment VerticalAlignment.Center
                                TextBlock.horizontalAlignment HorizontalAlignment.Center
                            ]
                        ]
                    ]
                    Button.create [
                        Button.content "Next"
                        Button.onClick (fun _ -> StateStore.shared.Screen.Set(DownloadScreen))
                        //
                        Grid.row 1
                        Button.fontSize 25
                        Button.horizontalAlignment HorizontalAlignment.Right
                        Button.margin 25
                    ]
                ]
                
            ]

        )

    let downloadView () =
        let currentPlatform = 
            let os =
                [OSPlatform.Windows; OSPlatform.Linux; OSPlatform.OSX]
                |> List.tryFind (fun x -> RuntimeInformation.IsOSPlatform x)
            match (os, RuntimeInformation.OSArchitecture) with
            | (Some os, Architecture.X64) when os = OSPlatform.Linux -> Some Linux64
            | (Some os, Architecture.X64) when os = OSPlatform.OSX -> Some Mac64
            | (Some os, _) when os = OSPlatform.OSX -> Some MacArm64
            | (Some os, _) when os = OSPlatform.Windows -> Some Win64
            | _ -> None
        Component.create ("downloadView", fun ctx ->
            Grid.create [
                Grid.rowDefinitions "*,Auto"
                Grid.margin 25
                Grid.children [
                    match currentPlatform with
                    | None -> TextBlock.create [
                        TextBlock.text "Your platform is not supported."
                        //
                        TextBlock.fontSize 40
                        TextBlock.foreground "red"
                        TextBlock.verticalAlignment VerticalAlignment.Center
                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                        ]
                    | Some plat ->
                        Button.create [
                            Button.content $"Download"
                            ToolTip.tip $"Download for {plat}"
                            Grid.row 0
                            //
                            Button.fontSize 40
                            Button.horizontalAlignment HorizontalAlignment.Center
                            Button.padding 25
                            Button.verticalAlignment VerticalAlignment.Center
                            Button.horizontalContentAlignment HorizontalAlignment.Center
                            Button.verticalContentAlignment VerticalAlignment.Center
                        ]
                    Button.create [
                        Button.content "Back"
                        Button.onClick (fun _ -> StateStore.shared.Screen.Set(StartScreen))
                        Grid.row 1
                        //
                        Button.fontSize 25
                        Button.horizontalAlignment HorizontalAlignment.Left
                        Button.margin 25
                    ]

                    Button.create [
                        Button.content "custom download"
                        Button.onClick (fun _ -> StateStore.shared.Screen.Set(CustomDownloadScreen))
                        Grid.row 1
                        //
                        Button.fontSize 8
                        Button.horizontalAlignment HorizontalAlignment.Center
                        Button.margin 25
                    ]
                    // Button.create [
                    //     Button.content "Next"
                    //     Button.onClick (fun _ -> StateStore.shared.Screen.Set(DownloadScreen))
                    //     //
                    //     Grid.row 1
                    //     Button.fontSize 25
                    //     Button.horizontalAlignment HorizontalAlignment.Right
                    //     Button.margin 25
                    // ]
                ]
                
            ]

        )


    type ButtonState =
        | Downloading of DownloadProgress
        | Downloaded
        | NotDownloaded


    let customDownloadView () =
        let enumerate source = Seq.mapi (fun i x -> i,x) source
        let mutable skip: Set<Package*Platform> = Set []
        let columnPlatforms (pack: Package) (plat_idx: int) =
            let plat = Platform.all.[plat_idx]
            let curr = Downloads.downloads.TryFind (pack, plat)
            Seq.takeWhile
                (fun (next) ->
                    match (curr, Downloads.downloads.TryFind (pack, next)) with
                    | (None, None) -> true
                    | (Some x, Some y) -> x.Url = y.Url
                    | _ -> false)
                Platform.all.[plat_idx..]
                |> Seq.map
                    (fun plat ->
                        skip <- skip.Add (pack, plat)
                        plat
                    )
        let mutable downloadFuns4Platform = Platform.all |> Seq.map (fun plat -> plat, []) |> Map
        let mutable downloadFuns4Package = Package.all |> Seq.map (fun plat -> plat, []) |> Map
        let mutable downloadFuns4Everything = []

        Component.create ("customDownloadView", fun ctx ->
            Grid.create [
                Grid.rowDefinitions ( seq {0..Package.all.Length+1} |> Seq.map (fun _ -> "Auto") |> String.concat ",")
                Grid.columnDefinitions (seq {0..Platform.all.Length+1} |> Seq.map (fun _ -> "Auto") |> String.concat ",")
                //
                // Grid.showGridLines true
                Grid.verticalAlignment VerticalAlignment.Center
                Grid.horizontalAlignment HorizontalAlignment.Center
                Grid.children [

                    Button.create [
                        Button.content "everything"
                        Button.onClick (fun _ ->
                            for f in downloadFuns4Everything do
                                f ()
                        )
                        Grid.row 0
                        Grid.column 0
                        //
                        ToolTip.tip ($"Download all remaining packages for all platforms")
                        TextBlock.horizontalAlignment HorizontalAlignment.Right
                        TextBlock.verticalAlignment VerticalAlignment.Bottom
                        TextBlock.margin 7
                        Button.fontSize 15
                        Button.fontWeight Avalonia.Media.FontWeight.Bold
                    ]

                    for x, plat in enumerate Platform.all do
                        Button.create [
                            Button.content (TextBlock.create [
                                TextBlock.text <| string plat
                            ])
                            Button.onClick (fun _ ->
                                for f in downloadFuns4Platform.[plat] do
                                    f ()
                            )
                            Grid.row 0
                            Grid.column (x+1)
                            //
                            ToolTip.tip ($"Download all remaining packages for {plat}")
                            TextBlock.horizontalAlignment HorizontalAlignment.Center
                            TextBlock.verticalAlignment VerticalAlignment.Bottom
                            TextBlock.margin 7
                            Button.fontSize 15
                            Button.fontWeight Avalonia.Media.FontWeight.Bold
                        ]
                    
                    for y, pack in enumerate Package.all do
                        Button.create [
                            Button.content (TextBlock.create [
                                TextBlock.text <| string pack
                            ])
                            Button.onClick (fun _ ->
                                for f in downloadFuns4Package.[pack] do
                                    f ()
                            )
                            Grid.row (y+1)
                            Grid.column 0
                            //
                            ToolTip.tip ($"Download {pack} for all remaining platforms")
                            TextBlock.horizontalAlignment HorizontalAlignment.Right
                            TextBlock.verticalAlignment VerticalAlignment.Center
                            TextBlock.margin 7
                            Button.fontSize 15
                            Button.fontWeight Avalonia.Media.FontWeight.Bold
                        ]

                    for plat_idx, plat in enumerate Platform.all do
                        for pack_idx, pack in enumerate Package.all do
                            if skip.Contains (pack, plat) then
                                ()
                            else
                                let column_plats = columnPlatforms pack plat_idx
                                let column_span = column_plats |> Seq.length
                                match Downloads.downloads.TryFind (pack, plat) with
                                | None -> 
                                    Rectangle.create [
                                        //
                                        Grid.columnSpan column_span
                                        Grid.row (pack_idx+1)
                                        Grid.column (plat_idx+1)
                                        Shapes.Shape.fill "gray"
                                        Shapes.Shape.height 2
                                        Shapes.Shape.margin 7
                                        Shapes.Shape.horizontalAlignment HorizontalAlignment.Stretch
                                    ]
                                | Some download -> 
                                    StackPanel.create [
                                        Grid.columnSpan column_span
                                        Grid.row (pack_idx+1)
                                        Grid.column (plat_idx+1)
                                        StackPanel.children [
                                            Component.create (pack.ToString()+plat.ToString(), fun ctx2 ->
                                                let button_state =
                                                    ctx2.useStateLazy <|
                                                        fun _ -> 
                                                            match Downloader.downloadExistsAndValid download with
                                                            | true -> Downloaded
                                                            | false -> NotDownloaded
                                                Button.create [
                                                    match button_state.Current with
                                                        | Downloaded -> "✔"
                                                        | NotDownloaded -> "⬇"
                                                        | Downloading progress -> $"{progress.ProgressPercentage:F2}%%"
                                                    |> Button.content

                                                    Button.onClick
                                                        (fun _ ->

                                                            match button_state.Current with
                                                            | Downloading _ -> ()
                                                            | _ ->
                                                                // printfn "Downloading %A" download
                                                                // printfn "%A" <| Downloader.packageDir download
                                                                Downloader.startDownloading download (
                                                                    fun progress ->
                                                                        if progress.TotalBytesDownloaded = progress.TotalFileSize then
                                                                            button_state.Set (Downloaded)
                                                                        else
                                                                            button_state.Set (Downloading progress)
                                                                    )
                                                        )

                                                    match button_state.Current with
                                                        | NotDownloaded -> $"""Download {pack} for {column_plats |> Seq.map string |> String.concat " | "}"""
                                                        | Downloaded -> $"""Re-download {pack} for {column_plats |> Seq.map string |> String.concat " | "}"""
                                                        | Downloading progress -> $"""Downloading {pack} | {progress.TotalBytesDownloaded / int64 1e6} / {progress.TotalFileSize / int64 1e6} MB | {progress.AvgMbps:F2} Mbps"""
                                                    |> ToolTip.tip

                                                    Button.init (fun button ->
                                                        let clickIfNotDownloaded () = 
                                                            if button_state.Current <> Downloaded then
                                                                button.RaiseEvent (RoutedEventArgs (Button.ClickEvent, button))
                                                        downloadFuns4Everything <- clickIfNotDownloaded::downloadFuns4Everything
                                                        downloadFuns4Platform <- downloadFuns4Platform.Add (plat, clickIfNotDownloaded :: downloadFuns4Platform.[plat])
                                                        downloadFuns4Package <- downloadFuns4Package.Add (pack, clickIfNotDownloaded :: downloadFuns4Package.[pack])
                                                    )

                                                    Button.verticalAlignment VerticalAlignment.Stretch
                                                    Button.horizontalAlignment HorizontalAlignment.Stretch
                                                    Button.horizontalContentAlignment HorizontalAlignment.Center
                                                    Button.verticalContentAlignment VerticalAlignment.Center
                                                    Button.fontSize 15
                                                    Button.margin 7

                                                ]
                                            )
                                        ]
                                    ]
                ]
            ]
        )

    let mainView () =
        Component (fun ctx ->
            let screen = ctx.usePassed StateStore.shared.Screen
            DockPanel.create [ 
                DockPanel.children [
                    match screen.Current with
                    | StartScreen -> startView ()
                    | DownloadScreen -> downloadView ()
                    | CustomDownloadScreen -> customDownloadView ()
                ]
            ]
        )
      
