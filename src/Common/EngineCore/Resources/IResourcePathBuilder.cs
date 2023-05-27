﻿/*
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


namespace Sovereign.EngineCore.Resources
{

    /// <summary>
    /// Builds paths to resources.
    /// </summary>
    public interface IResourcePathBuilder
    {

        /// <summary>
        /// Builds the path to the base directory for the given resource type.
        /// </summary>
        /// <param name="resourceType">Resource type.</param>
        /// <returns>Path to the base directory for the given resource type.</returns>
        string GetBaseDirectoryForResource(ResourceType resourceType);

        /// <summary>
        /// Builds the path to the given resource file.
        /// </summary>
        /// <param name="resourceType">Resource type.</param>
        /// <param name="resourceFilename">Name of the resource file.</param>
        /// <returns>Path to the resource file.</returns>
        string BuildPathToResource(ResourceType resourceType, string resourceFilename);

    }

}