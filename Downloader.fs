namespace widf

open System
open System.IO
open System.Net.Http
open System.Threading.Tasks

type DownloadProgress =
      { TotalFileSize: int64
        TotalBytesDownloaded: int64
        ProgressPercentage: float
        AvgMbps: float
    }

module Downloader =

    let packageDir download =
        Path.Combine(
            @"./",
            "WPILibInstallerDownloader",
            string download.Package,
            match download.Hash with
            | None -> "none"
            | _ -> Hash.value download.Hash.Value
        )

    // let hashFile

    let downloadExistsAndValid download = 
        let dir = packageDir download
        // printfn "Path: %s" path
        if Directory.Exists dir then
            let files = Directory.GetFiles (dir, "*", SearchOption.TopDirectoryOnly)
            if files.Length = 1 then
                let path = files.[0]
                match download.Hash with
                | Some (SHA256 hash) ->
                    let sha256 = System.Security.Cryptography.SHA256.Create()
                    use file = File.OpenRead(path)
                    let hashBytes = sha256.ComputeHash(file)
                    let hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower()
                    // printfn "Hash: %s" hashString
                    // printfn "Expected: %s" hash
                    hashString = hash
                // | None -> true
                | None -> false
            else
                false
        else
            false

    // let startDownloading (url: string) (destination: string) (progressCallback: DownloadProgress -> unit) =
    let startDownloading download (progressCallback: DownloadProgress -> unit) =
        task {

            progressCallback
                { TotalFileSize = 0L
                  TotalBytesDownloaded = 1L
                  ProgressPercentage = 0.
                  AvgMbps = 0.
                }

            let url = download.Url
            use client = new HttpClient(Timeout = TimeSpan.FromDays(1.))
            use! response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
            response.EnsureSuccessStatusCode() |> ignore
            let totalBytes = response.Content.Headers.ContentLength
            use! contentStream = response.Content.ReadAsStreamAsync()
            let header_filename_star =
                    try
                        match response.Content.Headers.ContentDisposition.FileNameStar with
                        | null -> None
                        | x -> Some (x.Trim('\"'))
                    with
                    | :? NullReferenceException as ne -> None

            let header_filename =
                    try
                        match response.Content.Headers.ContentDisposition.FileName.Trim('\"') with
                        | null -> None
                        | x -> Some (x.Trim('\"'))
                    with
                    | :? NullReferenceException as ne -> None
            
            let url_filename = 
                System.IO.Path.GetFileName
                    response.RequestMessage.RequestUri.LocalPath
            
            // printfn "URL Filename: %s" url_filename
            // printfn "Header Filename: %A" header_filename
            // printfn "Header Filename Star: %A" header_filename_star

            let filename =
                match header_filename_star with
                
                | Some x ->
                    // let q = printfn "using header_filename_star"
                    x
                | None ->
                    match header_filename with
                    | Some x ->
                        // let q = printfn "using header_filename"
                        x
                    | None ->
                        // let q = printfn "using url_filename"
                        url_filename
            
            // printfn "Filename: %s" filename
            // exit 0
            // printfn "Total bytes: %d" totalBytes.Value

            let mutable totalBytesRead = 0L
            let mutable readCount = 0L
            let buffer = Array.zeroCreate<byte> 0xFFFF
            let mutable isMoreToRead = true
            let startTime = DateTime.Now

            let destinationDir = packageDir download
            let destination = Path.Combine (destinationDir, filename)
            
            // let destination = packagePath download



            if Directory.Exists(destinationDir) then
                for file in Directory.GetFiles(destinationDir, "*", SearchOption.TopDirectoryOnly) do
                    File.Delete(file)

            let _ = Directory.CreateDirectory(destinationDir);

            use fileStream =
                new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 0x1000, true)

            // printfn "Starting download"

            while isMoreToRead = true do
                let! bytesRead = contentStream.ReadAsync(buffer, 0, buffer.Length)
                // printfn "Bytes read: %d" bytesRead
                match bytesRead with
                | 0 ->
                    isMoreToRead <- false
                | _ ->
                    do! fileStream.WriteAsync(buffer, 0, bytesRead)
                    totalBytesRead <- totalBytesRead + int64 bytesRead
                    readCount <- readCount + 1L

                if (not isMoreToRead) || readCount % 100L = 0L then
                    progressCallback
                        { TotalFileSize = totalBytes.Value
                          TotalBytesDownloaded = totalBytesRead
                          ProgressPercentage = (float totalBytesRead / float totalBytes.Value) * 100.
                          AvgMbps = 8. * (float totalBytesRead / (DateTime.Now - startTime).TotalSeconds) / 1000000.
                        }

        }
        |> ignore

        // ()
