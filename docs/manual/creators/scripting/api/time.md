# time Module

:::{contents}
:local:
:depth: 2
:::

The `time` module provides APIs for reading the game clock. The game clock
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

### GetAbsoluteTime()

#### Definition

```{eval-rst}
.. lua:function:: time.GetAbsoluteTime()

   Gets the absolute time in the game world, measured in seconds since the game started.
   
   :return: Absolute time in seconds.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the absolute time using time.GetAbsoluteTime.
:emphasize-lines: 1
local absoluteTime = time.GetAbsoluteTime()
```

### GetYear()

#### Definition

```{eval-rst}
.. lua:function:: time.GetYear()

   Gets the current year in the game world.
   
   :return: Current year.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current year using time.GetYear.
:emphasize-lines: 1
local year = time.GetYear()
```

### GetSeason()

#### Definition

```{eval-rst}
.. lua:function:: time.GetSeason()

   Gets the current season in the game world.
   
   :return: Current season.
   :rtype: string
```

#### Example

```{code-block} lua
:caption: Getting the current season using time.GetSeason.
:emphasize-lines: 1
local season = time.GetSeason()
```

### GetMonth()

#### Definition

```{eval-rst}
.. lua:function:: time.GetMonth()

   Gets the current month in the game world.
   
   :return: Current month.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current month using time.GetMonth.
:emphasize-lines: 1
local month = time.GetMonth()
```

### GetMonthOfYear()

#### Definition

```{eval-rst}
.. lua:function:: time.GetMonthOfYear()

   Gets the current month of the year in the game world.
   
   :return: Current month of the year.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current month of the year using time.GetMonthOfYear.
:emphasize-lines: 1
local monthOfYear = time.GetMonthOfYear()
```

### GetWeekOfMonth()

#### Definition

```{eval-rst}
.. lua:function:: time.GetWeekOfMonth()

   Gets the current week of the month in the game world.
   
   :return: Current week of the month.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current week of the month using time.GetWeekOfMonth.
:emphasize-lines: 1
local weekOfMonth = time.GetWeekOfMonth()
```

### GetDayOfMonth()

#### Definition

```{eval-rst}
.. lua:function:: time.GetDayOfMonth()

   Gets the current day of the month in the game world.
   
   :return: Current day of the month.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current day of the month using time.GetDayOfMonth.
:emphasize-lines: 1
local dayOfMonth = time.GetDayOfMonth()
```

### GetWeek()

#### Definition

```{eval-rst}
.. lua:function:: time.GetWeek()

   Gets the current week in the game world.
   
   :return: Current week.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current week using time.GetWeek.
:emphasize-lines: 1
local week = time.GetWeek()
```

### GetDayOfWeek()

#### Definition

```{eval-rst}
.. lua:function:: time.GetDayOfWeek()

   Gets the current day of the week in the game world.
   
   :return: Current day of the week.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current day of the week using time.GetDayOfWeek.
:emphasize-lines: 1
local dayOfWeek = time.GetDayOfWeek()
```

### GetDay()

#### Definition

```{eval-rst}
.. lua:function:: time.GetDay()

   Gets the current day in the game world.
   
   :return: Current day.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current day using time.GetDay.
:emphasize-lines: 1
local day = time.GetDay()
```

### GetHourOfDay()

#### Definition

```{eval-rst}
.. lua:function:: time.GetHourOfDay()

   Gets the current hour of the day in the game world.
   
   :return: Current hour of the day.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current hour of the day using time.GetHourOfDay.
:emphasize-lines: 1
local hourOfDay = time.GetHourOfDay()
```

### GetSecondOfDay()

#### Definition

```{eval-rst}
.. lua:function:: time.GetSecondOfDay()

   Gets the current second of the day in the game world.
   
   :return: Current second of the day.
   :rtype: integer
```

#### Example

```{code-block} lua
:caption: Getting the current second of the day using time.GetSecondOfDay.
:emphasize-lines: 1
local secondOfDay = time.GetSecondOfDay()
```
