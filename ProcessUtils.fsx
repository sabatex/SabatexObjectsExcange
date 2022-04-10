module Process =
    open System
    open System.Diagnostics

    type OutputLine =
        | StdOut of string
        | StdError of string

    let private addStdOutToList (lst : ResizeArray<OutputLine>) (line : DataReceivedEventArgs) =
        if not (isNull line.Data) then
            lst.Add(StdOut line.Data)

    let private addStdErrorToList (lst : ResizeArray<OutputLine>) (line : DataReceivedEventArgs) =
        if not (isNull line.Data) then
            lst.Add(StdError line.Data)

    /// Runs an external command and returns the result
    ///
    /// **Example**
    ///
    /// > Process.execSync "echo" [| "hello"; "world" |] None;;
    /// val it : int * ResizeArray<Process.OutputLine> =
    ///   (0, seq [StdOut "hello world"])
    ///
    /// > Process.execSync "non-existing" [||] None;;
    /// val it : Result<(int * ResizeArray<Process.OutputLine>),string> =
    ///   Error "Error trying to execute 'non-existing': No such file or directory"
    ///
    let execSync (cmd : string) (args : string[]) (workingDir : string option) =
        let psi = ProcessStartInfo(cmd)
        psi.Arguments <- String.Join(" ", args)
        psi.CreateNoWindow <- true
        psi.UseShellExecute <- false
        psi.RedirectStandardError <- true
        psi.RedirectStandardOutput <- true
        if Option.isSome workingDir then
            psi.WorkingDirectory <- Option.get workingDir

        use proc = new Process()
        proc.StartInfo <- psi
        let output = ResizeArray<OutputLine>()
        proc.OutputDataReceived |> Event.add (addStdOutToList output)
        proc.ErrorDataReceived |> Event.add (addStdErrorToList output)
        try
            proc.Start() |> ignore
            proc.BeginErrorReadLine()
            proc.BeginOutputReadLine()
            proc.WaitForExit()
            Ok (proc.ExitCode, output)
        with e -> Error (sprintf "Error trying to execute '%s': %s" cmd e.Message)