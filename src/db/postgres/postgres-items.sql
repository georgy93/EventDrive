-- Runs once, when the postgres container starts with an empty data directory
-- (scripts in /docker-entrypoint-initdb.d), connected to POSTGRES_DB as POSTGRES_USER.

CREATE TABLE IF NOT EXISTS items
(
    id          integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    -- Unique, so the worker can skip items it has already inserted (ON CONFLICT DO NOTHING).
    -- 450 matches the API's validation and the SQL Server column.
    item_id     varchar(450) NOT NULL UNIQUE,
    item_name   text         NOT NULL,
    -- When the row was inserted, filled by the database.
    inserted_at timestamptz  NOT NULL DEFAULT now()
);
