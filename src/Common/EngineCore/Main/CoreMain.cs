/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Text;
using System.Threading;
using Castle.Core.Logging;
using Castle.Windsor;
using Sovereign.EngineUtil.IoC;

namespace Sovereign.EngineCore.Main;

/// <summary>
///     Common main class for the engine.
/// </summary>
public class CoreMain
{
    public void RunEngine()
    {
        /* Name the thread. */
        Thread.CurrentThread.Name = "Main";

        /* Start up the IoC container */
        IWindsorContainer iocContainer = null;
        try
        {
            iocContainer = IoCUtil.InitializeIoC();
            var engineBase = iocContainer.Resolve<IEngineBase>();
            engineBase.Run();
        }
        catch (FatalErrorException)
        {
            /* Handled fatal error - halt the engine. */
        }
        catch (Exception e)
        {
            /* Unhandled exception - this usually indicates a fatal error. */
            LogEarlyError("Unhandled exception while running engine.", e, iocContainer);
        }

        /* Shut down the IoC container */
        ShutdownIoC(iocContainer);
    }

    private void ShutdownIoC(IWindsorContainer iocContainer)
    {
        if (iocContainer != null) iocContainer.Dispose();
    }

    /// <summary>
    ///     Attempts to log an unhandled exception that escaped the IoC scope.
    /// </summary>
    /// <param name="message">Message to log.</param>
    /// <param name="e">Exception to log.</param>
    /// <param name="iocContainer">IoC container.</param>
    private void LogEarlyError(string message, Exception e, IWindsorContainer iocContainer)
    {
        /* Attempt to resolve a logger. */
        try
        {
            var logger = iocContainer.Resolve<ILogger>();
            logger.Fatal(message, e);
        }
        catch
        {
            /* Logger not available - fall back on stderr. */
            var sb = new StringBuilder();
            sb.Append("FATAL: ").Append(message).Append("\n")
                .Append(e.GetType().Name).Append(": ").Append(e.Message);
            Console.Error.WriteLine(sb.ToString());
        }
    }
}