/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.EngineCore.Logging;
using System;
using System.Windows.Forms;

namespace Sovereign.ClientCore.Logging
{

    /// <summary>
    /// Utility class that provides methods for reporting errors and
    /// fatal errors to the user.
    /// </summary>
    public class ErrorHandler : IErrorHandler
    {

        private static readonly string CAPTION = "Error";

        public void Error(string message)
        {
            MessageBox.Show(null, message, CAPTION,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

    }

}
