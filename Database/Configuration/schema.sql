CREATE TABLE IF NOT EXISTS guild_settings
(
    guild_id           TEXT    NOT NULL PRIMARY KEY,
    guild_role_mode    TEXT    NOT NULL DEFAULT 'None',
    system_channel_id  TEXT    NOT NULL,
    welcome_channel_id TEXT             DEFAULT NULL,
    enabled            INTEGER NOT NULL DEFAULT 0,
    setup_complete     INTEGER NOT NULL DEFAULT 0,
    role_limit_warned  INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS user_roles
(
    user_id  TEXT NOT NULL,
    guild_id TEXT NOT NULL,
    role_id  TEXT DEFAULT NULL,

    PRIMARY KEY (user_id, guild_id),

    FOREIGN KEY (guild_id)
    REFERENCES guild_settings (guild_id)
    ON DELETE CASCADE
    );

CREATE TABLE IF NOT EXISTS tier_profiles
(
    user_id          TEXT    NOT NULL PRIMARY KEY,

    usual_tier       INTEGER NOT NULL,
    game_server      INTEGER NOT NULL,
    playstyle        INTEGER NOT NULL,

    primary_talent   INTEGER NOT NULL CHECK (primary_talent > 0),
    primary_isv      TEXT    NOT NULL,

    heal_talent      INTEGER CHECK (heal_talent IS NULL OR heal_talent > 0),
    heal_isv         TEXT,

    encore_talent    INTEGER CHECK (encore_talent IS NULL OR encore_talent > 0),
    encore_isv       TEXT,

    favorite_card_id INTEGER NOT NULL CHECK (favorite_card_id > 0),

    highest_tier     INTEGER,
    event_id         INTEGER CHECK (event_id IS NULL OR event_id > 0),

    timezone         TEXT NOT NULL,
    game_id          TEXT NOT NULL,

    CHECK (
(heal_talent IS NULL AND heal_isv IS NULL)
    OR
(heal_talent IS NOT NULL AND heal_isv IS NOT NULL)
    ),

    CHECK (
(encore_talent IS NULL AND encore_isv IS NULL)
    OR
(encore_talent IS NOT NULL AND encore_isv IS NOT NULL)
    )
    );

CREATE TABLE IF NOT EXISTS setup_roles
(
    guild_id TEXT NOT NULL,
    role_id  TEXT NOT NULL,

    PRIMARY KEY (guild_id, role_id),

    FOREIGN KEY (guild_id)
    REFERENCES guild_settings (guild_id)
    ON DELETE CASCADE
    );

CREATE TABLE IF NOT EXISTS setup_messages
(
    guild_id   TEXT NOT NULL,
    channel_id TEXT NOT NULL,
    message_id TEXT NOT NULL,

    PRIMARY KEY (guild_id, message_id),

    FOREIGN KEY (guild_id)
    REFERENCES guild_settings (guild_id)
    ON DELETE CASCADE
    );

CREATE TABLE IF NOT EXISTS schema_version
(
    id      INTEGER NOT NULL PRIMARY KEY CHECK (id = 1),
    version INTEGER NOT NULL
    );

CREATE INDEX IF NOT EXISTS idx_user_roles_guild_id
    ON user_roles (guild_id);