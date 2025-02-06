# Physics 

Sovereign Engine performs physics updates on both the client and the server
with the server acting as the authoritative source. This allows the client
to predict the motion of entities in realtime without regard to network latency,
then synchronize to the authoritative updates from the server when they are
received. This section describes the design and function of the physics engine.

## Roles of the Physics Engine

The physics engine fulfills several gameplay needs for Sovereign:

* **Collision Detection:** The physics engine detects collisions between entities
  and adjusts the position and velocity of the entities to exclude their overlap.
* **Gravity:** The physics engine applies a gravitational correction to entities which
  are not supported from below by a block, applying a constant downward acceleration
  up to a terminal velocity.
* **Jumping:** The physics engine applies the upward impulse to an entity performing
  a jump.
* **Movement:** The physics engine updates the position of moving entities based on
  their current velocity.

These four major functions must be implemented in a way that is scalable on the server
to large numbers of entities. In particular, they must be implemented in a way that
only performs processing for entities on which there will be a physics effect in a given
tick (or, at a minimum, entities for which an effect is highly likely). For example,
it would not be scalable to implement gravity by applying a constant acceleration to
all entities and immediately cancelling the effect through collision detection. Instead,
the physics engine must only apply the gravitational acceleration to entities which are
highly unlikely to be standing on a solid block.

## Movement System Behaviors

### Gravity

Physics entities which are flagged for active gravity have their velocity modified
according to a constant acceleration up to a terminal velocity:

```{math}
\mathbf{v'} = \mathbf{v} + \mathbf{a_g} \Delta t
```

### Jumping

A jump is modeled as an instantaneous impulse in the {math}`+z` direction in the
first tick of the jump, imparting a positive upward velocity to the entity which
eventually returns to zero by means of gravity and collision. A jump event
therefore sets the {math}`z` component of velocity to a constant {math}`v_j` and
sets an active gravity flag on the entity.

### Position Update

An initial position update is made without regard to collision by propagating the
entity position forward according to

```{math}
\mathbf{x'} = \mathbf{x} + \mathbf{v} \Delta t
```

### Collision Detection

Collision detection is done via 3D axis-aligned bounding box (AABB) collision testing
with bounding boxes aligned to the block grid. This is generally one of the fastest
available collision detection algorithms. The physics engine further assumes that the
velocity of a moving entity is small enough that an entity will not clip through an
obstacle during a single tick, therefore continuous collision detection is not required.

Each non-block physics entity has an assigned AABB range specified through its 
`BoundingBox` component. This is specified by a pair of vectors: a displacement vector 
{math}`\mathbf{d}` of the top-left corner of the bounding box relative to the entity 
position, and a range vector {math}`\mathbf{r}` specifying the length of each side of the
bounding box. Note that the {math}`y` component of both vectors is in world coordinates,
therefore the components typically have a negative sign.

Each block entity has an implicit AABB range with {math}`\mathbf{d}=\mathbf{0}` and
{math}`\mathbf{r}=\mathbf{1}`; that is, all block entities are completely covered by
their own bounding box.

Collisions are checked for all non-block physics entities that have a non-zero velocity.
After computing the new position {math}`\mathbf{x'}`, the
entity has an absolute AABB range with upper-top-left corner 
{math}`\mathbf{a_0}=\mathbf{x'}+\mathbf{d}`
and lower-bottom-right corner {math}`\mathbf{a_1}=\mathbf{a_0}+\mathbf{r}`. This range
is tested against every other AABB range from any world segment overlapped by this range.
An overlap indicates a collision between the two entities. Note that this means that for
pairs of moving physics entities, the result will depend on the order in which the
entities are processed (i.e. collision updates are sequential); this simplifies the
scalability concerns for the collision detection.

If a collision is detected, the movement of the entity must then be unwound to exclude
the overlap between entities. Collision resolution is accomplished by finding the nearest 
point along the moving entity's velocity vector that removes the overlap. This 
calculation is done independently for each position component which has an overlap.
The corresponding velocity component is next set to zero. If the {math}`z` component is zeroed,
the active gravity flag is removed if present.
Collision detection is then repeated for the moving entity to ensure that the unwinding did
not produce a new overlap with another physics entity.

## Implementation Details

### General

#### Identification of Physics Entities

Non-block physics entities are identified by holding the `Physics` tag 
(`PhysicsTagCollection`). This will most often be set through the template entity.
It is invalid for an entity to hold the `Physics` tag but not a `Kinematics` and
`BoundingBox` component.

All block entities are implicitly physics entities. Block entities therefore may not
hold the `Physics` tag. They are a special case of physics entity in that they do not
undergo movement processing and do not possess `Kinematics` or `BoundingBox` components.
Their role in the physics engine is limited to acting as obstacles during collision
detection with a moving non-block physics entity.

### Gravity

Active gravity flagging is maintained locally by `MovementSystem` and applies only to
non-block physics entities.

[TBD: What are the ways in which entities acquire an active gravity flag?]

### Jump Events

[TBD: What is the event sequence to trigger a jump?]

### Position Update

Position updates are processed for the entire `KinematicsComponentCollection` at once.
Any components with zero velocity are skipped for position update and the following
collision detection step.

### Collision Detection

#### Non-Block Obstacles

[TBD: What is the strategy for non-block obstacles? This is a very large number of
potential targets. Should they be merged by a greedy algorithm into xy slabs to facilitate
faster checking? How will this be implemented?]

