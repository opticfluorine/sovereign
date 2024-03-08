# Troubleshooting Common Issues in Development

The purpose of this document is to capture guidance for troubleshooting common types of issues that arise
during development.

## Inconsistent Behavior When Entities Unloaded

**Typical Symptoms:** Inconsistent behavior appears across a set of entities, particularly
when they are unloaded and possibly reloaded; for example, when unloading a set of blocks,
only a random set of blocks are removed.

**Possible Cause:** This is most often caused by filtering logic inconsistently dropping
remove/unload events in an event filter. For example, a filter that accepts only events from
block entities may only accept events for entities which have a Material component. When
entities are removed or unloaded, the component driving the filter may be removed before the
component triggering the event, and so the remove/unload events are inconsistently dropped.

**Solution:** Ensure that filtering logic is setting `lookback = true` in any component
retrieval. The lookback flag instructs the component collection to effectively ignore
removes/unloads from the same tick, removing the filter dependence on order of operations.
