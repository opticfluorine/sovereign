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
    id          INTEGER PRIMARY KEY,
	name        VARCHAR(255) NOT NULL
);


-------------
-- Account --
-------------
-- NOTE: Any changes to the Account schema must be documented in the
-- "Account Privacy" section of docs/accounts.md.

CREATE TABLE Account
(
    id          BYTEA PRIMARY KEY NOT NULL,
	username    VARCHAR(255) UNIQUE NOT NULL
);

CREATE UNIQUE INDEX Account_Username_Index ON Account (username);


----------------------------
-- Account Authentication --
----------------------------
-- NOTE: Any changes to the Account_Authentication schema must be documented
-- in the "Account Privacy" section of docs/accounts.md.

CREATE TABLE Account_Authentication
(
	id             BYTEA PRIMARY KEY NOT NULL,
	password_salt  BYTEA NOT NULL,
	password_hash  BYTEA NOT NULL,
	opslimit       BIGINT NOT NULL,
	memlimit       BIGINT NOT NULL,
	FOREIGN KEY (id) REFERENCES Account(id) ON DELETE CASCADE
);


------------------
-- Entity Table --
------------------

CREATE TABLE Entity
(
    id          BIGINT PRIMARY KEY
);


------------------------
-- Material Component --
------------------------

CREATE TABLE Material
(
	id	        BIGINT PRIMARY KEY,
    material    INTEGER NOT NULL,
	FOREIGN KEY (id) REFERENCES Entity(id) ON DELETE CASCADE
);


--------------------------------
-- MaterialModifier Component --
--------------------------------

CREATE TABLE MaterialModifier
(
    id          BIGINT PRIMARY KEY,
    modifier    INTEGER NOT NULL,
    FOREIGN KEY (id) REFERENCES Entity(id) ON DELETE CASCADE
);


------------------------
-- Position Component --
------------------------

CREATE TABLE Position
(
    id    BIGINT PRIMARY KEY,
	x     REAL NOT NULL,
	y     REAL NOT NULL,
	z     REAL NOT NULL,
	FOREIGN KEY (id) REFERENCES Entity(id) ON DELETE CASCADE
);

-- Index the position coordinates.
CREATE INDEX Position_Xyz_Index ON Position (x, y, z);


-- Create views.

--------------------------------------
-- Account With Authentication View --
--------------------------------------

CREATE VIEW AccountWithAuthentication AS
	SELECT Account.id AS id,
		   Account.username AS username,
		   Account_Authentication.salt AS salt,
		   Account_Authentication.hash AS hash,
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


-- Create stored procedures and functions.

--
-- EntityDetails(entityId)
-- Retrieves all components for the given entity.
--
CREATE FUNCTION EntityDetails (entityId BIGINT)
RETURNS TABLE 
(
    entityId BIGINT,
	pos_x REAL,
	pos_y REAL,
	pos_z REAL,
	material INTEGER,
	materialModifier INTEGER
)
LANGUAGE SQL
AS $$
  SELECT *
  FROM EntityWithComponents
  WHERE id = entityId;
$$;

--
-- PositionedEntitiesInRange([x,y,z]_min, [x,y,z]_max)
-- Retrieves all entities and components that are positioned within
-- the given range (inclusive min, exclusive max).
--
CREATE FUNCTION PositionedEntitiesInRange
	(x_min REAL, y_min REAL, z_min REAL,
	 x_max REAL, y_max REAL, z_max REAL)
RETURNS TABLE
(
	entityId BIGINT,
	pos_x REAL,
	pos_y REAL,
	pos_z REAL,
	material INTEGER,
	materialModifier INTEGER
)
LANGUAGE SQL
AS $$
  SELECT *
  FROM EntityWithComponents
  WHERE x >= x_min AND x < x_max AND
	    y >= y_min AND y < y_max AND
		z >= z_min AND z < z_max;
$$;

--
-- NextAvailablePersistedId()
-- Gets the next available persisted entity ID.
--
CREATE FUNCTION NextAvailablePersistedId() RETURNS BIGINT
LANGUAGE SQL
AS $$
	SELECT MAX(used_ids.id) + 1 FROM 
		(SELECT MAX(id) AS id FROM Entity          -- take greatest used id
		 UNION SELECT x'7ffeffffffffffff'::bigint) -- or (first persisted id - 1) otherwise.
		 AS used_ids;
$$;

-- Log the migration.
INSERT INTO MigrationLog VALUES (1, 'Baseline');

