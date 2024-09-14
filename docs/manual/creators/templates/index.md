# Template Entities

Most entities are not unique - every block of grass shares most of the same components, every
NPC of a given type, every item of a given type, etc. **Template Entities** allow you to create
a *template* for a given type of entity.

## Creating Types of Entities Using Templates

A template entity is an entity which specifies a set of component values to *inherit* as default
values by any entity which inherits the template. For example, a grass block template entity would
specify the `Material` and `MaterialModifier` components corresponding to a grass material; every
block of grass would then inherit this template and have the grass material by default.

When an entity inherits a template entity, it takes on the same component values as the template
by default. If a component is specified for both the entity and its inherited template, the
value assigned to the entity takes effect over the template value.

## Defining Template Entities

Template entities can be defined using the integrated **Template Entity Editor** that is built
into the Sovereign Engine client. To open the editor, first log into the server as a player with
the Admin role, then press the `Ins` key. The Template Entity Editor should be displayed.

### Creating and Modifying Block Template Entities

**Block Template Entities** are template entities which specify the properties of a block entity.
They can be created and modified through the Template Entity Editor using the *Blocks* tab.

![Block Template Editor](images/block_template_editor.png)

The left-hand side of the editor displays a list of all Block Template Entities known to the server.
New Block Template Entities may be created by clicking the *+* button below the list; this will
create a new Block Template Entity on the server with default values.

Block Template Entities may be opened for editing by clicking them in the left-hand list. The actively
selected Block Template Entity is highlighted in the list, and its editable properties are displayed
in drop-down categories to the right. The following components may be modified for a Block Template
Entity:

| Component         | Purpose                                               | Default Value         |
| ----------------- | ----------------------------------------------------- | --------------------- |
| Name              | Name for the Block Template Entity. Should be unique. | *(blank)*             |
| Drawable          | Whether the block can be drawn. Usually true.         | Checked               |
| Material/Modifier | Material and modifier for the block.                  | Material 1/Modifier 0 |

Clicking the *Save* button at the bottom of the editor will send the updated values to the server, and
the updated Block Template Entity will be sent to all connected clients. Clicking the *Cancel* button
will revert any local changes; the selected Block Template Entity will then match the definition from
the server.

Block Template Entities may not be deleted. This is by design to prevent blocks from referencing a
template that no longer exists. If a template is no longer needed, the recommended best practice is
to deprecate the template by changing its Name component to an appropriate value (e.g. *Deprecated*).
