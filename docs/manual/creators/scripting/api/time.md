# Time Module

:::{contents}
:local:
:depth: 2
:::

The `Time` module provides APIs for reading the game clock. The game clock
is a persisted clock that begins at zero when the server is started for the
first time. The game clock is monotonic (always increases - never goes
backwards) and is persisted across server runs.

The game clock also provides an in-game calendar based on the game clock.
The in-game calendar tracks the time by hour, day, week, month, season,
and year. The length of each of these periods is configurable through
the [client and server configuration files](../../config/index.md).

The clock provides two types of times, absolute and relative. Absolute times
give the number of periods elapsed since the game clock first started. Relative
times give the number of periods within a larger period (e.g. month of year).
In general, absolute times are monotonic, while relative times will repeat
periodically.

It is not possible to set the game clock through a script. The game clock is
intended to be monotonic, and allowing a script to modify the time would
break this guarantee. In exchange for this limitation, the game clock can be
used to accurately and reliably schedule future events (e.g. "replace this
sapling with a full-grown tree after one season").

:::{note}
All numeric time values start at zero (e.g. `GetMonthOfYear` runs from
0 to 11 if there are twelve months in a year).
:::

## Time Functions

### GetSystemTime()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetSystemTime()

   Gets the current system time in microseconds since an arbitrary reference point.
   
   :return: System time in microseconds.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the system time using Time.GetSystemTime.
:emphasize-lines: 1
local systemTime = Time.GetSystemTime()
```

#### Definition

```{eval-rst}
.. lua:function:: Time.FutureSystemTime(delaySeconds)

   Gets a future system time in microseconds since an arbitrary reference point.
   
   :param delaySeconds: Time in seconds to add to the current system time.
   :type delaySeconds: number
   
   :return: Future system time in microseconds.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting a future system time using Time.FutureSystemTime.
:emphasize-lines: 1
local futureTime = Time.FutureSystemTime(1.0) -- system time 1 second in the future
```

### GetAbsoluteTime()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetAbsoluteTime()

   Gets the absolute time in the game world, measured in seconds since the game started.
   
   :return: Absolute time in seconds.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the absolute time using Time.GetAbsoluteTime.
:emphasize-lines: 1
local absoluteTime = Time.GetAbsoluteTime()
```

### GetYear()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetYear()

   Gets the current year in the game world.
   
   :return: Current year.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current year using Time.GetYear.
:emphasize-lines: 1
local year = Time.GetYear()
```

### GetSeason()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetSeason()

   Gets the current season in the game world.
   
   :return: Current season.
   :rtype: string
```

#### Example

```{code-block} lua
:caption: Getting the current season using Time.GetSeason.
:emphasize-lines: 1
local season = Time.GetSeason()
```

### GetMonth()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetMonth()

   Gets the current month in the game world.
   
   :return: Current month.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current month using Time.GetMonth.
:emphasize-lines: 1
local month = Time.GetMonth()
```

### GetMonthOfYear()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetMonthOfYear()

   Gets the current month of the year in the game world.
   
   :return: Current month of the year.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current month of the year using Time.GetMonthOfYear.
:emphasize-lines: 1
local monthOfYear = Time.GetMonthOfYear()
```

### GetWeekOfMonth()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetWeekOfMonth()

   Gets the current week of the month in the game world.
   
   :return: Current week of the month.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current week of the month using Time.GetWeekOfMonth.
:emphasize-lines: 1
local weekOfMonth = Time.GetWeekOfMonth()
```

### GetDayOfMonth()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetDayOfMonth()

   Gets the current day of the month in the game world.
   
   :return: Current day of the month.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current day of the month using Time.GetDayOfMonth.
:emphasize-lines: 1
local dayOfMonth = Time.GetDayOfMonth()
```

### GetWeek()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetWeek()

   Gets the current week in the game world.
   
   :return: Current week.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current week using Time.GetWeek.
:emphasize-lines: 1
local week = Time.GetWeek()
```

### GetDayOfWeek()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetDayOfWeek()

   Gets the current day of the week in the game world.
   
   :return: Current day of the week.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current day of the week using Time.GetDayOfWeek.
:emphasize-lines: 1
local dayOfWeek = Time.GetDayOfWeek()
```

### GetDay()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetDay()

   Gets the current day in the game world.
   
   :return: Current day.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current day using Time.GetDay.
:emphasize-lines: 1
local day = Time.GetDay()
```

### GetHourOfDay()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetHourOfDay()

   Gets the current hour of the day in the game world.
   
   :return: Current hour of the day.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current hour of the day using Time.GetHourOfDay.
:emphasize-lines: 1
local hourOfDay = Time.GetHourOfDay()
```

### GetSecondOfDay()

#### Definition

```{eval-rst}
.. lua:function:: Time.GetSecondOfDay()

   Gets the current second of the day in the game world.
   
   :return: Current second of the day.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current second of the day using Time.GetSecondOfDay.
:emphasize-lines: 1
local secondOfDay = Time.GetSecondOfDay()
```
