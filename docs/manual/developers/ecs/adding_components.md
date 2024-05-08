# Adding Components

Adding components to Sovereign Engine is somewhat more involved as compared to other
well-known ECS frameworks due to the distributed and persisted nature of Sovereign's
ECS. This guide covers the basic steps of adding a new component type and optionally
persisting it in the database.

## Adding A New Component Type

The following steps must be performed when adding a new component to Sovereign:

1. Determine whether the component is client-side only, server-side only, or common
   to all. Most components are common to all unless there is a
   specific reason to limit the component to one or the other.
   :::{tip}
   Keep in mind that not all common and server-side components need to be persisted
   in the database. It is perfectly acceptable to have a component which is known to
   the server but only exists in memory. For example, the `Velocity` component is not
   persisted to the database. Therefore, do not consider the need for persistence when
   deciding the scope of a component.
   :::
2. Add a new component type to the appropriate section in the `ComponentType` enum.
3. Create a new component collection class derived from `BaseComponentCollection<T>`
   in the appropriate Components namespace as listed in the table below:
   | Scope  | Namespace                         |
   | ------ | --------------------------------- |
   | Common | `Sovereign.EngineCore.Components` |
   | Client | `Sovereign.ClientCore.Components` |
   | Server | `Sovereign.ServerCore.Components` |

   If the base type `T` of the component collection has not been used for another
   component before, you may need to add new component operators to the
   `ComponentOperators` class in order to dervice from `BaseComponentCollection<T>`.
4. Update `IEntityBuilder` with methods for adding and removing the new component
   from an entity. For common-scoped components, the new methods should be implemented
   in `AbstractEntityBuilder`, while client-only methods and server-only methods
   should be implemented in `ClientEntityBuilder` and `ServerEntityBuilder`
   respectively (with empty methods for out-of-scope builders). You will also need to
   update `ClientEntityFactory` and `ServerEntityFactory` to pass the component
   collection to the newly created builders as needed.
