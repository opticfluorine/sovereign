// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using Sovereign.EngineCore.Components.Validators;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     Provides validation for create player requests.
/// </summary>
public class CreatePlayerRequestValidator
{
    private readonly NameComponentValidator nameValidator;

    public CreatePlayerRequestValidator(NameComponentValidator nameValidator)
    {
        this.nameValidator = nameValidator;
    }

    /// <summary>
    ///     Checks the validity of a create player request.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool IsValid(CreatePlayerRequest request)
    {
        return nameValidator.IsValid(request.PlayerName);
    }
}