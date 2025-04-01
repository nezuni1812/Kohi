create role anon nologin;

grant usage on schema public to anon;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO anon;
GRANT INSERT, SELECT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO anon;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO anon;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT INSERT, SELECT, UPDATE, DELETE ON TABLES TO anon;

-- create role authenticator noinherit login password '1234';

-- grant anon to authenticator;