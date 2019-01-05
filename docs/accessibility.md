# Sovereign Engine and Accessibility

A game engine should strive to be usable by as many people as possible.
Following this principle, the Sovereign Engine client is designed with
accessibility (A11Y) in mind. This document serves to outline the planned
and implemented accessibility features in the Sovereign Engine client.

The Sovereign Engine developers do not have control over the level of
accessibility provided by games created with the Engine or of forks of
the Sovereign Engine.


## Providing Feedback on Accessibility Features

The Sovereign Engine developers welcome all feedback regarding the
accessibility of the engine, especially from individuals who require
accessibility features or other related support in order to play games built
on the Sovereign Engine.

Feedback on accessibility features may be provided by any of the following
methods, listed in order of preference:

* Opening an issue on the official Sovereign Engine project hosted on Gitlab.
  The current location of the official issues board is
  https://gitlab.com/opticfluorine/engine8/issues. When opening an
  accessibility-related issue, please use the "Accessibility" label.
* Sending an email to the project maintainer at opticfluorine@gmail.com.
  Please include the word "Accessibility" in the subject of the email to
  ensure that the message is given the appropriate attention.


## Accessible User-Configurable Options

**Implementation Status:** *Planned*

The Sovereign Engine client shall store user-configurable options in a text
file stored in a documented location.  This options file shall be created
by the Sovereign Engine client if it does not already exist.  The options
file shall be populated with all available options when first created.

By storing user-configurable options in a text file in a documented location,
the Sovereign Engine client ensures that a user can edit the game options with
any text editor of choice (with any accessibility features that are included
in their text editor of choice).  By populating the file will all available
options by default, the Sovereign Engine client ensures that all options are
documented and accessible through the text file.  Finally, by creating the
file when it does not already exist, the Sovereign Engine client ensures that
a user can obtain the latest set of all available options at any time by
moving or deleting their existing options file.


## Accessible Game Input

**Implementation Status:** *Planned*

The Sovereign Engine client shall allow the user to interact with the game
through the input device of their choice, provided that the device is
supported by the "Joystick Support" or the "Game Controller Support"
features of the SDL2 library (https://libsdl.org).  The mapping of inputs
(e.g. Up button, Right button, etc.) to game actions (e.g. move up, move 
right, etc.) is configurable in the user-configurable options text file.

All game mechanics shall be usable with one of either: keyboard/mouse;
joystick (as supported by SDL2's "Joystick Support"); or game controller
(as supported by SDL2's "Game Controller Support").


## Accessible User Interfaces

**Implementation Status:** *Planned*

The Sovereign Engine client user interfaces shall, by default, be accessible.
"User interfaces" in this context refer to any interaction with the client
that would normally be performed with a keyboard and/or mouse and cannot
be easily performed with a standard game controller. Examples of "user
interfaces" include, but are not limited to:

* Login dialog (requires entering a username and password)
* Chat

Accessibility considerations for specific user interfaces follow.

### Login Dialog

**Implementation Status:** *Planned*

Login shall be accomplished through an OAuth third-party login function that
opens in the default web browser of the computer on which the Sovereign Engine
client is running. This allows the user to use the full range of accessibility
features included in their web browser of choice to interact with the login
dialog.

### Chat

No chat features shall require an action be performed within a fixed amount
of time.

#### Chat Accessibility for the Visually Impaired

**Implementation Status:** *Planned*

The Sovereign Engine client shall provide an Accessible Chat mode that
presents the usual chat controls (message pane, input pane, etc.) in
a dedicated window using native controls instead of the custom rendered
controls used in the main game window. The use of native controls allows
the player to use existing accessibility tools (e.g. the accessibility
tools provided by the operating system) to interact with chat. For
example, a player who is vision-impared could use the operating system's
text-to-speech feature to read the latest chat message from the native
message pane, or a player who is unable to type could use the operating
system's speech-to-text feature to enter and send messages through the
native input pane.

#### Chat Accessibility for the Hearing Impaired

**Implementation Status:** *Planned*

The Sovereign Engine client provides text chat that is usable by the hearing
impaired.


## Accessible Displays

The Sovereign Engine display shall, as much as is possible and reasonable,
be accessible to individuals who are color blind or require high contrast
displays.

### Use of Color

**Implementation Status:** *Implemented*

The Sovereign Engine client shall not have any features which depend solely
on color.  All features and gameplay mechanics implemented in the official
Engine shall be accessible to color blind individuals.

### High Contrast Text

**Implementation Status:** *Planned*

The Sovereign Engine client shall provide a "High Contrast Text" option that,
when enabled, draws a rectangle behind all text on screen with a color chosen
such that the contrast ratio between the text and the rectangle is
sufficiently high.

### Large Text

**Implementation Status:** *Planned*

The Sovereign Engine client shall provide a "Large Text" option that, when
enabled, increases the size of all text on screen to improve visibility.

