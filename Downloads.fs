namespace widf

open System.ComponentModel

type Platform = 
    | Win64
    | Linux64
    | Mac64
    | MacArm64

    static member displayName (this: Platform) =
        match this with
        | Win64 -> "Windows x64"
        | Linux64 -> "linux x64"
        | Mac64 -> "macOS Intel x64"
        | MacArm64 -> "macOS Apple Silicon"
        // | _ -> this.ToString()

    static member all = [
        // Win64;
        // Linux64;
        // Mac64;
        MacArm64;
    ]

type Package = 
    | JavaJDK
    | Toolchain
    | VSCode
    // | Python
    | VSCodeExtension_cpptools
    | VSCodeExtension_wpilib
    | VSCodeExtension_wpilib_utility
    | VSCodeExtension_java
    | VSCodeExtension_javadependency
    | VSCodeExtension_javadebug

    static member all = [
        // JavaJDK
        // Toolchain
        VSCode
        // VSCodeExtension_cpptools
        // VSCodeExtension_wpilib
        // VSCodeExtension_wpilib_utility
        // VSCodeExtension_java
        // VSCodeExtension_javadependency
        // VSCodeExtension_javadebug
    ]


type Hash = 
    | SHA256 of string

    static member value (this: Hash) =
        match this with
        | SHA256 x -> x

type Download = 
    { Package: Package
      Platform: Platform
      Url: string
      Hash: Hash option}


