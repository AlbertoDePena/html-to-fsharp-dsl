namespace FSharp.Html.Dsl.BrowserTests

open System
open Microsoft.Playwright
open Xunit

/// A shared Chromium instance for the browser tests.
///
/// Installing the browser is a large download on first run, so when it cannot
/// be obtained (offline, or a sandbox that blocks the download) the tests skip
/// with the reason rather than failing - `dotnet test` stays green for anyone
/// who only wants the unit suite.
type BrowserFixture() =
    let mutable playwright : IPlaywright = null
    let mutable browser : IBrowser = null
    let mutable skipReason : string = null

    /// `null` when Chromium is usable, otherwise why it is not.
    member _.SkipReason = skipReason

    /// A fresh page with its own JavaScript context, so `window` probes set by
    /// one test cannot leak into another.
    member _.NewPageAsync() =
        if isNull browser then
            failwith "Chromium is unavailable; the test should have skipped."
        browser.NewPageAsync()

    interface IAsyncLifetime with
        member _.InitializeAsync() =
            task {
                try
                    // The programmatic equivalent of `playwright.ps1 install
                    // chromium`, so running the suite needs no PowerShell step.
                    let exitCode = Microsoft.Playwright.Program.Main [| "install"; "chromium" |]
                    if exitCode <> 0 then
                        skipReason <- sprintf "playwright install chromium exited with %i" exitCode
                    else
                        let! pw = Playwright.CreateAsync()
                        playwright <- pw
                        let! b = pw.Chromium.LaunchAsync(BrowserTypeLaunchOptions(Headless = true))
                        browser <- b
                with ex ->
                    skipReason <- sprintf "Chromium unavailable: %s" ex.Message
            }
            :> System.Threading.Tasks.Task

        member _.DisposeAsync() =
            task {
                if not (isNull browser) then
                    do! browser.CloseAsync()
                if not (isNull playwright) then
                    playwright.Dispose()
            }
            :> System.Threading.Tasks.Task
