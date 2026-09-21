// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System;
using System.Collections.Generic;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Systems.DebugInterface.Protocol;
using Sovereign.EngineCore.Network;
using Xunit;

namespace TestClientCore.Protocol;

/// <summary>
///     Unit tests for the MessagePack serialization of the debug interface protocol.
/// </summary>
public class TestDebugProtocol
{
    [Fact]
    public void RoundTripsRequestWithoutDetails()
    {
        var request = new DebugRequest
        {
            Type = DebugRequestType.GetInputState,
            RequestId = 42
        };

        var roundTripped = MessageConfig.DeserializeMsgPack<DebugRequest>(
            MessageConfig.SerializeMsgPack(request));

        Assert.NotNull(roundTripped);
        Assert.Equal(DebugRequestType.GetInputState, roundTripped.Type);
        Assert.Equal(42u, roundTripped.RequestId);
        Assert.Null(roundTripped.Details);
    }

    [Fact]
    public void RoundTripsKeyEventDetails()
    {
        var request = new DebugRequest
        {
            Type = DebugRequestType.SendKeyEvent,
            RequestId = 7,
            Details = new KeyEventRequestDetails
            {
                Keycode = (int)SDL2.SDL.SDL_Keycode.SDLK_w,
                Modifier = 0x0040,
                IsDown = true
            }
        };

        var roundTripped = MessageConfig.DeserializeMsgPack<DebugRequest>(
            MessageConfig.SerializeMsgPack(request));

        var details = Assert.IsType<KeyEventRequestDetails>(roundTripped.Details);
        Assert.Equal((int)SDL2.SDL.SDL_Keycode.SDLK_w, details.Keycode);
        Assert.Equal(0x0040, details.Modifier);
        Assert.True(details.IsDown);
    }

    [Fact]
    public void RoundTripsMouseEventDetails()
    {
        var request = new DebugRequest
        {
            Type = DebugRequestType.SendMouseEvent,
            RequestId = 9,
            Details = new MouseEventRequestDetails
            {
                EventType = DebugMouseEventType.Wheel,
                X = 11f,
                Y = 22f,
                Button = 3,
                IsDown = true,
                Dx = -1.5f,
                Dy = 2.5f
            }
        };

        var roundTripped = MessageConfig.DeserializeMsgPack<DebugRequest>(
            MessageConfig.SerializeMsgPack(request));

        var details = Assert.IsType<MouseEventRequestDetails>(roundTripped.Details);
        Assert.Equal(DebugMouseEventType.Wheel, details.EventType);
        Assert.Equal(11f, details.X);
        Assert.Equal(22f, details.Y);
        Assert.Equal(3, details.Button);
        Assert.True(details.IsDown);
        Assert.Equal(-1.5f, details.Dx);
        Assert.Equal(2.5f, details.Dy);
    }

    [Fact]
    public void RoundTripsEmptyResponse()
    {
        var response = new DebugResponse
        {
            RequestId = 13,
            Status = DebugResponseStatus.Ok,
            Details = new EmptyResponseDetails()
        };

        var roundTripped = MessageConfig.DeserializeMsgPack<DebugResponse>(
            MessageConfig.SerializeMsgPack(response));

        Assert.NotNull(roundTripped);
        Assert.Equal(13u, roundTripped.RequestId);
        Assert.Equal(DebugResponseStatus.Ok, roundTripped.Status);
        Assert.Null(roundTripped.ErrorMessage);
        Assert.IsType<EmptyResponseDetails>(roundTripped.Details);
    }

    [Fact]
    public void RoundTripsErrorResponse()
    {
        var response = new DebugResponse
        {
            RequestId = 14,
            Status = DebugResponseStatus.Error,
            ErrorMessage = "Something went wrong."
        };

        var roundTripped = MessageConfig.DeserializeMsgPack<DebugResponse>(
            MessageConfig.SerializeMsgPack(response));

        Assert.NotNull(roundTripped);
        Assert.Equal(DebugResponseStatus.Error, roundTripped.Status);
        Assert.Equal("Something went wrong.", roundTripped.ErrorMessage);
        Assert.Null(roundTripped.Details);
    }

    [Fact]
    public void RoundTripsInputStateResponse()
    {
        var response = new DebugResponse
        {
            RequestId = 15,
            Status = DebugResponseStatus.Ok,
            Details = new InputStateResponseDetails
            {
                PressedKeys = new List<int> { 119, 115 },
                MouseX = 320.5f,
                MouseY = 240.25f,
                LeftDown = true,
                MiddleDown = false,
                RightDown = true,
                TotalScrollAmount = -4.5f
            }
        };

        var roundTripped = MessageConfig.DeserializeMsgPack<DebugResponse>(
            MessageConfig.SerializeMsgPack(response));

        var details = Assert.IsType<InputStateResponseDetails>(roundTripped.Details);
        Assert.Equal(new List<int> { 119, 115 }, details.PressedKeys);
        Assert.Equal(320.5f, details.MouseX);
        Assert.Equal(240.25f, details.MouseY);
        Assert.True(details.LeftDown);
        Assert.False(details.MiddleDown);
        Assert.True(details.RightDown);
        Assert.Equal(-4.5f, details.TotalScrollAmount);
    }

    [Fact]
    public void RoundTripsScreenshotResponseWithLargePixelArray()
    {
        var pixels = new byte[1 << 20];
        new Random(1234).NextBytes(pixels);
        var response = new DebugResponse
        {
            RequestId = 16,
            Status = DebugResponseStatus.Ok,
            Details = new ScreenshotResponseDetails
            {
                Width = 512,
                Height = 512,
                Format = CapturedPixelFormat.Bgra8Srgb,
                Pixels = pixels
            }
        };

        var roundTripped = MessageConfig.DeserializeMsgPack<DebugResponse>(
            MessageConfig.SerializeMsgPack(response));

        var details = Assert.IsType<ScreenshotResponseDetails>(roundTripped.Details);
        Assert.Equal(512, details.Width);
        Assert.Equal(512, details.Height);
        Assert.Equal(CapturedPixelFormat.Bgra8Srgb, details.Format);
        Assert.Equal(pixels.Length, details.Pixels.Length);
        Assert.Equal(pixels, details.Pixels);
    }
}
