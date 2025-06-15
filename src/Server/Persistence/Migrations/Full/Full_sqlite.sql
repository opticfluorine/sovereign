--
-- Sovereign Engine
-- Copyright (c) 2019 opticfluorine
--
-- Permission is hereby granted, free of charge, to any person obtaining a 
-- copy of this software and associated documentation files (the "Software"), 
-- to deal in the Software without restriction, including without limitation 
-- the rights to use, copy, modify, merge, publish, distribute, sublicense, 
-- and/or sell copies of the Software, and to permit persons to whom the 
-- Software is furnished to do so, subject to the following conditions:
--
-- The above copyright notice and this permission notice shall be included in
-- all copies or substantial portions of the Software.
--
-- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
-- IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
-- FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
-- AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
-- LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
-- FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
-- DEALINGS IN THE SOFTWARE.
--

-------------------
-- Migration Log --
-------------------

CREATE TABLE MigrationLog
(
    id   INTEGER PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

-------------------
-- Account Table --
-------------------
-- NOTE: Any changes to the Account schema must be documented in the
-- "Account Privacy" section of docs/accounts.md.

CREATE TABLE Account
(
    id       BLOB PRIMARY KEY NOT NULL,
    username VARCHAR(255)     NOT NULL
);

CREATE UNIQUE INDEX Account_Username_Index ON Account (username);


----------------------------
-- Account Authentication --
----------------------------
-- NOTE: Any changes to the Account_Authentication schema must be documented
-- in the "Account Privacy" section of docs/accounts.md.

CREATE TABLE Account_Authentication
(
    id            BLOB PRIMARY KEY NOT NULL,
    password_salt BLOB             NOT NULL,
    password_hash BLOB             NOT NULL,
    opslimit      INTEGER          NOT NULL,
    memlimit      INTEGER          NOT NULL,
    FOREIGN KEY (id) REFERENCES Account (id)
);


------------------
-- Entity Table --
------------------

CREATE TABLE Entity
(
    id                  INTEGER PRIMARY KEY NOT NULL,
    template_id         INTEGER,
    material            INTEGER,
    material_mod        INTEGER,
    pos_x               FLOAT,
    pos_y               FLOAT,
    pos_z               FLOAT,
    player_char         BOOLEAN,
    player_char_deleted BOOLEAN             NOT NULL DEFAULT (FALSE),
    name                TEXT,
    account_id          BLOB,
    parent_id           INTEGER,
    drawable            BOOLEAN,
    animated_sprite     INTEGER,
    orientation         INTEGER,
    admin               BOOLEAN,
    cast_block_shadows  BOOLEAN,
    pls_radius          FLOAT,
    pls_intensity       FLOAT,
    pls_color           INTEGER,
    pls_pos_x           FLOAT,
    pls_pos_y           FLOAT,
    pls_pos_z           FLOAT,
    physics             BOOLEAN,
    bb_pos_x            FLOAT,
    bb_pos_y            FLOAT,
    bb_pos_z            FLOAT,
    bb_size_x           FLOAT,
    bb_size_y           FLOAT,
    bb_size_z           FLOAT,
    shadow_radius       FLOAT,
    entity_type         INTEGER,
    FOREIGN KEY (template_id) REFERENCES Entity (id),
    FOREIGN KEY (parent_id) REFERENCES Entity (id),
    FOREIGN KEY (account_id) REFERENCES Account (id)
);

CREATE INDEX Entity_PC ON Entity (player_char);
CREATE INDEX Entity_Pos ON Entity (pos_x, pos_y, pos_z);


------------------------------
-- World Segment Block Data --
------------------------------

CREATE TABLE WorldSegmentBlockData
(
    x    INTEGER NOT NULL,
    y    INTEGER NOT NULL,
    z    INTEGER NOT NULL,
    data BLOB    NOT NULL,
    PRIMARY KEY (x, y, z)
);


----------------------------
-- Global Key-Value Store --
----------------------------

CREATE TABLE GlobalKeyValue
(
    key   TEXT PRIMARY KEY NOT NULL,
    value TEXT             NOT NULL
);


----------------------------
-- Entity Key-Value Store --
----------------------------

CREATE TABLE EntityKeyValue
(
    entity_id INTEGER PRIMARY KEY NOT NULL,
    key       TEXT                NOT NULL,
    value     TEXT                NOT NULL,
    FOREIGN KEY (entity_id) REFERENCES Entity (id)
);


--------------------------------------
-- Account With Authentication View --
--------------------------------------

CREATE VIEW AccountWithAuthentication AS
SELECT Account.id                           AS id,
       Account.username                     AS username,
       Account_Authentication.password_salt AS salt,
       Account_Authentication.password_hash AS hash,
       Account_Authentication.opslimit      AS opslimit,
       Account_Authentication.memlimit      AS memlimit
FROM Account
         INNER JOIN Account_Authentication ON Account.id = Account_Authentication.id;


---------------------------------
-- Entity With Components View --
---------------------------------

CREATE VIEW EntityWithComponents AS
SELECT Entity.id                 AS id,
       Entity.template_id        AS template_id,
       Entity.pos_x              AS x,
       Entity.pos_y              AS y,
       Entity.pos_z              AS z,
       Entity.material           AS material,
       Entity.material_mod       AS materialModifier,
       Entity.player_char        AS playerCharacter,
       Entity.name               AS name,
       Entity.account_id         AS account,
       Entity.parent_id          AS parent,
       Entity.drawable           AS drawable,
       Entity.animated_sprite    AS animatedSprite,
       Entity.orientation        AS orientation,
       Entity.admin              AS admin,
       Entity.cast_block_shadows AS castBlockShadows,
       Entity.pls_radius         AS plsRadius,
       Entity.pls_intensity      AS plsIntensity,
       Entity.pls_color          AS plsColor,
       Entity.pls_pos_x          AS plsPosX,
       Entity.pls_pos_y          AS plsPosY,
       Entity.pls_pos_z          AS plsPosZ,
       Entity.physics            AS physics,
       Entity.bb_pos_x           AS bbPosX,
       Entity.bb_pos_y           AS bbPosY,
       Entity.bb_pos_z           AS bbPosZ,
       Entity.bb_size_x          AS bbSizeX,
       Entity.bb_size_y          AS bbSizeY,
       Entity.bb_size_z          AS bbSizeZ,
       Entity.shadow_radius      AS shadowRadius,
       Entity.entity_type        AS entityType
FROM Entity;


--------------------------------------
-- Starter Data - Template Entities --
--------------------------------------

-- Grass block template entity.
INSERT INTO Entity (id, name, material, material_mod, drawable, cast_block_shadows)
VALUES (0x7FFE000000000000, 'Grass', 1, 0, 1, 1);

-- Water block template entity.
INSERT INTO Entity (id, name, material, material_mod, drawable)
VALUES (0x7FFE000000000001, 'Water', 2, 0, 1);

-- GrassRock block template entity (currently unused).
INSERT INTO Entity (id, name, material, material_mod, drawable, cast_block_shadows)
VALUES (0x7FFE000000000002, 'GrassRock', 1, 0, 1, 1);

-- Dirt block template entity.
INSERT INTO Entity (id, name, material, material_mod, drawable, cast_block_shadows)
VALUES (0x7FFE000000000003, 'Dirt', 3, 0, 1, 1);

-- Sand block template entity.
INSERT INTO Entity (id, name, material, material_mod, drawable, cast_block_shadows)
VALUES (0x7FFE000000000004, 'Sand', 4, 0, 1, 1);

-- Rock block template entity.
INSERT INTO Entity (id, name, material, material_mod, drawable, cast_block_shadows)
VALUES (0x7FFE000000000005, 'Rock', 5, 0, 1, 1);

-- Cobblestone block template entity.
INSERT INTO Entity (id, name, material, material_mod, drawable, cast_block_shadows)
VALUES (0x7FFE000000000006, 'Cobblestone', 6, 0, 1, 1);

-- Initial block data at origin.
INSERT INTO WorldSegmentBlockData
VALUES (0, 0, 0,
        X'92dc0020920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100920100919200929200929200920000920192000092019292019200009200920000');
INSERT INTO WorldSegmentBlockData
VALUES (-1, 0, 0,
        X'92dc002092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010091920092920091921f920000920191921f920000');
INSERT INTO WorldSegmentBlockData
VALUES (-1, -1, 0,
        X'92dc002092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010091920091921f91921f920000');
INSERT INTO WorldSegmentBlockData
VALUES (0, -1, 0,
        X'92dc002092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010092010091920091921f9292009200009201920000');

-- Log the migration.
INSERT INTO MigrationLog
VALUES (1, 'Baseline');

-- Enable WAL.
PRAGMA
journal_mode= WAL;
