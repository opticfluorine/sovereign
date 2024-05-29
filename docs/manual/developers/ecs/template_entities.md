# Template Entities

Few entities in a typical game world are truly unique. Most are identical to, or slight
variations of, a number of basic entity types (e.g. "block of grass", "player", etc.).
Reproducing tens of thousands of copies of the components for these entities would be
a waste of resources. **Template entities** avoid repeated duplication of component data
by allowing an entity to inherit a default set of components from a template unless they
are overridden by custom values.

## Implementation of Template Entities

Template entities are persisted in the database with entity IDs between
`0xFFFE000000000000` through `0xFFFEFFFFFFFFFFFF`. Entities may inherit from a template
entity by setting the `template_id` column in the `Entity` table; in the engine, this
results in the inheritance being tracked in `EntityTable` and implemented through
`BaseComponentCollection<T>`.

Template entities are transparent to code which relies on the component collections 
inheriting `BaseComponentCollection<T>`. When component values are read from the
collection, the inherited component value will be read if the entity does not have
its own value for that component. Writing a component value to an entity overrides the
component for that entity.

## Template Entity Loading

Template entities are loaded by the server at startup from the database. The entire
collection of template entities are maintained in server memory at all times. When
changes are made to these templates, those changes are synchronized to the database
during the standard persistence synchronization process.

The client loads the latest template entities when an account first logs in
by requesting a full template table via an authenticated REST endpoint on the server. 
This table consists of a LZ4-compressed list of entity definition structures encoded 
using MessagePack. It is parsed and the definitions are processed by 
`EntityDefinitionProcessor` to synchronize the template table in client memory.

## Template Entity Creation

Template entity creation and modification is done through a request-reply process that
involves passing entity definitions through the normal entity synchronization methods.
Special client-to-server events (`Server_TemplateEntity_Update`)
make changes to the template entity table on the server. Whenever updates occur on the
server, they are relayed to all connected clients through the standard entity
synchronization mechanism. 

Below is a sequence diagram showing a high-level depiction of the creation and update 
processes.

:::{mermaid}
sequenceDiagram
    participant Client
    participant Server
    Client ->> Client: Admin creates or modifies template in client
    Client ->> Server: UpdateTemplateEntity
    Server ->> Client: EntitySync (Updated Template)
:::

Template entities cannot be deleted once created. This is an intentional choice to
reduce the risk of deleting a critical template entity that is widely used across
the game world.

## Template Entity Component Support

Not all components are applicable to template entities. The matrix below provides
a summary of which components are applicable to which types of template entities.
Any component not listed is not supported in any template entities.

| Component          | Blocks |
| ------------------ | ------ |
| `AnimatedSprite`   |        |
| `Drawable`         | X      |
| `Material`         | X      |
| `MaterialModifier` | X      |
| `Name`             | X      |
| `Orientation`      |        |
