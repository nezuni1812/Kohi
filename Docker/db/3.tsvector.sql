-- Script to add tsvector columns and triggers for full-text search
DO $$
DECLARE
    table_rec RECORD;
    col_rec RECORD;
    text_columns TEXT;
    trigger_func_name TEXT;
    trigger_name TEXT;
BEGIN
    RAISE NOTICE 'Starting tsvector column setup for all tables';

    -- Loop through all tables in the public schema
    FOR table_rec IN (
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
        AND table_type = 'BASE TABLE'
    ) LOOP
        RAISE NOTICE 'Processing table: %', table_rec.table_name;

        -- Add search_vector column if it doesn't exist
        IF NOT EXISTS (
            SELECT 1
            FROM information_schema.columns
            WHERE table_schema = 'public'
            AND table_name = table_rec.table_name
            AND column_name = 'search_vector'
        ) THEN
            EXECUTE format('ALTER TABLE %I ADD COLUMN search_vector tsvector;', table_rec.table_name);
            RAISE NOTICE 'Added search_vector column to table: %', table_rec.table_name;
        END IF;

        -- Initialize text_columns
        text_columns := NULL;

        -- Get list of text/varchar columns for the table
        FOR col_rec IN (
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
            AND table_name = table_rec.table_name
            AND data_type IN ('text', 'character varying')
        ) LOOP
            RAISE NOTICE 'Found text/varchar column % in table %', col_rec.column_name, table_rec.table_name;
            IF text_columns IS NULL THEN
                text_columns := format('to_tsvector(''english'', COALESCE(NEW.%I, ''''))', col_rec.column_name);
            ELSE
                text_columns := text_columns || ' || ' || format('to_tsvector(''english'', COALESCE(NEW.%I, ''''))', col_rec.column_name);
            END IF;
        END LOOP;

        -- Create trigger function only if there are text/varchar columns
        IF text_columns IS NOT NULL THEN
            trigger_func_name := table_rec.table_name || '_search_vector_update';
            trigger_name := table_rec.table_name || '_search_vector_trigger';

            -- Drop existing trigger and function
            EXECUTE format('DROP TRIGGER IF EXISTS %I ON %I;', trigger_name, table_rec.table_name);
            EXECUTE format('DROP FUNCTION IF EXISTS %I();', trigger_func_name);
            RAISE NOTICE 'Dropped existing trigger/function for table: %', table_rec.table_name;

            -- Create trigger function
            EXECUTE format('
                CREATE FUNCTION %I() RETURNS trigger AS $func$
                BEGIN
                    NEW.search_vector := %s;
                    RETURN NEW;
                END;
                $func$ LANGUAGE plpgsql;
            ', trigger_func_name, text_columns);
            RAISE NOTICE 'Created trigger function % for table %', trigger_func_name, table_rec.table_name;

            -- Create trigger
            EXECUTE format('
                CREATE TRIGGER %I
                BEFORE INSERT OR UPDATE ON %I
                FOR EACH ROW EXECUTE FUNCTION %I();
            ', trigger_name, table_rec.table_name, trigger_func_name);
            RAISE NOTICE 'Created trigger % for table %', trigger_name, table_rec.table_name;

            -- Create GIN index
            EXECUTE format('
                CREATE INDEX IF NOT EXISTS %I_search_vector_idx ON %I USING GIN(search_vector);
            ', table_rec.table_name, table_rec.table_name);
            RAISE NOTICE 'Created GIN index for table: %', table_rec.table_name;
        ELSE
            RAISE NOTICE 'No text or varchar columns found in table %', table_rec.table_name;
        END IF;
    END LOOP;

    RAISE NOTICE 'Completed tsvector column setup';
END $$;

-- Function to refresh tsvector columns for existing data
CREATE OR REPLACE FUNCTION refresh_search_vectors() RETURNS void AS $$
DECLARE
    table_rec RECORD;
    col_rec RECORD;
    text_columns TEXT;
BEGIN
    RAISE NOTICE 'Starting refresh of search vectors';
    FOR table_rec IN (
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
        AND table_type = 'BASE TABLE'
    ) LOOP
        IF EXISTS (
            SELECT 1
            FROM information_schema.columns
            WHERE table_schema = 'public'
            AND table_name = table_rec.table_name
            AND column_name = 'search_vector'
        ) THEN
            text_columns := NULL;
            FOR col_rec IN (
                SELECT column_name
                FROM information_schema.columns
                WHERE table_schema = 'public'
                AND table_name = table_rec.table_name
                AND data_type IN ('text', 'character varying')
            ) LOOP
                RAISE NOTICE 'Refreshing column % in table %', col_rec.column_name, table_rec.table_name;
                IF text_columns IS NULL THEN
                    text_columns := format('to_tsvector(''english'', COALESCE(%I, ''''))', col_rec.column_name);
                ELSE
                    text_columns := text_columns || ' || ' || format('to_tsvector(''english'', COALESCE(%I, ''''))', col_rec.column_name);
                END IF;
            END LOOP;

            IF text_columns IS NOT NULL THEN
                EXECUTE format('
                    UPDATE %I SET search_vector = %s;
                ', table_rec.table_name, text_columns);
                RAISE NOTICE 'Refreshed search_vector for table %', table_rec.table_name;
            ELSE
                RAISE NOTICE 'No text or varchar columns to update in table %', table_rec.table_name;
            END IF;
        END IF;
    END LOOP;
    RAISE NOTICE 'Completed refresh of search vectors';
END;
$$ LANGUAGE plpgsql;