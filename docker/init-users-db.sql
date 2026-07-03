SELECT 'CREATE DATABASE "MiKompri_Users"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'MiKompri_Users')\gexec
