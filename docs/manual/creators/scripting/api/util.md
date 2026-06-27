# Util Module

The `Util` global module provides common utility functions for scripts.

## Logging Functions

### LogInfo(message)

#### Definition

```{eval-rst}
.. lua:function:: Util.LogInfo(message)

    Writes an informational message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the Util.LogInfo(message) function.
:emphasize-lines: 1
Util.LogInfo("Hello World!")
```

### LogWarn(message)

#### Definition

```{eval-rst}
.. lua:function:: Util.LogWarn(message)

    Writes a warning message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the Util.LogWarn(message) function.
:emphasize-lines: 1
Util.LogWarn("Hello World!")
```

(script-util-logerror)=
### LogError(message)

#### Definition

```{eval-rst}
.. lua:function:: Util.LogError(message)

    Writes an error message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the Util.LogError(message) function.
:emphasize-lines: 1
Util.LogError("Hello World!")
```

### LogCrit(message)

#### Definition

```{eval-rst}
.. lua:function:: Util.LogCrit(message)

    Writes a critical/fatal error message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the Util.LogCrit(message) function.
:emphasize-lines: 1
Util.LogCrit("Hello World!")
```

### LogDebug(message)

#### Definition

```{eval-rst}
.. lua:function:: Util.LogDebug(message)

    Writes a debug message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the Util.LogDebug(message) function.
:emphasize-lines: 1
Util.LogDebug("Hello World!")
```

### LogTrace(message)

#### Definition

```{eval-rst}
.. lua:function:: Util.LogTrace(message)

    Writes a trace debug message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the Util.LogTrace(message) function.
:emphasize-lines: 1
Util.LogTrace("Hello World!")
```

### ToBool(value)

#### Definition

```{eval-rst}
.. lua:function:: Util.ToBool(value)

    Tries to convert the given string to a boolean.

    :param value: String to convert to boolean.
    :type value: string

    :return: Converted boolean value, or `nil` if not convertible.
    :rtype: boolean
```

#### Example

```{code-block} lua
:caption: Converting a string to boolean using the Util.ToBool(value) function.
:emphasize-lines: 1,2,3
Util.ToBool("True")          -- returns true
Util.ToBool("False")         -- returns false
Util.ToBool("Hello World!")  -- returns nil
```
