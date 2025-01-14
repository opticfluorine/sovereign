# util Module

The `util` global module provides common utility functions for scripts.

## Logging Functions

### LogInfo(message)

#### Definition

```{eval-rst}
.. lua:function:: util.LogInfo(message)

    Writes an informational message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the util.LogInfo(message) function.
:emphasize-lines: 1
util.LogInfo("Hello World!")
```

### LogWarn(message)

#### Definition

```{eval-rst}
.. lua:function:: util.LogWarn(message)

    Writes a warning message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the util.LogWarn(message) function.
:emphasize-lines: 1
util.LogWarn("Hello World!")
```

(script-util-logerror)=
### LogError(message)

#### Definition

```{eval-rst}
.. lua:function:: util.LogError(message)

    Writes an error message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the util.LogError(message) function.
:emphasize-lines: 1
util.LogError("Hello World!")
```

### LogCrit(message)

#### Definition

```{eval-rst}
.. lua:function:: util.LogCrit(message)

    Writes a critical/fatal error message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the util.LogCrit(message) function.
:emphasize-lines: 1
util.LogCrit("Hello World!")
```

### LogDebug(message)

#### Definition

```{eval-rst}
.. lua:function:: util.LogDebug(message)

    Writes a debug message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the util.LogDebug(message) function.
:emphasize-lines: 1
util.LogDebug("Hello World!")
```

### LogTrace(message)

#### Definition

```{eval-rst}
.. lua:function:: util.LogTrace(message)

    Writes a trace debug message to the log.
    
    :param message: Message to write to the log.
    :type message: string
```

#### Example

```{code-block} lua
:caption: Logging using the util.LogTrace(message) function.
:emphasize-lines: 1
util.LogTrace("Hello World!")
```