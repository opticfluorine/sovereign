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
   (or `BaseTagCollection` for tags)
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
   :::{tip}
   Not all components are applicable to template entities. If the new component type
   is not applicable to template entities, ensure that the `IEntityBuilder`
   implementations check for `AbstractEntityBuilder`'s `isTemplate` field and avoid
   adding the component to template entities.
   :::

### Replicating Components Over the Network

:::{note}
This section applies only to common- and server-scoped components.
:::

Many common-scoped components need to be replicated from server to client as part
of entity synchronization. This may be done through the following steps:

1. Add a new nullable property to the end of `EntityDefinition` for the component
   value. Ensure that the `Key` attribute is correctly set to ensure that the
   field is properly serialized and deserialized. Note that tag properties are
   not nullable in this struct.
2. If necessary, update `EntityDefinitionValidator` with any validation logic
   required for the new component.
3. Update `EntityDefinitionProcessor` to call the appropriate `IEntityBuilder`
   methods based on the value of the new property you added to `EntityDefinition`.
4. Update `EntityDefinitionGenerator` to set the new property you added to
   `EntityDefinition` as needed.

### Displaying Component Values in Entity Debugger

:::{note}
This section applies only to common- and client-scoped components.
:::

Common-scoped and client-scoped component values should be displayed in the
client-side entity debugger windows. To do this, make the following changes in
the client:

1. In the `EntityDebugGui.Render()` method, add a new call to `AddComponentRow`
   for the new component.
2. If the component applies to player characters, update `PlayerDebugGui.Render()`
   to add a new call to `AddComponentRow` for the new component.

### Adding Components to Template Entity Editor

:::{note}
This section applies only to common-scoped components.
:::

#### Block Template Editor

For common-scoped components which apply to blocks and are persisted, the Block
Template Editor should be updated to allow editing of the component in block templates.
To do this, make the following changes in the client:

1. In the `BlockTemplateEditorTab` class, add a private field of the same type as the
   component value. Name the field `input{ComponentName}` where `{ComponentName}` is the
   name of the component. This field will store the value of the component while it is
   being edited.
2. In the `BlockTemplateEditorTab.RenderComponentTable()` method, determine if the component
   belongs to one of the existing categories (under a `ImGui.CollapsingHeader` call). If not,
   create a new category where the component editor will live along with a table to hold
   the fields belonging to the category. Follow the existing examples.
3. In the category, add a row for editing the component. Follow the existing examples.
   Use the field you added in step 1 as the `ref` argument to the input field.
4. In the `InsertNew()` method, set the default value for the component in new templates
   if desired.
5. In the `Save()` method, update the corresponding field in the `selectedDefinition` struct.
6. In the `Select(int)` method, update the input field from the `selectedDefinition` struct.

### Persisting Components in Database

:::{note}
This section applies only to common- and server-scoped components.
:::

Persisting a component in the database requires several changes:

1. Update the SQL migration scripts with a table for the new component as well as
   any required indices. Also update the `EntityWithComponents` view to add columns
   for the new component.
2. Update `IPersistenceProvider` with add/modify/remove queries for your new
   component.
3. Update `SqlitePersistenceProvider` to implement the methods you added to
   `IPersistenceProvider`. For simple (non-struct/class) types you can most likely
   use `SimpleSqliteAddComponentQuery` and `SimpleSqliteModifyComponentQuery`. For
   more complex types, check to see if a reusable query type already exists; otherwise
   you will need to create your own (see `Vector3SqliteAddComponentQuery` for an
   example).
4. Add a new state tracker derived from `BaseStateTracker` for the component to
   the `Sovereign.Persistence.State.Trackers` namespace. Add the tracker to the
   `StateTrackerInstaller` and `TrackerManager` classes as well.
5. Update `StateBuffer` to add a new `StructBuffer<StateUpdate<T>>` field to store
   state updates for the components, where `T` is the component value type. Add a
   new update method to `StateBuffer` to the component, then call this from your
   new state tracker. Also update the `StateBuffer.Reset()` method to call `Clear()`
   on the new `StructBuffer` field. Finally, update the 
   `DoSynchronize(IPersistenceProvider)` method to call `SynchronizeComponent` with
   the newly added `StructBuffer` and add/modify/delete queries.
6. Update `SqliteRetrieveEntityQuery` and `SqliteRetrieveRangeQuery` to retrieve
   the new component when fetching entities from the database. For components which
   may be held by template entities, also update `SqliteRetrieveAllTemplatesQuery`.
7. Update `EntityProcessor` to process the new component from the 
   `EntityWithComponents` by adding a new `ProcessX(IDataReader, IEntityBuilder)`
   method (where `X` is the component name) and calling it from the
   `ProcessSingleEntity(IDataReader)` method.

### Binding Components to Scripting Engine

:::{note}
This section applies only to common- and server-scoped components.
:::

For many components, it is useful to have a binding of the component collection to
the scripting engine so that scripts may read and write the component value. To do this,
simply add the `[ScriptableComponents]` attribute to the component collection class
with a single parameter corresponding to the name that will be assigned to the binding.
By standard Lua conventions, this name should be all lowercase. Refer to an existing
bound component collection (e.g. `NameComponentCollection`) for examples.
