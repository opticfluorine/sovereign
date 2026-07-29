# Items Module

The `Items` module provides APIs for looking up item template entities by
name.

Item template name lookups are case-insensitive. Unlike player names, item
template names are **not** required to be unique: a name may map to multiple
item template entity IDs. Exact lookups return all matching entity IDs;
fuzzy lookups select the top-N matching *names* by similarity score and then
expand each name to all of its entity IDs (so the returned list may contain
more entries than `maxResults`).

## FindByName(name)

### Definition

```{eval-rst}
.. lua:function:: Items.FindByName(name)

   Looks up item template entities by name (case-insensitive). Because item
   template names are not unique, all matching entity IDs are returned.

   :param name: Item template name.
   :type name: string
   :return: List of item template entity IDs matching the name (empty if none).
   :rtype: table
```

### Example

```{code-block} lua
:caption: Looking up item template entities by name using Items.FindByName.
:emphasize-lines: 1,2,3
for _, itemId in ipairs(Items.FindByName("Sword")) do
    Util.LogInfo(string.format("Found item template %x", itemId))
end
```

## FindByFuzzyName(name, maxResults)

### Definition

```{eval-rst}
.. lua:function:: Items.FindByFuzzyName(name, maxResults)

   Performs a fuzzy lookup of item template entities by name, returning the
   best matches ordered by descending similarity score (ties broken
   alphabetically). ``maxResults`` caps the number of *distinct names*
   selected; each selected name is expanded to all of its entity IDs, so the
   returned list may contain more than ``maxResults`` entries.

   :param name: Query name.
   :type name: string
   :param maxResults: Maximum number of distinct names to select. Clamped to [1, 64].
   :type maxResults: integer
   :return: Best-first matches as a list of
            `ItemTemplateMatch <#script-types-itemtemplatematch>`_ tables.
   :rtype: table
```

### Example

```{code-block} lua
:caption: Fuzzy lookup of item templates using Items.FindByFuzzyName.
:emphasize-lines: 1,2,3
for _, match in ipairs(Items.FindByFuzzyName("Sowrd", 5)) do
    Util.LogInfo(string.format("%s (id %x, score %.2f)", match.Name, match.EntityId, match.Score))
end
```
