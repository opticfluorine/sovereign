// Sovereign Engine
// Copyright (c) 2025 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sovereign.IntegrationTests.Fixtures;

/// <summary>
///     Provides a test fixture that runs a copy of the Sovereign Engine server.
/// </summary>
public sealed class ServerFixture : IDisposable
{
    private readonly Process serverProcess;
    private readonly string tempDir;

    public ServerFixture()
    {
        // Determine build configuration and framework from test context
        var testDir = AppContext.BaseDirectory;
        // Find the configuration and framework from the test's own build path
        var testBinDir = Directory.GetParent(testDir)?.FullName;
        if (testBinDir == null)
            throw new InvalidOperationException("Could not determine test bin directory.");
        var config = Path.GetFileName(Directory.GetParent(testBinDir)?.FullName ?? "");
        var framework = Path.GetFileName(testBinDir);
        // Build path to SovereignServer output
        var solutionDir = Directory.GetParent(testBinDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
        if (solutionDir == null)
            throw new InvalidOperationException("Could not determine solution directory.");
        var serverBuildDir = Path.Combine(solutionDir, "Server", "SovereignServer", "bin", config, framework);
        if (!Directory.Exists(serverBuildDir))
            throw new DirectoryNotFoundException($"SovereignServer build directory not found: {serverBuildDir}");

        // Create temp directory
        tempDir = Path.Combine(Path.GetTempPath(), "SovereignServerTest_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);

        // Copy files from SovereignServer build dir to temp dir
        CopyDirectory(serverBuildDir, tempDir);

        // Determine executable name
        var serverExeName1 = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "Sovereign.Server.exe"
            : "Sovereign.Server";
        var exePath = Path.Combine(tempDir, serverExeName1);
        if (!File.Exists(exePath))
            throw new FileNotFoundException($"SovereignServer executable not found: {exePath}");

        // Start server process
        serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = tempDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        serverProcess.Start();
        Thread.Sleep(2000);
    }

    public void Dispose()
    {
        try
        {
            if (!serverProcess.HasExited)
            {
                serverProcess.Kill();
                serverProcess.WaitForExit(5000);
            }
        }
        catch
        {
            /* ignore */
        }

        try
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
        catch
        {
            /* ignore */
        }
    }

    /// <summary>
    ///     Recursively copies a directory and its contents.
    /// </summary>
    /// <param name="sourceDir">Directory to copy.</param>
    /// <param name="destDir">Destination path.</param>
    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var dest = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, dest);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dest = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, dest);
        }
    }
}