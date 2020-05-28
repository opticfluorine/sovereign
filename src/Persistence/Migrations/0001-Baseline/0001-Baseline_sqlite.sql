--
-- Sovereign Engine
-- Copyright (c) 2020 opticfluorine
--
-- This program is free software: you can redistribute it and/or modify
-- it under the terms of the GNU Affero General Public License as
-- published by the Free Software Foundation, either version 3 of the
-- License, or (at your option) any later version.
--
-- This program is distributed in the hope that it will be useful,
-- but WITHOUT ANY WARRANTY; without even the implied warranty of
-- MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
-- GNU Affero General Public License for more details.
--
-- You should have received a copy of the GNU Affero General Public License
-- along with this program.  If not, see <https://www.gnu.org/licenses/>.
--

-------------------
-- Migration Log --
-------------------

CREATE TABLE MigrationLog
(
    id          INTEGER PRIMARY KEY,
	name        VARCHAR(255) NOT NULL
);

-------------------
-- Account Table --
-------------------
-- NOTE: Any changes to the Account schema must be documented in the
-- "Account Privacy" section of docs/accounts.md.

CREATE TABLE Account
(
    id              BLOB PRIMARY KEY NOT NULL,
	username        VARCHAR(255) NOT NULL
);

CREATE UNIQUE INDEX Account_Username_Index ON Account (username);


----------------------------
-- Account Authentication --
----------------------------
-- NOTE: Any changes to the Account_Authentication schema must be documented
-- in the "Account Privacy" section of docs/accounts.md.

CREATE TABLE Account_Authentication
(
    id             BLOB PRIMARY KEY NOT NULL,
	password_salt  BLOB NOT NULL,
	password_hash  BLOB NOT NULL,
	opslimit       INTEGER NOT NULL,
	memlimit       INTEGER NOT NULL,
	FOREIGN KEY (id) REFERENCES Account(id)
);


------------------
-- Entity Table --
------------------

CREATE TABLE Entity
(
    id          INTEGER PRIMARY KEY NOT NULL
);


------------------------
-- Material Component --
------------------------

CREATE TABLE Material
(
	id	        INTEGER PRIMARY KEY NOT NULL,
	material    INTEGER NOT NULL,
	FOREIGN KEY (id) REFERENCES Entity(id)
);


--------------------------------
-- MaterialModifier Component --
--------------------------------

CREATE TABLE MaterialModifier
(
    id          INTEGER PRIMARY KEY NOT NULL,
    modifier    INTEGER NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity(id)
);


------------------------
-- Position Component --
------------------------

CREATE TABLE Position
(
    id    INTEGER PRIMARY KEY NOT NULL,
	x     FLOAT NOT NULL,
	y     FLOAT NOT NULL,
	z     FLOAT NOT NULL,
	FOREIGN KEY (id) REFERENCES Entity(id)
);

-- Index the position coordinates.
CREATE INDEX Position_Xyz_Index ON Position (x, y, z);


--------------------------------------
-- Account With Authentication View --
--------------------------------------

CREATE VIEW AccountWithAuthentication AS
	SELECT Account.id AS id,
	       Account.username AS username,
	       Account_Authentication.password_salt AS salt,
		   Account_Authentication.password_hash AS hash,
		   Account_Authentication.opslimit AS opslimit,
		   Account_Authentication.memlimit AS memlimit
	FROM Account
	INNER JOIN Account_Authentication ON Account.id = Account_Authentication.id;


---------------------------------
-- Entity With Components View --
---------------------------------

CREATE VIEW EntityWithComponents AS
    SELECT Entity.id AS id,
           Position.x AS x,
           Position.y AS y,
           Position.z AS z,
           Material.material AS material,
           MaterialModifier.modifier AS materialModifier
    FROM Entity
    LEFT JOIN Position ON Position.id = Entity.id
    LEFT JOIN Material ON Material.id = Entity.id
    LEFT JOIN MaterialModifier ON MaterialModifier.id = Entity.id;


-- Log the migration.
INSERT INTO MigrationLog VALUES (1, 'Baseline');
