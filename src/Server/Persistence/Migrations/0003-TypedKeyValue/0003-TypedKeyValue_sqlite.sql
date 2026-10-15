-- Migration 0003: Typed key-value data.
-- Adds a per-value type tag to both key-value stores. Enum numbering is fixed:
-- String=0, Int=1, Bool=2, Float=3, Vector3=4, EntityId=5.

ALTER TABLE GlobalKeyValue
    ADD COLUMN type INTEGER NOT NULL DEFAULT 0;

ALTER TABLE EntityKeyValue
    ADD COLUMN type INTEGER NOT NULL DEFAULT 0;

-- Retype known keys. Clock time is non-negative and fits int64.
UPDATE GlobalKeyValue
SET type = 3 -- Float
WHERE key IN ('__Sovereign.Config.MinimumZ', '__Sovereign.Config.MaximumZ');

UPDATE GlobalKeyValue
SET type = 1 -- Int
WHERE key = '__Sovereign.Clock';

-- Log the migration.
INSERT INTO MigrationLog
VALUES (3, 'TypedKeyValue');
