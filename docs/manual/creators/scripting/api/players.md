# Players Module

The `Players` module provides APIs for looking up online players by name.

Player lookups are case-insensitive. Exact lookups are O(1); fuzzy lookups
scan all online player names and return the top-N matches ordered by
descending similarity score (ties broken alphabetically).

## FindByName(name)

### Definition

```{eval-rst}
.. lua:function:: Players.FindByName(name)

   Looks up an online player entity by name (case-insensitive, O(1)).

   :param name: Player name.
   :type name: string
   :return: Player entity ID, or entity ID 0 (``Entities.ToEntityId(0)``) if no online player has the given name.
   :rtype: lightuserdata
```

### Example

```{code-block} lua
:caption: Looking up an online player by name using Players.FindByName.
:emphasize-lines: 1
local playerId = Players.FindByName("Alice")
```

## FindByFuzzyName(name, maxResults)

### Definition

```{eval-rst}
.. lua:function:: Players.FindByFuzzyName(name, maxResults)

   Performs a fuzzy lookup of online players by name, returning the top-N
   matches ordered by descending similarity score (ties broken
   alphabetically).

   :param name: Query name.
   :type name: string
   :param maxResults: Maximum number of results. Clamped to [1, 64].
   :type maxResults: integer
   :return: Up to ``maxResults`` matches, best-first, as a list of
            `PlayerNameMatch <#script-types-playernamematch>`_ tables.
   :rtype: table
```

### Example

```{code-block} lua
:caption: Fuzzy lookup of online players using Players.FindByFuzzyName.
:emphasize-lines: 1,2,3
for _, match in ipairs(Players.FindByFuzzyName("Alce", 5)) do
    Util.LogInfo(string.format("%s (id %s, score %.2f)",
        match.Name, Entities.FormatEntityId(match.EntityId), match.Score))
end
```
