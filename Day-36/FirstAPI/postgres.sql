CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    city TEXT
);
—————————
CREATE OR REPLACE FUNCTION process_csv(csv_input text)
RETURNS SETOF text
LANGUAGE plpgsql
AS $$
DECLARE
    row text;
    name text;
    email text;
    city text;
    errors text[];
    line_number integer := 0;
BEGIN
    -- Split CSV into lines
    FOREACH row IN ARRAY string_to_array(csv_input, E'\n')
    LOOP
        line_number := line_number + 1;


        IF line_number = 1 THEN
            CONTINUE;
        END IF;

        BEGIN
            -- Split values
            name := split_part(row, ',', 1);
            email := split_part(row, ',', 2);
            city := split_part(row, ',', 3);

            -- Validate
            IF name IS NULL OR email IS NULL THEN
                errors := array_append(errors, 'Missing fields (line ' || line_number || '): ' || row);
                CONTINUE;
            END IF;

            -- Insert
            INSERT INTO users(name, email, city)
            VALUES (name, email, city);

        EXCEPTION WHEN OTHERS THEN
            errors := array_append(errors, 'DB error (line ' || line_number || '): ' || row);
        END;
    END LOOP;

    RETURN QUERY SELECT unnest(errors);
END;
$$;