module Downloads = 
    let downloads =
        [
            let vscode_version = "1.74.2"
            { Package = VSCode
              Platform = Win64
              Url = $"https://update.code.visualstudio.com/{vscode_version}/win32-x64-archive/stable"
              Hash = None }

            { Package = VSCode
              Platform = Linux64
              Url = $"https://update.code.visualstudio.com/{vscode_version}/linux-x64/stable"
              Hash = None }
            
            for plat in [Mac64; MacArm64] do
                { Package = VSCode
                  Platform = plat
                  Url = $"https://update.code.visualstudio.com/{vscode_version}/darwin-universal/stable"
                  Hash = Some <| SHA256 "53761b208bc7646ba661a6330aed1a030fd1ee06088c47177e3d731135c99909" }

            let cpptools_version = "1.13.8"

            { Package = VSCodeExtension_cpptools
              Platform = Win64
              Url = $"https://github.com/Microsoft/vscode-cpptools/releases/download/v{cpptools_version}/cpptools-win64.vsix"
              Hash = None }
            
            { Package = VSCodeExtension_cpptools
              Platform = Mac64
              Url = $"https://github.com/Microsoft/vscode-cpptools/releases/download/v{cpptools_version}/cpptools-osx.vsix"
              Hash = None }

            { Package = VSCodeExtension_cpptools
              Platform = MacArm64
              Url = $"https://github.com/Microsoft/vscode-cpptools/releases/download/v{cpptools_version}/cpptools-osx-arm64.vsix"
              Hash = None }

            { Package = VSCodeExtension_cpptools
              Platform = Linux64
              Url = $"https://github.com/Microsoft/vscode-cpptools/releases/download/v{cpptools_version}/cpptools-linux.vsix"
              Hash = None }
            
            let wpilib_version = "2023.4.3"

            for plat in Platform.all do
                { Package = VSCodeExtension_wpilib
                  Platform = plat
                  Url = $"https://github.com/wpilibsuite/vscode-wpilib/releases/download/v{wpilib_version}/vscode-wpilib-{wpilib_version}.vsix"
                  Hash = None }

            { Package = VSCodeExtension_wpilib_utility
              Platform = Win64
              Url = $"https://github.com/wpilibsuite/vscode-wpilib/releases/download/v${wpilib_version}/wpilibutility-windows.zip"
              Hash = None }

            { Package = VSCodeExtension_wpilib_utility
              Platform = Linux64
              Url = $"https://github.com/wpilibsuite/vscode-wpilib/releases/download/v${wpilib_version}/wpilibutility-linux.tar.gz"
              Hash = None }
            
            for plat in [Mac64; MacArm64] do
                { Package = VSCodeExtension_wpilib_utility
                  Platform = plat
                  Url = $"https://github.com/wpilibsuite/vscode-wpilib/releases/download/v${wpilib_version}/wpilibutility-mac.tar.gz"
                  Hash = None }

            for plat in Platform.all do
                let version = "1.14.0"
                { Package = VSCodeExtension_java
                  Platform = plat
                  Url = $"https://github.com/redhat-developer/vscode-java/releases/download/v${version}/redhat.java-${version}.vsix"
                  Hash = None }

            for plat in Platform.all do
                let version = "0.21.1"
                { Package = VSCodeExtension_javadependency
                  Platform = plat
                  Url = $"https://github.com/microsoft/vscode-java-dependency/releases/download/${version}/vscjava.vscode-java-dependency-${version}.vsix"
                  Hash = None }
            
            for plat in Platform.all do
                let version = "0.47.0"
                { Package = VSCodeExtension_javadebug
                  Platform = plat
                  Url = $"https://github.com/microsoft/vscode-java-debug/releases/download/${version}/vscjava.vscode-java-debug-${version}.vsix"
                  Hash = None }
            
            let jdk_version = "17.0.5+8"
            let jdk_version_underscore = jdk_version.Replace("+", "_")
            let jdk_version_escaped = jdk_version.Replace("+", "%2B")

            { Package = JavaJDK
              Platform = Win64
              Url = $"https://github.com/adoptium/temurin17-binaries/releases/download/jdk-{jdk_version_escaped}/OpenJDK17U-jdk_x64_windows_hotspot_${jdk_version_underscore}.zip"
              Hash = None }
            
            { Package = JavaJDK
              Platform = Linux64
              Url = $"https://github.com/adoptium/temurin17-binaries/releases/download/jdk-{jdk_version_escaped}/OpenJDK17U-jdk_x64_linux_hotspot_${jdk_version_underscore}.tar.gz"
              Hash = None }
            
            { Package = JavaJDK
              Platform = Mac64
              Url = $"https://github.com/adoptium/temurin17-binaries/releases/download/jdk-{jdk_version_escaped}/OpenJDK17U-jdk_x64_mac_hotspot_${jdk_version_underscore}.tar.gz"
              Hash = None }

            { Package = JavaJDK
              Platform = MacArm64
              Url = $"https://github.com/adoptium/temurin17-binaries/releases/download/jdk-{jdk_version_escaped}/OpenJDK17U-jdk_aarch64_mac_hotspot_${jdk_version_underscore}.tar.gz"
              Hash = None }
            
            let gcc_version = "12.1.0"
            let toolchain_version = "v2023-7"

            let toolchain_base_url = $"https://github.com/wpilibsuite/opensdk/releases/download/{toolchain_version}/"

            { Package = Toolchain
              Platform = Win64
              Url = toolchain_base_url + $"cortexa9_vfpv3-roborio-academic-2023-x86_64-w64-mingw32-Toolchain-{gcc_version}.zip"
              Hash = None }
            
            { Package = Toolchain
              Platform = Linux64
              Url = toolchain_base_url + $"cortexa9_vfpv3-roborio-academic-2023-x86_64-linux-gnu-Toolchain-{gcc_version}.tgz"
              Hash = None }
            
            { Package = Toolchain
              Platform = Mac64
              Url = toolchain_base_url + $"cortexa9_vfpv3-roborio-academic-2023-x86_64-apple-darwin-Toolchain-{gcc_version}.tar.gz"
              Hash = None }

            { Package = Toolchain
              Platform = MacArm64
              Url = toolchain_base_url + $"cortexa9_vfpv3-roborio-academic-2023-arm64-apple-darwin-Toolchain-{gcc_version}.tar.gz"
              Hash = None }

        ] |> Seq.map (fun (d) -> (d.Package, d.Platform), d) |> Map
