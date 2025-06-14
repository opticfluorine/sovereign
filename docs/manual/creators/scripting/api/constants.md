# Constants

This section describes constants and enumerations provided by the scripting engine.

:::{contents}
:local:
:depth: 2
:::

(script-constants-entitytype)=
## EntityType

```{eval-rst}
.. lua:class:: EntityType

    Enumeration that specifies the type of an entity.
    
    .. lua:attribute:: Npc: integer
    
        Entity is an NPC.
        
    .. lua:attribute:: Item: integer
    
        Entity is an item.
        
    .. lua:attribute:: Player: integer
    
        Entity is a player.
        
    .. lua:attribute:: Other: integer
    
        Entity has no special type. Not explicitly stored; only used in EntityDefinition.
```

(script-constants-orientation)=
## Orientation

```{eval-rst}
.. lua:class:: Orientation

    Enumeration that specifies a directional orientation.
    
    .. lua:attribute:: North: integer
    
        North.
        
    .. lua:attribute:: Northeast: integer
    
        Northeast.
        
    .. lua:attribute:: East: integer
    
        East.
        
    .. lua:attribute:: Southeast: integer
    
        Southeast.
        
    .. lua:attribute:: South: integer
    
        South.
        
    .. lua:attribute:: Southwest: integer
    
        Southwest.
        
    .. lua:attribute:: West: integer
    
        West.
        
    .. lua:attribute:: Northwest: integer
    
        Northwest.
```
