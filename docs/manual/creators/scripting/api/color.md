(script-color)=
# Color Module

## Predefined Colors

The `Color` module provides two sets of predefined color constants:
color constants by color, and color constants by purpose.

The predefined color constants by color (represented here as HTML color
codes) are as follows:

|Constant     |Color  |
|-------------|-------|
|`Color.WHITE`|#FFFFFF|
|`Color.BLACK`|#000000|
|`Color.RED`  |#FF0000|
|`Color.GREEN`|#00FF00|
|`Color.BLUE` |#0000FF|

The predefined color constants by purpose are as follows:

|Constant           |Color  |Purpose                                       |
|-------------------|-------|----------------------------------------------|
|`Color.MOTD`       |#FFFFFF|Welcome messages displayed to player at login.|
|`Color.ALERT`      |#D20000|Alerts and error messages displayed in chat.  |
|`Color.CHAT_LOCAL` |#B3B3B3|Local chat messages.                          |
|`Color.CHAT_GLOBAL`|#FFFFFF|Global chat messages.                         |
|`Color.CHAT_SYSTEM`|#808080|System chat messages.                         |

## Color Functions

### Rgb(r, g, b)

#### Definition

```{eval-rst}
.. lua:function:: Color.Rgb(r, g, b)

   Creates a color with the given red, green, and blue components.
   The alpha component is set to 255.
   
   :param r: Red component.
   :type r: integer
   :param g: Green component.
   :type g: integer
   :param b: Blue component.
   :type b: integer
   
   :return: Color.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Using `Rgb(r, g, b)` to define a color.
:emphasize-lines: 1
local white = Color.Rgb(255, 255, 255)
```

### Rgba(r, g, b, a)

#### Definition

```{eval-rst}
.. lua:function:: Color.Rgba(r, g, b, a)

   Creates a color with the given red, green, blue, and alpha components.
   
   :param r: Red component.
   :type r: integer
   :param g: Green component.
   :type g: integer
   :param b: Blue component.
   :type b: integer
   :param a: Alpha component.
   :type a: integer
   
   :return: Color.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Using `Rgba(r, g, b, a)` to define a color.
:emphasize-lines: 1
local transparentRed = Color.Rgba(255, 0, 0, 160)
```
