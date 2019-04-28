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


----------------------
-- Full Entity View --
----------------------

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
