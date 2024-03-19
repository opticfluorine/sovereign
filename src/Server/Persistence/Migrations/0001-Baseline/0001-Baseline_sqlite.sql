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
    id INTEGER PRIMARY KEY NOT NULL
);


------------------------
-- Material Component --
------------------------

CREATE TABLE Material
(
    id       INTEGER PRIMARY KEY NOT NULL,
    material INTEGER             NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity (id)
);


--------------------------------
-- MaterialModifier Component --
--------------------------------

CREATE TABLE MaterialModifier
(
    id       INTEGER PRIMARY KEY NOT NULL,
    modifier INTEGER             NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity (id)
);


------------------------
-- Position Component --
------------------------

CREATE TABLE Position
(
    id INTEGER PRIMARY KEY NOT NULL,
    x  FLOAT               NOT NULL,
    y  FLOAT               NOT NULL,
    z  FLOAT               NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity (id)
);

-- Index the position coordinates.
CREATE INDEX Position_Xyz_Index ON Position (x, y, z);


-------------------------
-- PlayerCharacter Tag --
-------------------------

CREATE TABLE PlayerCharacter
(
    id      INTEGER PRIMARY KEY NOT NULL,
    value   BOOLEAN             NOT NULL,
    deleted BOOLEAN             NOT NULL DEFAULT FALSE,
    FOREIGN KEY (id) REFERENCES Entity (id)
);


--------------------
-- Name Component --
--------------------

CREATE TABLE Name
(
    id    INTEGER PRIMARY KEY NOT NULL,
    value TEXT                NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity (id)
);


-----------------------
-- Account Component --
-----------------------

-- Note non-standard table name to deconflict from the Account table
CREATE TABLE AccountComponent
(
    id         INTEGER PRIMARY KEY NOT NULL,
    account_id BLOB                NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity (id),
    FOREIGN KEY (account_id) REFERENCES Account (id)
);


----------------------
-- Parent Component --
----------------------

CREATE TABLE Parent
(
    id        INTEGER PRIMARY KEY NOT NULL,
    parent_id INTEGER             NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity (id),
    FOREIGN KEY (parent_id) REFERENCES Entity (id)
);


------------------
-- Drawable Tag --
------------------

CREATE TABLE Drawable
(
    id    INTEGER PRIMARY KEY NOT NULL,
    value BOOLEAN             NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity (id)
);


------------------------------
-- AnimatedSprite Component --
------------------------------

CREATE TABLE AnimatedSprite
(
    id    INTEGER PRIMARY KEY NOT NULL,
    value INTEGER             NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity (id)
);


---------------------------
-- Orientation Component --
---------------------------

CREATE TABLE Orientation
(
    id    INTEGER PRIMARY KEY NOT NULL,
    value INTEGER             NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity (id)
);

------------------
-- Drawable Tag --
------------------

CREATE TABLE Admin
(
    id    INTEGER PRIMARY KEY NOT NULL,
    value BOOLEAN             NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity (id)
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
SELECT Entity.id                   AS id,
       Position.x                  AS x,
       Position.y                  AS y,
       Position.z                  AS z,
       Material.material           AS material,
       MaterialModifier.modifier   AS materialModifier,
       PlayerCharacter.value       AS playerCharacter,
       Name.value                  AS name,
       AccountComponent.account_id AS account,
       Parent.parent_id            AS parent,
       Drawable.value              AS drawable,
       AnimatedSprite.value        AS animatedSprite,
       Orientation.value           AS orientation,
       Admin.value                 AS admin
FROM Entity
         LEFT JOIN Position ON Position.id = Entity.id
         LEFT JOIN Material ON Material.id = Entity.id
         LEFT JOIN MaterialModifier ON MaterialModifier.id = Entity.id
         LEFT JOIN PlayerCharacter ON Entity.id = PlayerCharacter.id
         LEFT JOIN Name ON Entity.id = Name.id
         LEFT JOIN AccountComponent ON Entity.id = AccountComponent.id
         LEFT JOIN Parent ON Entity.id = Parent.id
         LEFT JOIN Drawable ON Entity.id = Drawable.id
         LEFT JOIN AnimatedSprite ON Entity.id = AnimatedSprite.id
         LEFT JOIN Orientation ON Entity.id = Orientation.id
         LEFT JOIN Admin ON Entity.id = Admin.id;


-- Log the migration.
INSERT INTO MigrationLog
VALUES (1, 'Baseline');
