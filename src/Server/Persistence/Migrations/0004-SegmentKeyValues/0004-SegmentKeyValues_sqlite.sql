-- Migration 0004: World segment key-value data.
-- Adds per-segment typed key-value metadata. The type tag uses the DataValueType
-- numbering: String=0, Int=1, Bool=2, Float=3, Vector3=4, EntityId=5.
--
-- The primary key (x, y, z, key) also serves per-segment loads: its leftmost prefix
-- (x, y, z) covers the WHERE clause, so no separate index is needed. World segments
-- are not entities, so no foreign key is declared.

CREATE TABLE WorldSegmentKeyValue
(
    x     INTEGER NOT NULL,
    y     INTEGER NOT NULL,
    z     INTEGER NOT NULL,
    key   TEXT    NOT NULL,
    value TEXT    NOT NULL,
    type  INTEGER NOT NULL,
    PRIMARY KEY (x, y, z, key)
);

-- Log the migration.
INSERT INTO MigrationLog
VALUES (4, 'SegmentKeyValues');
