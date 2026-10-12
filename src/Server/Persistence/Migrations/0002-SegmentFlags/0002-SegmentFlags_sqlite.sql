-- Migration 0002: World segment flags.
-- Adds per-segment bitwise flags (e.g. PVP allowed). See /segflag admin command (follow-up card).

CREATE TABLE WorldSegmentFlag
(
    x     INTEGER NOT NULL,
    y     INTEGER NOT NULL,
    z     INTEGER NOT NULL,
    flags INTEGER NOT NULL,
    PRIMARY KEY (x, y, z)
);

-- Default config constants.
INSERT OR IGNORE INTO GlobalKeyValue (key, value)
VALUES ('__Sovereign.Config.MaximumZ', '1024');

-- Log the migration.
INSERT INTO MigrationLog
VALUES (2, 'SegmentFlags');
