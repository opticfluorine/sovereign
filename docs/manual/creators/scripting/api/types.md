(script-types)=
# Data Types

The scripting engine uses Lua tables with specific entries to communicate
information through function calls. This section defines the table structures
to be used in scripts.

:::{contents}
:local:
:depth: 2
:::

(script-types-boundingbox)=
## BoundingBox

```{eval-rst}
.. lua:class:: BoundingBox

    Bounding box used for physics calculations.

    .. lua:attribute:: Position: [#script-types-vector3](Vector3)```

        The position relative to the upper-top-left corner of the entity at which
        the bounding box starts.

    .. lua:attribute:: Size

        The size of the bounding box.
```

(script-types-entityeventdetails)=
## EntityEventDetails

```{eval-rst}
.. lua:class:: EntityEventDetails

    Event details type containing a single entity ID.
    
    .. lua:attribute:: EntityId: integer
    
        The entity ID related to the event.
```

(script-types-gridposition)=
## GridPosition

```{eval-rst}
.. lua:class:: GridPosition

    Integer-valued 3D vector type.
   
    .. lua:attribute:: X: integer
   
        The X value of the vector.
      
    .. lua:attribute:: Y: integer
   
        The Y value of the vector.
      
    .. lua:attribute:: Z: integer
    
        The Z value of the vector.
```

(script-types-kinematics)=
## Kinematics

```{eval-rst}
.. lua:class:: Kinematics

    Describes the position and velocity of an entity.
    
    .. lua:attribute:: Position: Vector3
    
        Entity position.
    
    .. lua:attribute:: Velocity: Vector3
    
        Entity velocity.
```

(script-types-pointlight)=
## PointLight

```{eval-rst}
.. lua:class:: PointLight

    Specifies a point light source attached to an entity.
    
    .. lua:attribute:: Radius: number
    
        Radius of the light source as a multiple of the block size.
        
    .. lua:attribute:: Intensity: number
    
        Intensity of the light source. Larger is brighter.
        
    .. lua:attribute:: Color: integer
    
        RGB-packed color of the light source.
        
    .. lua:attribute:: PositionOffset: Vector3
    
        Offset of the light source relative to the entity position, specified as a percentage
        of the sprite size. For example, an offset of (0.5, 0.5, 0.0) positions the light source
        in the center of the entity's XY plane.
```

(script-types-vector3)=
## Vector3

```{eval-rst}
.. lua:class:: Vector3

    3D vector type.
   
    .. lua:attribute:: X: number
   
        The X value of the vector.
      
    .. lua:attribute:: Y: number
   
        The Y value of the vector.
      
    .. lua:attribute:: Z: number
    
        The Z value of the vector.
```
